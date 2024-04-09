using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Tacta.Connection;

namespace Tacta.EventStore.Repository
{
    public abstract class GenericRepository : IGenericRepository
    {
        private readonly IConnectionFactory _sqlConnectionFactory;
        private readonly string _table;

        protected GenericRepository(IConnectionFactory sqlConnectionFactory, string table)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _table = table;
        }

        public async Task<T> GetAsync<T>(Guid id)
        {
            using (var connection = _sqlConnectionFactory.Connection())
            {
                return await connection.QuerySingleOrDefaultAsync<T>($"SELECT * FROM {_table} WHERE Id=@Id",
                    new {Id = id});
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>()
        {
            using (var connection = _sqlConnectionFactory.Connection())
            {
                return await connection.QueryAsync<T>($"SELECT * FROM {_table}");
            }
        }

        public async Task InsertAsync<T>(T t)
        {
            var insertQuery = GenerateInsertQuery<T>();

            using (var connection = _sqlConnectionFactory.Connection())
            {
                await connection.ExecuteAsync(insertQuery, t);
            }
        }

        public async Task<int> InsertAsync<T>(IEnumerable<T> list)
        {
            var inserted = 0;
            var query = GenerateInsertQuery<T>();

            using (var connection = _sqlConnectionFactory.Connection())
            {
                inserted += await connection.ExecuteAsync(query, list);

                return inserted;
            }
        }

        public async Task UpdateAsync<T>(T t)
        {
            var updateQuery = GenerateUpdateQuery<T>();

            using (var connection = _sqlConnectionFactory.Connection())
            {
                await connection.ExecuteAsync(updateQuery, t);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            using (var connection = _sqlConnectionFactory.Connection())
            {
                await connection.ExecuteAsync($"DELETE FROM {_table} WHERE Id=@Id", new {Id = id});
            }
        }

        public async Task DeleteAllAsync()
        {
            using (var connection = _sqlConnectionFactory.Connection())
            {
                await connection.ExecuteAsync($"DELETE FROM {_table}");
            }
        }

        private string GenerateUpdateQuery<T>()
        {
            var updateQuery = new StringBuilder($"UPDATE {_table} SET ");
            var properties = GenerateListOfProperties(typeof(T).GetProperties());

            properties.ForEach(property =>
            {
                if (!property.Equals("Id")) updateQuery.Append($"[{property}]=@{property},");
            });

            updateQuery.Remove(updateQuery.Length - 1, 1);
            updateQuery.Append(" WHERE [Id]=@Id");

            return updateQuery.ToString();
        }

        private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties)
        {
            return (from prop in listOfProperties
                let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
                where attributes.Length <= 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore"
                select prop.Name).ToList();
        }

        private string GenerateInsertQuery<T>()
        {
            var insertQuery = new StringBuilder($"INSERT INTO {_table} ");

            insertQuery.Append("(");

            var properties = GenerateListOfProperties(typeof(T).GetProperties());
            properties.ForEach(prop => { insertQuery.Append($"[{prop}],"); });

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(") VALUES (");

            properties.ForEach(prop => { insertQuery.Append($"@{prop},"); });

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(")");

            return insertQuery.ToString();
        }
    }
}