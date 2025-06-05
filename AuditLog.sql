IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL 
BEGIN
    CREATE TABLE [dbo].AuditLog (
        [Sequence] BIGINT NOT NULL,
        [AppliedAt] DATETIME2(7) NOT NULL
        );

    DROP INDEX IF EXISTS [SequenceIndex] ON [dbo].[AuditLog];

    CREATE NONCLUSTERED INDEX [SequenceIndex] ON [dbo].[AuditLog] ([Sequence]);
END;