using Grocery.Shopping.API.Application.Interfaces;
using Grocery.Shopping.API.Application.Services;
using Grocery.Shopping.API.Domain.Services;
using Grocery.Shopping.API.Infra.Mongo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<IReconhecimentoFotoService, ReconhecimentoFotoService>();
builder.Services.AddHttpClient<IEstoqueService, EstoqueService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Mongo settings
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Mongo"));

// Contexto Mongo
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
