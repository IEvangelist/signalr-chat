using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using BlazingChatter.Server;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build()
    .RunAsync();