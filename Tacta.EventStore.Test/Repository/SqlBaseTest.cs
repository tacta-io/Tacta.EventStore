﻿using System;
using Dapper;
using Tacta.EventStore.Test.Repository.ConnectionFactory;
using Tacta.Connection;


#if USE_SYSTEM_DATA_SQLCLIENT
    using System.Data.SqlClient;
#elif USE_MICROSOFT_DATA_SQLCLIENT
using Microsoft.Data.SqlClient;
#endif

namespace Tacta.EventStore.Test.Repository
{
    public abstract class SqlBaseTest : IDisposable
    {
        public const string UserReadModelTableName = "[dbo].[UserReadModel]";
        private const string EventStoreTableName = "[dbo].[EventStore]";
        private const string AuditTableName = "[dbo].[ProjectionsAuditLog]";

        private const string MasterConnectionString =
            @"Server=(localdb)\mssqlLocaldb; Database=master; Trusted_Connection=True;";
        private readonly string _connectionString;
        private readonly string _dbName;

        protected readonly IConnectionFactory ConnectionFactory;

        protected SqlBaseTest()
        {
            _dbName = $"TmpTestDb{Guid.NewGuid().ToString("n").Substring(0, 8)}";
            _connectionString =
                $@"Server=(localdb)\mssqlLocaldb;Database={_dbName};Trusted_Connection=True;Max Pool Size=200;Connect Timeout=60";
            ConnectionFactory = new SqlConnectionFactory(_connectionString);

            CreateDatabase();
            CreateTables();
        }

        public void Dispose() => DeleteDatabase();

        private void CreateDatabase()
        {
            var createDatabase = $@"CREATE DATABASE {_dbName};";

            using var masterConnection = new SqlConnection(MasterConnectionString);
            masterConnection.Execute(createDatabase);
        }

        private void CreateTables()
        {
            CreateEventStoreTable();
            CreateUserReadModelTable();
            CreateAuditTable();
        }

        private void DeleteDatabase()
        {
            var dropDatabase =
                $@"USE master;                 
                ALTER DATABASE {_dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE {_dbName};";

            using var connection = new SqlConnection(_connectionString);

            connection.Execute(dropDatabase);
        }

        private void CreateEventStoreTable()
        {
            using var connection = new SqlConnection(_connectionString);
            var sqlScript =
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

            connection.Execute(sqlScript);
        }

        private void CreateUserReadModelTable()
        {
            using var connection = new SqlConnection(_connectionString);
            var sqlScript =
                $@"CREATE TABLE {UserReadModelTableName}(
	                [Id] [uniqueidentifier] NOT NULL,
	                [UpdatedAt] [datetime2](7) NOT NULL,
	                [Sequence] [int] NOT NULL,
	                [EventId] [uniqueidentifier] NOT NULL,
	                [Name] [nvarchar](50) NOT NULL) ON [PRIMARY]";

            connection.Execute(sqlScript);
        }

        private void CreateAuditTable()
        {
            using var connection = new SqlConnection(_connectionString);
            var sqlScript =
                $@"CREATE TABLE {AuditTableName} (
                    [Sequence] BIGINT NOT NULL,
                    [AppliedAt] DATETIME2(7) NOT NULL);
                CREATE NONCLUSTERED INDEX [SequenceIndex] ON {AuditTableName} ([Sequence]);";

            connection.Execute(sqlScript);
        }
    }
}