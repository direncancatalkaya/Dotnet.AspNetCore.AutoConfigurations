using AutoConfigurations.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace AutoConfigurations.Extensions
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddAutoConfigurations(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var methodinfo = typeof(OptionsConfigurationServiceCollectionExtensions).GetMethods()
                .Where(m => m.Name == "Configure").Single(m => m.GetParameters().Length == 2);

            var configurationsTypes = GetConfigurationClasses();
            var optionsType = typeof(IOptions<>);

            foreach (var configType in configurationsTypes)
            {
                methodinfo
                    .MakeGenericMethod(configType)
                    .Invoke(services, new object[] {services, config.GetSection(configType.GetCustomAttribute<AutoConfigAttribute>().SectionName ?? configType.Name)});

                services.AddSingleton(configType, cfg =>
                {
                    var optionsInstance = cfg.GetService(optionsType.MakeGenericType(configType));
                    var reflectedType = optionsInstance.GetType();
                    var propInfo = reflectedType.GetProperty("Value");

                    return propInfo.GetValue(optionsInstance);
                });
            }

            return services;
        }

        private static List<Type> GetConfigurationClasses()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetCustomAttribute<AutoConfigAttribute>() != null).ToList();
        }
    }
}