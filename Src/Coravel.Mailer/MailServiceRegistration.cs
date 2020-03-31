using System;
using System.Collections.Generic;
using System.Net.Security;
using Coravel.Mailer.Mail;
using Coravel.Mailer.Mail.Interfaces;
using Coravel.Mailer.Mail.Mailers;
using Coravel.Mailer.Mail.Renderers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;

namespace Coravel
{
    /// <summary>
    /// IServiceCollection extensions for registering Coravel's Mailer.
    /// </summary>
    public static class MailServiceRegistration
    {
        /// <summary>
        /// Register Coravel's mailer using the IConfiguration to provide all configuration details.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static IServiceCollection AddMailer(this IServiceCollection services, IConfiguration config)
        {
            string mailerType = config.GetValue<string>("Coravel:Mail:Driver", "FileLog");

            var strategies = new Dictionary<string, Action>();
            strategies.Add("SMTP", () => AddSmtpMailer(services, config));
            strategies.Add("FILELOG", () => AddFileLogMailer(services, config));
            strategies[mailerType.ToUpper()].Invoke();
            return services;
        }

        /// <summary>
        /// Register Coravel's mailer using the File Log Mailer - which sends mail to a file.
        /// Useful for testing.
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddFileLogMailer(this IServiceCollection services, IConfiguration config)
        {
            var globalFrom = GetGlobalFromRecipient(config);
            services.AddSingleton<IMailer>(p =>
                new FileLogMailer(p.GetRequiredService<RazorRenderer>(), globalFrom));
            RegisterRazorRenderer(services);
            return services;
        }

        /// <summary>
        /// Register Coravel's mailer using the Smtp Mailer.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="certCallback"></param>
        public static IServiceCollection AddSmtpMailer(this IServiceCollection services, IConfiguration config, RemoteCertificateValidationCallback certCallback)
        {
            var globalFrom = GetGlobalFromRecipient(config);

            services.AddSingleton<IMailer>(p =>
                new SmtpMailer(
                    p.GetRequiredService<RazorRenderer>(),
                    config.GetValue<string>("Coravel:Mail:Host", ""),
                    config.GetValue<int>("Coravel:Mail:Port", 0),
                    config.GetValue<string>("Coravel:Mail:Username", null),
                    config.GetValue<string>("Coravel:Mail:Password", null),
                    globalFrom,
                    certCallback)
            );
            RegisterRazorRenderer(services);
            return services;
        }

        /// <summary>
        /// Register Coravel's mailer using the Smtp Mailer.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static IServiceCollection AddSmtpMailer(this IServiceCollection services, IConfiguration config)
        {
            AddSmtpMailer(services, config, null);
            return services;
        }

        /// <summary>
        /// Register Coravel's mailer using the Custom Mailer.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sendMailAsync"></param>
        public static IServiceCollection AddCustomMailer(this IServiceCollection services, IConfiguration config, CustomMailer.SendAsyncFunc sendMailAsync)
        {
            var globalFrom = GetGlobalFromRecipient(config);
            services.AddSingleton<IMailer>(p => 
                new CustomMailer(p.GetRequiredService<RazorRenderer>(), sendMailAsync, globalFrom));
            RegisterRazorRenderer(services);
            return services;
        }

        private static MailRecipient GetGlobalFromRecipient(IConfiguration config)
        {
            string globalFromAddress = config.GetValue<string>("Coravel:Mail:From:Address", null);
            string globalFromName = config.GetValue<string>("Coravel:Mail:From:Name", null);

            if (globalFromAddress != null)
            {
                return new MailRecipient(globalFromAddress, globalFromName);
            }
            else
            {
                return null;
            }
        }

        private static void RegisterRazorRenderer(IServiceCollection services)
        {
            services.AddSingleton<IRazorLightEngine>(p => {
                return new RazorLightEngineBuilder()
                .UseFileSystemProject(AppDomain.CurrentDomain.BaseDirectory)
                .UseMemoryCachingProvider()
                .Build();
            });
            services.AddScoped<RazorRenderer>();
        }
    }
}