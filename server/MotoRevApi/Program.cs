using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

//Configurar o Banco de Dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.Run();
