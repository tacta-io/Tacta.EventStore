using System;
using System.Data.SqlClient;
using Dapper;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Repository.ConnectionFactory;

namespace Tacta.EventStore.Test.Repository
{
    public abstract class SqlBaseTest : IDisposable
    {
        public const string EventStoreTableName = "[dbo].[EventStore]";
        public const string UserReadModelTableName = "[dbo].[UserReadModel]";

        private const string MasterConnectionString =
            @"Server=(localdb)\mssqlLocaldb; Database=master; Trusted_Connection=True;";

        public static ISqlConnectionFactory ConnectionFactory;

        private static readonly string CreateEventStoreTable =
            $@"CREATE TABLE {EventStoreTableName} (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [Name] NVARCHAR(100) NOT NULL,
                [AggregateId] NVARCHAR(100) NOT NULL,
                [Aggregate] NVARCHAR(100) NOT NULL,
                [Version] INT NOT NULL,
                [Sequence] INT IDENTITY(1,1) NOT NULL,
                [CreatedAt] DATETIME2(7) NOT NULL,
                [Payload] NVARCHAR(MAX) NOT NULL) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];          
            CREATE UNIQUE NONCLUSTERED INDEX [ConcurrencyCheckIndex] ON [dbo].[EventStore]
                ([Version] ASC, [AggregateId] ASC) WITH (
                PAD_INDEX = OFF,
                STATISTICS_NORECOMPUTE = OFF,
                SORT_IN_TEMPDB = OFF,
                IGNORE_DUP_KEY = OFF,
                DROP_EXISTING = OFF, ONLINE = OFF,
                ALLOW_ROW_LOCKS = ON,
                ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
            CREATE NONCLUSTERED INDEX [AggregateIdIndex] ON [dbo].[EventStore]
                ([AggregateId] ASC) WITH (
                PAD_INDEX = OFF, 
                STATISTICS_NORECOMPUTE = OFF, 
                SORT_IN_TEMPDB = OFF, 
                DROP_EXISTING = OFF, 
                ONLINE = OFF, 
                ALLOW_ROW_LOCKS = ON, 
                ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];";

        private static readonly string CreateUserReadModelTable =
            $@"CREATE TABLE {UserReadModelTableName}(
	            [Id] [uniqueidentifier] NOT NULL,
	            [UpdatedAt] [datetime2](7) NOT NULL,
	            [Sequence] [int] NOT NULL,
	            [EventId] [uniqueidentifier] NOT NULL,
	            [Name] [nvarchar](50) NOT NULL) ON [PRIMARY]";

        private static readonly string Database = $"TmpTestDb{Guid.NewGuid().ToString("n").Substring(0, 8)}";

        protected string ConnectionString =
            $@"Server=(localdb)\mssqlLocaldb; Database={Database}; Trusted_Connection=True;";

        protected SqlBaseTest()
        {
            CreateDatabase();
        }

        public void Dispose()
        {
            DeleteDatabase();
        }

        private void CreateDatabase()
        {
            var createDatabase = $@"CREATE DATABASE {Database};";

            using var masterConnection = new SqlConnection(MasterConnectionString);

            masterConnection.Execute(createDatabase);

            using var connection = new SqlConnection(ConnectionString);

            connection.Execute(CreateEventStoreTable);
            connection.Execute(CreateUserReadModelTable);

            ConnectionFactory = new SqlConnectionFactory(ConnectionString);
        }

        private void DeleteDatabase()
        {
            var dropDatabase =
                $@"USE master;                 
                ALTER DATABASE {Database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE {Database};";

            using var connection = new SqlConnection(ConnectionString);

            connection.Execute(dropDatabase);
        }
    }
}