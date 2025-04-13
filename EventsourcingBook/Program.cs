using Deciders;
using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Domain.Carts.ReadModels;
using EventsourcingBook.Infra;
using EventsourcingBook.Infra.Carts;
using EventsourcingBook.Infra.Carts.EventStore;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using CartDispatch = System.Func<EventsourcingBook.Domain.Carts.CartId, EventsourcingBook.Domain.Carts.CartCommand, System.Threading.Tasks.Task<System.Result<System.Collections.Generic.IReadOnlyCollection<EventsourcingBook.Domain.Carts.CartEvent>, EventsourcingBook.Domain.Carts.CartError>>>;

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
var runApplicationEventSourced = true;
var cartStateStored = CartDecider.Decider.ConfigureState(new CartStateEntityFrameworkRepository(dbContext));
var cartEventStored = CartDecider.Decider.ConfigureEventPersistence(new CartEventsRepository(eventStoreDbClient));

CartDispatch cartDispatch = runApplicationEventSourced ? cartEventStored.ExecuteCommand : cartStateStored.ExecuteCommand;

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
            ItemId: this.itemId);
    }
}

