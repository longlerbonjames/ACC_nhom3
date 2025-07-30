using Microsoft.EntityFrameworkCore;
using TravelTourCrawler.Data;
using TravelTourCrawler.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // 🆕 THÊM dòng này nếu chưa có

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

            // ✅ CẤU HÌNH DÙNG MySQL (không phải SQL Server)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 36)); // Đổi đúng version MySQL

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, serverVersion));

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
        }
    }
}
