using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper
{
    public static class DapperExtensions
    {
        public static async Task MergeAsync<TEntity>(this IDbConnection dbConnection, TEntity entity, string tableName, string lastModifiedFieldName) where TEntity : class
        {
            var props = entity.GetType().GetProperties().Select(p => p.Name).ToList();
            var keyFieldName = entity.GetType().GetProperties().Single(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(KeyAttribute))).Name;
            var names = string.Join(", ", props);
            var values = string.Join(", ", props.Select(n => "@" + n));
            var updates = string.Join(", ", props.Select(n => $"{n} = @{n}"));
            await dbConnection.ExecuteAsync(
                  $@"MERGE {tableName} as target
          USING (VALUES({values}))
          AS SOURCE ({names})
          ON target.{keyFieldName} = @{keyFieldName}
          WHEN matched and target.{lastModifiedFieldName}<source.{lastModifiedFieldName} THEN
            UPDATE SET {updates}
          WHEN not matched THEN
            INSERT({names}) VALUES({values});",
                  entity);
        }
    }
}