using System;

namespace Tacta.EventStore.DependencyInjection
{
    public interface IEventNameToTypeConverter
    {
        Type GetType(string eventName);
    }
}