IF OBJECT_ID(N'dbo.Audit', N'U') IS NULL 
BEGIN
    CREATE TABLE [dbo].Audit (
        [Sequence] BIGINT NOT NULL,
        [AppliedAt] DATETIME2(7) NOT NULL
        );

    DROP INDEX IF EXISTS [SequenceIndex] ON [dbo].[Audit];

    CREATE NONCLUSTERED INDEX [SequenceIndex] ON [dbo].[Audit] ([Sequence]);
END;