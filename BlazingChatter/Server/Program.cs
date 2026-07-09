using System.Net.Mime;
using BlazingChatter.Hubs;
using BlazingChatter.Server.Extensions;
using Microsoft.AspNetCore.ResponseCompression;

const string CorsPolicy = nameof(CorsPolicy);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddResponseCompression(
    options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        [MediaTypeNames.Application.Octet]));

builder.Services.AddAppAuthentication(builder.Environment);
builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddCors(
    options => options.AddPolicy(
        CorsPolicy,
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));

builder.Services.AddSignalR(options => options.EnableDetailedErrors = true)
    .AddMessagePackProtocol();

var app = builder.Build();

app.UseResponseCompression();

app.MapDefaultEndpoints();

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chat");

app.Run();