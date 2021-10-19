namespace Tacta.EventStore.Domain
{
    public interface IEntity<out TIdentity> where TIdentity : IEntityId
    {
        TIdentity Id { get; }
    }
}
