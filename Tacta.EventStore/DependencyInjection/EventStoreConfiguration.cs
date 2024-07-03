
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.DependencyInjection
{
    public static class EventStoreConfiguration
    {
        public static IServiceCollection AddEventStoreRepository(this IServiceCollection services, Assembly[] assemblies)
        {
            GuardAgainstEventsWithSameName(assemblies);
            
            var domainEvents = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IDomainEvent).IsAssignableFrom(t))
                .ToDictionary(t => t.Name);

            AddEventStoreRepository(services, domainEvents);

            return services;
        }
        
        public static IServiceCollection AddEventStoreRepository(this IServiceCollection services, IDictionary<string, Type> domainEvents)
        {
            services.AddTransient<IEventStoreRepository, EventStoreRepository>();
            services.AddSingleton<IEventNameToTypeConverter>(new EventNameToTypeConverter(domainEvents));

            return services;
        }

        private static void GuardAgainstEventsWithSameName(Assembly[] assemblies)
        {
            var domainEvents = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IDomainEvent).IsAssignableFrom(t))
                .Select(t => (Name: t.Name, Description: t.AssemblyQualifiedName))
                .ToList();
                
            var duplicates = domainEvents
                .GroupBy(tuple => tuple.Name)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Any())
            {
                var formattedDuplicates = new StringBuilder();
                var duplicateDomainEvents = duplicates.SelectMany(d => domainEvents.Where(de => de.Name == d.Key));
                foreach (var duplicateDomainEvent in duplicateDomainEvents)
                    formattedDuplicates.AppendLine(duplicateDomainEvent.Description);
                    
                throw new ArgumentException("Every event has to have a unique name! Following events collide:" +
                                            $"{Environment.NewLine}{formattedDuplicates}");
            }
        }
    }
}