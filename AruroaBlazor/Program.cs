using AruroaBlazor.Components;
using DBL;

namespace AruroaBlazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add HttpClient for API calls
            builder.Services.AddHttpClient();

            // Register application services
            builder.Services.AddScoped<Services.PlaylistService>();
            builder.Services.AddScoped<Services.UserAdminService>();

            // Load connection string from appsettings.json and set it in DB class
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString) == false)
            {
                DB.SetConnectionString(connectionString);
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
