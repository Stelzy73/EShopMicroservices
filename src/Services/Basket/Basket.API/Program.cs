using BuildingBlocks.Exceptions.Handler;

var builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

// Carter (Minimal API)
builder.Services.AddCarter();

// MediatR (CQRS Pattern, Behaviors)
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);

// Marten (PostgreSQL Document DB)
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database")!);
    // Indentify the document by UserName property
    options.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

// Basket Repository
builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// Exceptions
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

#endregion


var app = builder.Build();


#region Configure the HTTP request pipeline.

app.MapCarter();

app.UseExceptionHandler(options => { });

#endregion


app.Run();
 