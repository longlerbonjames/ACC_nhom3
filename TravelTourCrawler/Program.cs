using Microsoft.EntityFrameworkCore;
using TravelTourCrawler.Data;
using TravelTourCrawler.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace TravelTourCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ITourCrawler, OTripCrawler>();

            builder.Services.AddHttpClient<OTripCrawler>(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            });

            // ✅ CẤU HÌNH CORS CHO REACT (http://localhost:5173)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // ✅ ĐÚNG PORT VITE
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // ✅ CẤU HÌNH DÙNG MySQL
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, serverVersion));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // ✅ BẬT CORS TRƯỚC Authorization
            app.UseCors("AllowReactApp");

            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
