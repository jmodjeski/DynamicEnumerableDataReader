using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ModjeskiNet.Data
{
    public static class DataReaderExtensions
    {
        public static IEnumerable<dynamic> AsEnumerable(this IDataReader reader)
        {
            while (reader.Read())
            {
                yield return new DynamicDataRecord(reader);
            }
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> query, string clause)
        {
            var builder = new PredicateBuilder<T>();
            var predicate = builder.Build(clause);

            return query.Where(predicate);
        }
    }
}
