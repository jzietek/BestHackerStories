
using BestHackerStories.Api.Controllers;
using BestHackerStories.Api.Extensions;
using BestHackerStories.Service;

namespace BestHackerStories.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient();
        builder.Services.AddTransient<IBestStoriesService, BestStoriesService>();

        var app = builder.Build();

        var logger = app.Services.GetService<ILogger<BestStoriesController>>();        
        app.ConfigureExceptionHandler(logger);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

