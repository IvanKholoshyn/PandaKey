using PandaKey.Api.Data;
using PandaKey.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<ZonesRepository>();
builder.Services.AddScoped<AccessEventsRepository>();

builder.Services.AddScoped<AccessDecisionRepository>();
builder.Services.AddScoped<PandaKey.Api.Services.AccessDecisionService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
