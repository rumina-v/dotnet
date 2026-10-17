using UserRegistration.Api.Services;

using Npgsql;
using UserRegistration.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddProblemDetails();
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
await DatabaseInitializer.InitializeAsync(app.Services.GetRequiredService<NpgsqlDataSource>());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
