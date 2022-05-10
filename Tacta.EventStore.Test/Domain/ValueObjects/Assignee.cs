using System.Collections.Generic;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Test.Domain.ValueObjects
{
    public sealed class Assignee : ValueObject
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public decimal FirstAmount { get; }
        public decimal SecondAmount { get; }

        public Assignee(string name, string displayName, decimal firstAmount, decimal secondAmount)
        {
            Name = name;
            DisplayName = displayName;
            FirstAmount = firstAmount;
            SecondAmount = secondAmount;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
            yield return DisplayName;
            yield return FirstAmount;
            yield return SecondAmount;
        }
    }
}
