IF OBJECT_ID(N'dbo.ProjectionsAuditLog', N'U') IS NULL 
BEGIN
    CREATE TABLE [dbo].ProjectionsAuditLog (
        [Sequence] BIGINT NOT NULL,
        [AppliedAt] DATETIME2(7) NOT NULL
        );

    DROP INDEX IF EXISTS [SequenceIndex] ON [dbo].[ProjectionsAuditLog];

    CREATE NONCLUSTERED INDEX [SequenceIndex] ON [dbo].[ProjectionsAuditLog] ([Sequence]);
END;