using System.Text;
using CKFA.EasyQuery.Enums;
using CKFA.EasyQuery.ValueObjects;
using Dapper;

namespace CKFA.EasyQuery.Services;

public static class QueryBuilder
{
    #region Functions

    public static BuildedQuery BuildQuery(EDatabase database, string tableName, string? fields = null,
        List<QueryWhere>? whereParameters = null, string? orderBy = null, List<QueryJoinTable>? joinTables = null,
        int limit = 1000, int page = 1, QueryReturn returnType = QueryReturn.ORM)
    {
        tableName = database == EDatabase.Postgres ? $"\"{tableName}\"" : tableName;

        #region Fields

        /*
         * Build queryFields param, for now specific for postgres
         */

        StringBuilder fieldBuilder = new();

        List<string>? fieldsList = !string.IsNullOrEmpty(fields) switch
        {
            true when database == EDatabase.Postgres =>
                fields.Replace(" ", "").Split(",").Select(x => $"\"{x}\"").ToList(),

            true when database == EDatabase.SqlServer =>
                fields.Replace(" ", "").Split(",").Select(x => $"{x}").ToList(),

            _ => null
        };

        fieldBuilder.Append(fieldsList != null ? string.Join(", ", fieldsList.Select(a => $"{tableName}.{a}")) : "*");

        #endregion

        #region Inner Join

        /*
         * Build queryInnerJoin param
         */

        StringBuilder joinBuilder = new StringBuilder();

        foreach (QueryJoinTable joinTable in joinTables)
        {
            if (ValidateQueryJoinTable(joinTable))
            {
                continue;
            }

            if (joinTable.Fields.Any(queryField => fieldsList.Any(y => queryField.Alias.ToLower().Contains(y.Replace("\"","")))))
            {
                var matches = fieldsList.Where(x =>
                    joinTable.Fields.Any(y => y.Alias.ToLower().Contains(x.Replace("\"","")))).ToList();

                //Removes duplicated fields and comma's
                foreach (var match in matches)
                {
                    string fullSentence = $"{tableName}.{match}";
                    int start = fieldBuilder.ToString().IndexOf(fullSentence);
                    int length = fullSentence.Length;

                    fieldBuilder.Remove(start, length);
                    fieldBuilder.Replace(",", " ");
                    fieldBuilder = new(string.Join(",", fieldBuilder.ToString().Split(" ").Where(x => x != "")));
                }

                //Builds inner join
                string joinTableName = database == EDatabase.Postgres ? $"\"{joinTable.TableName}\"" : joinTable.TableName;
                string firstTableColumn = GetColumnName(database, joinTable.FirstTableColumn, tableName);
                string lastTableColumn = GetColumnName(database, joinTable.LastTableColumn, joinTable.TableName);
                List<string> joinFields = GetJoinFields(database, joinTable);

                string joinCondition = $"{firstTableColumn} = {lastTableColumn}";
                joinBuilder.Append($"INNER JOIN {joinTableName} ON {joinCondition} ");
                fieldBuilder.Append(fieldBuilder.Length != 0 ? $", {string.Join(", ", joinFields)}" : string.Join(", ", joinFields));
            }
        }

        #endregion

        #region Where

        /*
         * Build queryWhere param
         */

        StringBuilder whereBuilder = new();
        var parameters = new DynamicParameters();

        if (whereParameters != null && whereParameters.Any())
        {
            whereBuilder.Append("WHERE ");

            foreach (QueryWhere whereParameter in whereParameters)
            {
                var paramName = $"@p{whereParameter.Key}";

                // Split the values by comma and add quotes if the type is Text
                var whereValues = whereParameter.Value
                    .Replace(" ", "").Split(",")
                    .Select(x => whereParameter.Type == EColumnType.Text && returnType == QueryReturn.Query ? $"'{x}'" : $"{x}")
                    .ToList();

                string value = GetWhereValue(ref parameters, database, whereParameter.Operator, tableName, whereParameter.Key,
                    whereValues, paramName, returnType);

                whereBuilder.Append($"{value} AND ");
            }

            // Remove the last AND and return the final where string
            whereBuilder.Remove(whereBuilder.Length - 5, 5);
        }

        #endregion

        #region OrderBy

        string order = "";

        if (!string.IsNullOrEmpty(orderBy))
            order = $"ORDER BY {orderBy}"; //terminar

        #endregion

        #region Build query strings

        string field = fieldBuilder.ToString();
        string join = joinBuilder.ToString();
        string whereString = whereBuilder.ToString();

        #endregion

        return new()
        {
            Query = $"SELECT {field} FROM {tableName} {join} {whereString} {order} LIMIT {limit} OFFSET {(page - 1) * limit}",
            Parameters = parameters
        };
    }

    #endregion

    #region Get Functions

    private static string GetColumnName(EDatabase database, string columnName, string tablename) => database switch
    {
        EDatabase.Postgres => $"\"{tablename.Replace("\"", "")}\".\"{columnName}\"",
        EDatabase.SqlServer => $"{tablename}.{columnName}"
    };

    private static List<string> GetJoinFields(EDatabase database, QueryJoinTable joinTable) => joinTable.Fields
        .Select(x => $"{GetColumnName(database, x.Field, joinTable.TableName)} AS {x.Alias}").ToList();

    private static string GetWhereValue(ref DynamicParameters parameters, EDatabase database, EOperator queryOperator, string tableName,
        string columnName, List<string> whereValues, string paramName, QueryReturn returnType)
    {
        return queryOperator switch
        {
            EOperator.OR => BuildOrValueQuery(ref parameters, whereValues, paramName, tableName, columnName, database, returnType),

            EOperator.IN => BuildInValueQuery(ref parameters, whereValues, paramName, tableName, columnName, database, returnType)
        };
    }

    private static string BuildOrValueQuery(ref DynamicParameters parameters, List<string> whereValues, string paramName, string tableName, string columnName, EDatabase database, QueryReturn returnType)
    {
        int i = 0;
        StringBuilder query = new();
        while (i < whereValues.Count)
        {
            string param = $"{paramName}{i}";
            parameters.Add(param, whereValues[i]);

            switch (database)
            {
                case EDatabase.Postgres when returnType == QueryReturn.ORM:
                    query.Append($"{tableName}.\"{columnName}\" = {param} OR ");
                    break;

                case EDatabase.SqlServer when returnType == QueryReturn.ORM:
                    query.Append($"{tableName}.{columnName} = {param} OR ");
                    break;

                case EDatabase.Postgres when returnType == QueryReturn.Query:
                    query.Append($"{tableName}.\"{columnName}\" = {whereValues[i]} OR ");
                    break;

                case EDatabase.SqlServer when returnType == QueryReturn.Query:
                    query.Append($"{tableName}.{columnName} = {whereValues[i]} OR ");
                    break;
            }

            i++;
        }

        // Remove the last OR
        query.Remove(query.Length - 4, 4);

        return query.ToString();
    }

    private static string BuildInValueQuery(ref DynamicParameters parameters, List<string> whereValues, string paramName, string tableName, string columnName, EDatabase database, QueryReturn returnType)
    {
        parameters.Add(paramName, string.Join(',', whereValues));

        return database switch
        {
            EDatabase.Postgres when returnType == QueryReturn.ORM =>
                $"{tableName}.\"{columnName}\" IN ({paramName})",

            EDatabase.SqlServer when returnType == QueryReturn.ORM =>
                $"{tableName}.{columnName} IN ({paramName})",

            EDatabase.Postgres when returnType == QueryReturn.Query=>
                $"{tableName}.\"{columnName}\" IN ({string.Join(",", whereValues)})",

            EDatabase.SqlServer when returnType == QueryReturn.Query =>
                $"{tableName}.{columnName} IN ({string.Join(",", whereValues)})",
        };
    }

    #endregion

    #region Validations

    static bool ValidateQueryJoinTable(QueryJoinTable joinTable) => string.IsNullOrEmpty(joinTable.TableName) ||
                                                                    joinTable.Fields == null ||
                                                                    !joinTable.Fields.Any() ||
                                                                    string.IsNullOrEmpty(joinTable.FirstTableColumn) ||
                                                                    string.IsNullOrEmpty(joinTable.LastTableColumn);

    #endregion
}