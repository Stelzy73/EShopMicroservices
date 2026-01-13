
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
// Möglich dank Scrutor
// Das Decorator Pattern erlaubt es, zusätzliche Funktionalität zu einem bestehenden Objekt hinzuzufügen, ohne dessen Interface zu ändern:
/*
 Das Decorator Pattern mag auf den ersten Blick "komplizierter" aussehen, aber es bietet:
   •	Saubere Trennung der Verantwortlichkeiten
   •	Hohe Flexibilität in der Konfiguration
   •	Bessere Testbarkeit
   •	Einfache Erweiterbarkeit
   •	Wartbarkeit durch klare Struktur
   In größeren Anwendungen zahlt sich diese Struktur definitiv aus, auch wenn sie anfangs etwas mehr Code bedeutet.   
 */
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")!;
});

// Exceptions
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

// Health Checks
builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("Database")!);
builder.Services.AddHealthChecks().AddRedis(builder.Configuration.GetConnectionString("Redis")!);

#endregion

var app = builder.Build();

#region Configure the HTTP request pipeline.

// Carter (Minimal API)
app.MapCarter();

// Exception Handling
app.UseExceptionHandler(options => { });

// Health Checks
app.UseHealthChecks("/health", new HealthCheckOptions
{
    // Für JSON-Ausgabe im HealthCheck-UI-Format
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

#endregion


app.Run();
 