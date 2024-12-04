using MongoDB.Bson;
using MongoDB.Driver;
using WebApplicationZyfra.BLL.Repository;
using WebApplicationZyfra.BLL.Services;
using WebApplicationZyfra.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Регистрация MongoClient с чтением строки подключения из конфигурации
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

// Регистрация MongoDbContext и остальных зависимостей
builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())

app.UseSwagger();
app.UseSwaggerUI();




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
