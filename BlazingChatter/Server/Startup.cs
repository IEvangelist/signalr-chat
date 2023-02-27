using System.Net.Mime;
using BlazingChatter.Hubs;
using BlazingChatter.Server.Extensions;
using Microsoft.AspNetCore.ResponseCompression;

namespace BlazingChatter.Server;

public class Startup
{
    const string CorsPolicy = nameof(CorsPolicy);

    readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) => _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddResponseCompression(
            options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { MediaTypeNames.Application.Octet }));

        services.AddAppAuthentication(_configuration);
        services.AddAppServices(_configuration);
        services.AddCors(options =>
        {
            options.AddPolicy(
                name: CorsPolicy,
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddSignalR(options => options.EnableDetailedErrors = true)
                .AddMessagePackProtocol();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseResponseCompression();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors(CorsPolicy);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHub<ChatHub>("/chat");
            endpoints.MapFallbackToFile("index.html");
        });
    }
}
