using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace IEvangelist.SignalR.Chat
{
    public class Program
    {
        public static void Main(string[] args) 
            => CreateWebHostBuilder(args).Build().Run();

        static IWebHostBuilder CreateWebHostBuilder(string[] args) 
            => WebHost.CreateDefaultBuilder(args)
                      .UseStartup<Startup>();
    }
}