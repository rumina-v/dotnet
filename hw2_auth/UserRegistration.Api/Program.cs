using System.Diagnostics;
using Npgsql;
using UserRegistration.Api.Data;
using UserRegistration.Api.Logging;
using UserRegistration.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    var defaultFactory = options.InvalidModelStateResponseFactory;
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(LogEvents.ValidationFailed,
            "Запрос отклонён валидацией. Ошибок: {ErrorCount}. TraceId: {TraceId}.",
            context.ModelState.ErrorCount, Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
        return defaultFactory(context);
    };
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Users");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Задайте ConnectionStrings__Users через окружение или ConnectionStrings:Users через user secrets.");
    }
    return NpgsqlDataSource.Create(connectionString);
});
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
try
{
    await DatabaseInitializer.InitializeAsync(app.Services.GetRequiredService<NpgsqlDataSource>());
    app.Logger.LogInformation(LogEvents.DatabaseReady, "Подключение к PostgreSQL готово.");
}
catch (Exception exception)
{
    app.Logger.LogError(LogEvents.StartupFailed,
        "Не удалось запустить API. Тип ошибки: {ExceptionType}. Проверьте PostgreSQL и ConnectionStrings:Users.",
        exception.GetType().Name);
    await app.DisposeAsync();
    Environment.ExitCode = 1;
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
