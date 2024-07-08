using System;
using System.Collections.Generic;

namespace Tacta.EventStore.DependencyInjection
{
    public class EventNameToTypeConverter : IEventNameToTypeConverter
    {
        private readonly IDictionary<string, Type> _eventTypeLookup;

        public EventNameToTypeConverter(IDictionary<string, Type> eventTypeLookup)
        {
            _eventTypeLookup = eventTypeLookup;
        }
        
        public Type GetType(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException($"Event Name cannot be empty or NULL!");
            }

            if (_eventTypeLookup.ContainsKey(eventName) == false)
            {
                throw new ArgumentException($"No Event registered for name: '{eventName}'");
            }
            
            var type = _eventTypeLookup[eventName];

            return type;
        }
    }
}