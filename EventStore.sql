IF OBJECT_ID(N'dbo.EventStore', N'U') IS NULL 
BEGIN
    CREATE TABLE [dbo].[EventStore] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [AggregateId] NVARCHAR(100) NOT NULL,
        [Aggregate] NVARCHAR(100) NOT NULL,
        [Version] INT NOT NULL,
        [Sequence] INT IDENTITY(1,1) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL,
        [Payload] NVARCHAR(MAX) NOT NULL) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

    DROP INDEX IF EXISTS [ConcurrencyCheckIndex] ON [dbo].[EventStore];

    CREATE UNIQUE NONCLUSTERED INDEX [ConcurrencyCheckIndex] ON [dbo].[EventStore]
        ([Version] ASC, [AggregateId] ASC) WITH (
            PAD_INDEX = OFF,
            STATISTICS_NORECOMPUTE = OFF,
            SORT_IN_TEMPDB = OFF,
            IGNORE_DUP_KEY = OFF,
            DROP_EXISTING = OFF, ONLINE = OFF,
            ALLOW_ROW_LOCKS = ON,
            ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];

    DROP INDEX IF EXISTS [AggregateIdIndex] ON [dbo].[EventStore];
    
    CREATE NONCLUSTERED INDEX [AggregateIdIndex] ON [dbo].[EventStore]
        ([AggregateId] ASC) WITH (
            PAD_INDEX = OFF, 
            STATISTICS_NORECOMPUTE = OFF, 
            SORT_IN_TEMPDB = OFF, 
            DROP_EXISTING = OFF, 
            ONLINE = OFF, 
            ALLOW_ROW_LOCKS = ON, 
            ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];

    DROP INDEX IF EXISTS [SequenceIndex] ON [dbo].[EventStore];
    
    CREATE CLUSTERED INDEX [SequenceIndex] ON [dbo].[EventStore]
        ([Sequence] ASC) WITH (
            PAD_INDEX = OFF, 
            STATISTICS_NORECOMPUTE = OFF, 
            SORT_IN_TEMPDB = OFF, 
            DROP_EXISTING = OFF, 
            ONLINE = OFF, 
            ALLOW_ROW_LOCKS = ON, 
            ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];

    DROP INDEX IF EXISTS [IdIndex] ON [dbo].[EventStore];
    
    CREATE NONCLUSTERED INDEX [IdIndex] ON [dbo].[EventStore]
        ([Id] ASC) WITH (
            PAD_INDEX = OFF,
            STATISTICS_NORECOMPUTE = OFF,
            SORT_IN_TEMPDB = OFF,
            DROP_EXISTING = OFF,
            ONLINE = OFF,
            ALLOW_ROW_LOCKS = ON,
            ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
END;