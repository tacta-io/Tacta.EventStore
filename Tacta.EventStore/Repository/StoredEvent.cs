using System;

namespace Tacta.EventStore.Repository
{
    internal sealed class StoredEvent
    {
        public string AggregateId { get; set; }
        public string Aggregate { get; set; }
        public int Version { get; set; }
        public long Sequence { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Payload { get; set; }

        public static string InsertQuery =
            @"INSERT INTO [dbo].[EventStore] 
                ([Id], [Name], [AggregateId], [Aggregate], [Version], [CreatedAt], [Payload]) 
                    VALUES (@Id, @Name, @AggregateId, @Aggregate, @Version, @CreatedAt, @Payload);";

        public static string SelectQuery =
            @"SELECT [Id], [Name], [AggregateId], [Version], [CreatedAt], [Payload], [Sequence] 
                FROM [dbo].[EventStore] WHERE [AggregateId] = @AggregateId";

        public static string SelectChunkedWithoutLimitQuery =
            @"SELECT [Id], [Name], [AggregateId], [Version], [CreatedAt], [Payload], [Sequence] 
                FROM [dbo].[EventStore] 
                WHERE [Sequence] > @Sequence
                ORDER BY [Sequence]";

        public static string SelectChunkedWithLimitQuery =
            @"SELECT TOP (@Take) [Id], [Name], [AggregateId], [Version], [CreatedAt], [Payload], [Sequence] 
                FROM [dbo].[EventStore] 
                WHERE [Sequence] > @Sequence
                ORDER BY [Sequence]";

        public static string SelectUntilEventQuery =
            @"SELECT [Id], [Name], [AggregateId], [Version], [CreatedAt], [Payload], [Sequence] 
                FROM [dbo].[EventStore] 
                WHERE [AggregateId] = @AggregateId AND [Sequence] <= (SELECT TOP 1 [Sequence] FROM [dbo].[EventStore] WHERE [Id] = @EventId)
                ORDER BY [Sequence]";

        public static string SelectUntilSequenceQuery =
            @"SELECT [Id], [Name], [AggregateId], [Version], [CreatedAt], [Payload], [Sequence] 
                FROM [dbo].[EventStore] 
                WHERE [AggregateId] = @AggregateId AND [Sequence] <= @Sequence
                ORDER BY [Sequence]";

        public static string SelectLatestSequenceQuery =
            @"SELECT MAX ([Sequence]) FROM [dbo].[EventStore]";
    }
}
