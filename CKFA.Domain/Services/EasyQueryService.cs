using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CKFA.Domain.Enum;

namespace CKFA.Domain.Services
{
    public class EasyQueryService
    {
        public EasyQueryService()
        {
            
        }

        public async Task<string> GetQuery(EDatabaseLanguage language, List<string>? fields, List<string>? fieldValues,
            string databaseName, string tableName)
        {
            string lastFieldOnFields = "";
            if (fieldValues == null)
            {
                if (fields != null)
                {
                    return null;
                }
            }

            var baseQuery = new StringBuilder();
            baseQuery.Append($"Select ");

            if (fields != null)
            {
                lastFieldOnFields = fields.ElementAt(fields.Count - 1);

                fields.ForEach(field =>
                {
                    if (field != null && field != lastFieldOnFields) baseQuery.Append((string?)$" {field}, ");

                    else if (field == lastFieldOnFields) baseQuery.Append($"{field} ");
                });
            }

            else
            {
                baseQuery.Append(" * ");
            }

            baseQuery.Append($"FROM {databaseName}.dbo.{tableName} ");

            if (fieldValues != null)
            {
                baseQuery.Append($"Where ");

                for (int i = 0; i < fields.Count; i++)
                {
                   if (fields[i] != lastFieldOnFields) baseQuery.Append($"{fields[i]} = {fieldValues[i]} AND ");

                   else baseQuery.Append($"{fields[i]} = {fieldValues[i]}");
                }

            }

            return baseQuery.ToString();
        }
    }
}
