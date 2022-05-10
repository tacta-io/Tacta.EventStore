using System.Collections.Generic;
using Tacta.EventStore.Test.Domain.ValueObjects;
using Xunit;

namespace Tacta.EventStore.Test.Domain
{
    public class ValueObjectTest
    {
        [Theory]
        [MemberData(nameof(GetStructurallyEqualAssignees))]
        public void ValueObject_StructurallyEqualsAnotherValueObject(Assignee firstAssignee, Assignee secondAssignee)
        {
            Assert.True(firstAssignee.Equals(secondAssignee));
            Assert.Equal(firstAssignee, secondAssignee);
            Assert.True(firstAssignee == secondAssignee);
            Assert.False(firstAssignee != secondAssignee);
        }

        [Theory]
        [MemberData(nameof(GetStructurallyUnequalAssignees))]
        public void ValueObject_StructurallyNotEqualsAnotherValueObject(Assignee firstAssignee, Assignee secondAssignee)
        {
            Assert.False(firstAssignee.Equals(secondAssignee));
            Assert.NotEqual(firstAssignee, secondAssignee);
            Assert.False(firstAssignee == secondAssignee);
            Assert.True(firstAssignee != secondAssignee);
        }

        [Fact]
        public void ValueObject_StructurallyEqualsItsCopy()
        {
            var assignee0 = new Assignee("Max Mathew", "max", 0, 0);
            var assignee1 = assignee0.GetCopy() as Assignee;

            Assert.Equal(assignee0.Name, assignee1?.Name);
            Assert.Equal(assignee0.DisplayName, assignee1?.DisplayName);

            Assert.True(assignee0.Equals(assignee1));
            Assert.Equal(assignee0, assignee1);
            Assert.True(assignee0 == assignee1);
            Assert.False(assignee0 != assignee1);
        }

        [Fact]
        public void ValueObject_StructurallyNotEqualsToNull()
        {
            var assignee = new Assignee("Max Mathew", "max", 0, 0);

            Assert.False(assignee.Equals(null));
            Assert.False(null == assignee);
            Assert.True(assignee != null);
        }

        [Fact]
        public void ValueObject_HashCodeValueShouldBeDependentOnIndex()
        {
            var assignee0 = new Assignee("sam", "sam", 0, 0);
            var assignee1 = new Assignee("joe", "joe", 0, 0);
            var assignee2 = new Assignee("sam", "sam", 0, 0);

            Assert.True(assignee0 != assignee1);
            Assert.False(assignee0 != assignee2);
        }

        public static IEnumerable<object[]> GetStructurallyEqualAssignees()
        {
            var data = new List<object[]>
            {
                new object[]
                {
                    new Assignee("John Doe", "john", 0, 0),
                    new Assignee("John Doe", "john", 0, 0)
                },
                new object[]
                {
                    new Assignee("John Doe", "john", 10, 100),
                    new Assignee("John Doe", "john", 10, 100)
                },
                new object[]
                {
                    new Assignee("John Doe", "john", 270.00m, 45.90m),
                    new Assignee("John Doe", "john", 270.00m, 45.90m)
                }
            };

            return data;
        }

        public static IEnumerable<object[]> GetStructurallyUnequalAssignees()
        {
            var data = new List<object[]>
            {
                new object[]
                {
                    new Assignee("Max Mathew", "max", 0, 0),
                    new Assignee("John Doe", "john", 0, 0)
                },
                new object[]
                {
                    new Assignee("John Doe", "john", 10, 100),
                    new Assignee("John Doe", "john", 1, 10)
                },
                new object[]
                {
                    new Assignee("John Doe", "john", 270.00m, 45.90m),
                    new Assignee("John Doe", "john", -270.00m, -45.90m)
                }
            };

            return data;
        }
    }
}
