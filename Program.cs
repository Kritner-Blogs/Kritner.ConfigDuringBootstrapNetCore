using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kritner.ConfigDuringBootstrapNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var (env, configurationRoot, myWebsiteConfig) = BootstrapConfigurationRoot();

            CreateHostBuilder(args, env, configurationRoot, myWebsiteConfig).Build().Run();
        }

        private static (string env, IConfigurationRoot configurationRoot, MyWebsiteConfig myWebsiteConfig) BootstrapConfigurationRoot()
        {
            var env = GetEnvironmentName();
            var tempConfigBuilder = new ConfigurationBuilder();

            tempConfigBuilder
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: false);

            var configurationRoot = tempConfigBuilder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<MyWebsiteConfig>(configurationRoot.GetSection(nameof(MyWebsiteConfig)));
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var myWebsiteConfig = serviceProvider.GetService<IOptions<MyWebsiteConfig>>().Value;
            return (env, configurationRoot, myWebsiteConfig);
        }

        private static string GetEnvironmentName()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env))
            {
                throw new Exception("ASPNETCORE_ENVIRONMENT env variable not set.");
            }

            return env;
        }

        private static IHostBuilder CreateHostBuilder(string[] args, string env, IConfigurationRoot configurationRoot,
            MyWebsiteConfig myWebsiteConfig) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    context.HostingEnvironment.EnvironmentName = env;
                    builder.AddConfiguration(configurationRoot);
                })
                .ConfigureLogging(builder => { builder.AddConsole(); })
                .ConfigureServices((hostContext, services) =>
                {
                    // Add some services
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseUrls($"http://*:{myWebsiteConfig.Port}");
                    builder.UseStartup<Startup>();
                });
    }
}