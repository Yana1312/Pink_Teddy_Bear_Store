using Microsoft.EntityFrameworkCore;
using StoreTeddyBear.Models;

internal class Program
{
    public static StorepinkteddybearBdContext context { get; } = new StorepinkteddybearBdContext();
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add this line to register your DbContext
        builder.Services.AddDbContext<StorepinkteddybearBdContext>(options =>
            options.UseMySql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 23)) // Specify your MySQL server version
            ));

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

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