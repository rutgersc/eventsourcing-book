using Deciders;
using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Domain.Carts.ReadModels;
using EventsourcingBook.Domain.Inventories;
using EventsourcingBook.Domain.Products;
using EventsourcingBook.Infra;
using EventsourcingBook.Infra.Carts;
using EventsourcingBook.Infra.Carts.EventStore;
using EventStore.Client;
using EventsourcingBook.Infra.Inventories;
using EventsourcingBook.Infra.Inventories.EventStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using CartDispatch = System.Func<EventsourcingBook.Domain.Carts.CartId, EventsourcingBook.Domain.Carts.CartCommand, System.Threading.Tasks.Task<System.Result<System.Collections.Generic.IReadOnlyCollection<EventsourcingBook.Domain.Carts.CartEvent>, EventsourcingBook.Domain.Carts.CartError>>>;
using InventoryDispatch = System.Func<EventsourcingBook.Domain.Products.ProductId, EventsourcingBook.Domain.Inventories.InventoryCommand, System.Threading.Tasks.Task<System.Result<System.Collections.Generic.IReadOnlyCollection<EventsourcingBook.Domain.Inventories.InventoryEvent>, EventsourcingBook.Domain.Inventories.InventoryError>>>;
using PricingDispatch = System.Func<EventsourcingBook.Domain.Products.ProductId, EventsourcingBook.Domain.Inventories.PricingCommand, System.Threading.Tasks.Task<System.Result<System.Collections.Generic.IReadOnlyCollection<EventsourcingBook.Domain.Inventories.PricingEvent>, EventsourcingBook.Domain.Inventories.PricingError>>>;

// Setup services
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOpenApi()
    .AddDbContextPool<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();
app.MapOpenApi();

var eventStoreDbClient = new EventStoreClient(EventStoreClientSettings.Create("esdb://localhost:2113?tls=false"));
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

// Application
var runApplicationEventSourced = false;
var cartStateStored = CartDecider.Decider.ConfigureState(new CartStateEntityFrameworkRepository(dbContext));
var cartEventStored = CartDecider.Decider.ConfigureEventPersistence(new CartEventsRepository(eventStoreDbClient));

var inventoryEventStored = InventoryDecider.Decider.ConfigureEventPersistence(new InventoryEventsRepository(eventStoreDbClient));
var inventoryStateStored = InventoryDecider.Decider.ConfigureState(new InventoryStateEntityFrameworkRepository(dbContext));

var pricingEventStored = PricingDecider.Decider.ConfigureEventPersistence(new PricingEventsRepository(eventStoreDbClient));
var pricingStateStored = PricingDecider.Decider.ConfigureState(new PricingStateEntityFrameworkRepository(dbContext));

CartDispatch cartDispatch = runApplicationEventSourced
    ? cartEventStored.ExecuteCommand
    : cartStateStored.ExecuteCommand;

InventoryDispatch inventoryDispatch = runApplicationEventSourced
    ? inventoryEventStored.ExecuteCommand
    : inventoryStateStored.ExecuteCommand;

 PricingDispatch pricingDispatch = runApplicationEventSourced
    ? pricingEventStored.ExecuteCommand
    : pricingStateStored.ExecuteCommand;

await inventoryEventStored.Persistence.Subscribe(async (productId, inventoryEvent) =>
{
    await InventoriesReadModelProjector.Project(dbContext, productId, inventoryEvent);
});

await cartEventStored.Persistence.Subscribe(async (cartId, inventoryEvent) =>
{
    await CartsWithProductsReadModelProjector.Project(dbContext, cartId, inventoryEvent);
});

await pricingEventStored.Persistence.Subscribe(async (productId, @event) =>
{
    await ArchiveItemAutomationProcessor.React(dbContext, cartEventStored, productId, @event);
});

IResult ResultToHttpResponse<TId, TEvent, TError>(TId id, Result<IReadOnlyCollection<TEvent>, TError> result) where TError : notnull
{
    return result.Switch(
        ok: _ => Results.Ok(id),
        error: err => Results.BadRequest(err.ToString()));
}

app.MapPost(
        "/additem",
        async ([FromBody] AddItemPayload item) =>
        {
            var id = new CartId(Guid.NewGuid());
            var result = await cartDispatch(id, item.ToCommand());
            return ResultToHttpResponse(id, result);
        })
    .WithName("AddNewCartItem");

app.MapPost(
    "/additem/{cartId}",
    async (CartId cartId, [FromBody] AddItemPayload item) =>
    {
        var result = await cartDispatch(cartId, item.ToCommand());
        return ResultToHttpResponse(cartId, result);;
    })
    .WithName("AddCartItem");

app.MapGet("/{cartId}/cartitems",
    async (CartId cartId) =>
    {
        if (runApplicationEventSourced)
        {
            return await cartEventStored.ReadStateView(CartItemsReadModel.StateView, cartId);
        }
        else
        {
            // State-stored decider does not persist the events, some the alternatives are
            // 1) In-memory subscribe & persist the ReadModel as the events happen
            // 2) Create a mapping from (CartState -> CartItemsReadModel) like we do with CRUD
            // 3) Persist the events/outbox (why not do eventsourcing then)
            return CartItemsReadModel.StateView.InitialState;
        }
    });

app.MapPost("removeitem/{cartId}",
    async (CartId cartId, [FromBody] RemoveItemPayload item) =>
    {
        var result = await cartDispatch(cartId, item.ToCommand());
        return ResultToHttpResponse(cartId, result);
    });

app.MapPost("clearcart/{cartId}",
    async (CartId cartId) =>
    {
        var result = await cartDispatch(cartId, new CartCommand.ClearCartCommand());
        return ResultToHttpResponse(cartId, result);
    });

app.MapPost("simulate-inventory-external-event-translation",
    async ([FromBody] ExternalInventoryChangedEvent externalInventoryChangedEvent) =>
    {
        // external gets translated to internal a command/event
        var productId = new ProductId(externalInventoryChangedEvent.ProductId);
        var result = await inventoryDispatch(productId, externalInventoryChangedEvent.ToCommand());
        return ResultToHttpResponse(productId, result);
    });

app.MapGet("/inventories/{productId:Guid}",
    async (Guid productId) =>
    {
        if (runApplicationEventSourced)
        {
            return await dbContext.InventoriesReadModel
                .FirstOrDefaultAsync(i => i.ProductId == productId);
        }
        else
        {
            // With state-stored systems, the read-model is usually coupled to the same table as the write side.
            return await dbContext.Inventories
                .Where(i => i.ProductId == productId)
                .Select(i => new InventoriesReadModelEntity { ProductId = i.ProductId, Inventory = i.Inventory })
                .FirstOrDefaultAsync();
        }
    });

app.MapPost("simulate-external-event-translation-price",
    async ([FromBody] ExternalInventoryPriceChangedEvent priceChangedEvent) =>
    {
        // external gets translated to internal a command/event
        var productId = new ProductId(priceChangedEvent.ProductId);
        var result = await pricingEventStored.ExecuteCommand(productId, priceChangedEvent.ToCommand());

        return ResultToHttpResponse(productId, result);
    });

app.MapPost("/changeprice/{productId}",
    async (ProductId productId, [FromBody] ChangePricePayload payload) =>
    {
        var result = await pricingDispatch(productId, payload.ToCommand());
        return ResultToHttpResponse(productId, result);
    });

app.MapGet("/cartwithproducts/{productId}",
    async (ProductId productId) =>
    {
        if (runApplicationEventSourced)
        {
            var cartsWithProducts = await dbContext.CartsWithProducts
                .Where(entity => entity.ProductId == productId.Value)
                .ToListAsync();

            return new CartsWithProductsReadModel(cartsWithProducts);
        }
        else
        {
            var cartsWithProducts  = await dbContext.Carts
                .Where(entity => entity.CartItems.Any(c => c.ProductId == productId.Value))
                .Select(entity => new CartsWithProductsReadModelEntity
                {
                    CartId = entity.CartId,
                    ProductId = entity.CartItems.Select(c => c.ProductId).First()
                })
                .ToListAsync();

            return new CartsWithProductsReadModel(cartsWithProducts);
        }
    });

app.MapPost("/archiveitem/{cartId}",
    async (CartId cartId, [FromBody] ArchivePayload payload) =>
    {
        var result = await cartDispatch(cartId, payload.ToCommand());
        return ResultToHttpResponse(cartId, result);
    });

app.Run();

record AddItemPayload(
    string description,
    string image,
    decimal price,
    Guid itemId,
    Guid productId)
{
    public CartCommand.AddItemCommand ToCommand()
    {
        var item = this;
        return new CartCommand.AddItemCommand(
            Description: item.description,
            Image: item.image,
            Price: item.price,
            ItemId: item.itemId,
            ProductId: item.productId);
    }
}

record RemoveItemPayload(
    Guid itemId)
{
    public CartCommand.RemoveItemCommand ToCommand()
    {
        return new CartCommand.RemoveItemCommand(
            ItemId: new CartItemId(this.itemId));
    }
}

record ExternalInventoryChangedEvent(Guid ProductId, int Inventory)
{
    public InventoryCommand.ChangeInventoryCommand ToCommand() =>
        new(this.Inventory);
};

record ExternalInventoryPriceChangedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice)
{
    public PricingCommand.ChangePriceCommand ToCommand() => new (this.OldPrice, this.NewPrice);
}

record ChangePricePayload(
    decimal OldPrice,
    decimal NewPrice)
{
    public PricingCommand.ChangePriceCommand ToCommand() => new(this.OldPrice, this.NewPrice);
}

record CartsWithProductsReadModel(List<CartsWithProductsReadModelEntity> Data);

record ArchivePayload(Guid ProductId)
{
    public CartCommand.ArchiveItemCommand ToCommand() => new(new ProductId(ProductId));
}
