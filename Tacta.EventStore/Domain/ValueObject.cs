using System.Collections.Generic;
using System.Linq;

namespace Tacta.EventStore.Domain
{
    /// <summary>
    /// ValueObject represents value object tactical DDD pattern.
    /// Main properties of value objects is their immutability
    /// and structural equality (two value objects are equal if
    /// their properties are equal) 
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        ///     This is needed as salt for index. If only index was used, there is a chance that i ^ i+some_low_number produces
        ///     same value
        ///     Issue is shown in following fiddle: https://dotnetfiddle.net/E3tgYY
        /// </summary>
        private const int HighPrime = 557927;

        /// <summary>
        ///     Override GetAtomicValues in order to implement structural equality for your value object.
        /// </summary>
        /// <returns>Enumerable of properties to participate in equality comparison</returns>
        protected abstract IEnumerable<object> GetAtomicValues();

        public override int GetHashCode()
        {
            return GetAtomicValues()
                .Select((x, i) => (x != null ? x.GetHashCode() : 0) + HighPrime * i)
                .Aggregate((x, y) => x ^ y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;

            var other = (ValueObject)obj;

            return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
        }

        public ValueObject GetCopy()
        {
            return MemberwiseClone() as ValueObject;
        }

        public static bool operator ==(ValueObject one, ValueObject two)
        {
            return EqualOperator(one, two);
        }

        public static bool operator !=(ValueObject one, ValueObject two)
        {
            return NotEqualOperator(one, two);
        }

        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null)) return false;
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !EqualOperator(left, right);
        }
    }
}
