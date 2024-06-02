using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SegalAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SegalAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                        {
                            serverOptions.Listen(IPAddress.Loopback, 5001); // Listen on port 5001 for localhost
                        }
                        else
                        {
                            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
                            serverOptions.Listen(IPAddress.Any, Convert.ToInt32(port));
                        }
                    })
                    .UseStartup<Startup>();
                });
    }
}
