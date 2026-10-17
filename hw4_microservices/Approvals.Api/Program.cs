using Approvals.Api.Data;
using Approvals.Api.Logging;
using Approvals.Api.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
});
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Approvals");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Задайте ConnectionStrings__Approvals через окружение.");
    }
    return NpgsqlDataSource.Create(connectionString);
});
builder.Services.AddScoped<RequestRepository>();
builder.Services.AddScoped<ApprovalService>();

var app = builder.Build();
app.UseExceptionHandler();
try
{
    await RequestRepository.InitializeAsync(app.Services.GetRequiredService<NpgsqlDataSource>());
    app.Logger.LogInformation(4000, "Сервис согласования заявок готов.");
}
catch (Exception exception)
{
    app.Logger.LogError(4005, "Не удалось запустить сервис заявок. Тип: {ExceptionType}.", exception.GetType().Name);
    await app.DisposeAsync();
    Environment.ExitCode = 1;
    return;
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.MapControllers();
app.Run();
