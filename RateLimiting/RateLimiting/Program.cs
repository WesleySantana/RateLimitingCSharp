using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRateLimiter(options => 
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("Fixed", config =>
    {      
        config.PermitLimit = 100; // Número máximo de requisições por janela
        config.Window = TimeSpan.FromSeconds(30);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 2;
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
