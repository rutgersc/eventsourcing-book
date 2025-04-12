using System.Collections.Immutable;
using Deciders;
using EventsourcingBook.Domain.Carts;
using EventsourcingBook.Infra;
using Microsoft.EntityFrameworkCore;

// Setup services
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOpenApi()
    .AddDbContextPool<AppStateDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();
app.MapOpenApi();

// Application
var carts = ImmutableDictionary<CartId, CartState>.Empty;

var cartDispatch = CartDecider.Decider.ConfigureState(
    loadState: async (id, initialState) =>
    {
        return carts.GetValueOrDefault(id, initialState);
    },
    saveState: async (id, state) =>
    {
        carts = carts.Add(id, state);
        return id;
    });

var resultToHttpResponse = (CartId cartId, Result<IReadOnlyCollection<CartEvent>, CartError> result) =>
{
    return result.Switch(
        ok: _ => Results.Ok(cartId),
        error: err => Results.BadRequest(err.ToString()));
};


app.Run();
