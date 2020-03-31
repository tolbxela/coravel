using System;
using Coravel.Mailer.Mail.Renderers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RazorLight;

namespace MailerUnitTests.Mail.Shared
{
    public static class Helpers
    {
        public static RazorRenderer GetRenderer()
        {
            var razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(AppDomain.CurrentDomain.BaseDirectory)
                .UseMemoryCachingProvider()
                .Build();
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var config = new ConfigurationBuilder().Build();
            return new RazorRenderer(provider, config, razorEngine);
        }
        
    }
}