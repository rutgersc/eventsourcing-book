using EventsourcingBook.Infra;

// Setup services
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Configure
var app = builder.Build();
app.MapOpenApi();
app.UseHttpsRedirection();

app.Run();
