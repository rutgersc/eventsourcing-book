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

IResult ResultToHttpResponse<TId, TEvent, TError>(TId id, Result<IReadOnlyCollection<TEvent>, TError> result) where TError : notnull
{
    return result.Switch(
        ok: _ => Results.Ok(id),
        error: err => Results.BadRequest(err.ToString()));
}

app.Run();
