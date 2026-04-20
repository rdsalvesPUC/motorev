using Microsoft.EntityFrameworkCore;
using MotoRevApi.Data;
using MotoRevApi.Profiles;
using MotoRevApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Configurar o Banco de Dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddScoped<MotoService>();
builder.Services.AddScoped<ModeloMotoService>();
builder.Services.AddEndpointsApiExplorer();

// Configurar AutoMapper
builder.Services.AddAutoMapper( cfg =>
    cfg.AddProfile<MappingProfile>()
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Ativa a interface visual do Scalar em /scalar
    app.MapScalarApiReference();
}

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
