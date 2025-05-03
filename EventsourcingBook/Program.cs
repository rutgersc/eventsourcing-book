using Deciders;
using EventsourcingBook.Domain.Carts;
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
    "/additem/{cartId}",
    async (CartId cartId, [FromBody] AddItemPayload item) =>
    {
        var result = await cartDispatch(cartId, item.ToCommand());
        return ResultToHttpResponse(cartId, result);;
    })
    .WithName("AddCartItem");

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

