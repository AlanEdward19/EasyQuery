using System.Text;
using CKFA.EasyQuery.Enums;
using CKFA.EasyQuery.ValueObjects;

namespace CKFA.EasyQuery.Services;

public static class QueryBuilder
{
    #region Functions

    public static string BuildQuery(EDatabase database, string tableName, string? fields = null,
        List<QueryWhere>? whereParameters = null, string? orderBy = null, List<QueryJoinTable>? joinTables = null, int limit = 1000, int page = 1)
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

            //Builds inner join
            string joinTableName = database == EDatabase.Postgres ? $"\"{joinTable.TableName}\"" : joinTable.TableName;
            string firstTableColumn = GetColumnName(database, joinTable.FirstTableColumn, tableName);
            string lastTableColumn = GetColumnName(database, joinTable.LastTableColumn, joinTable.TableName);
            List<string> joinFields = GetJoinFields(database, joinTable);

            string joinCondition = $"{firstTableColumn} = {lastTableColumn}";
            joinBuilder.Append($"INNER JOIN {joinTableName} ON {joinCondition} ");
            fieldBuilder.Append($", {string.Join(", ", joinFields)}");
        }

        #endregion

        #region Where

        /*
         * Build queryWhere param
         */

        StringBuilder whereBuilder = new();

        if (whereParameters != null && whereParameters.Any())
        {
            whereBuilder.Append("WHERE ");

            foreach (QueryWhere whereParameter in whereParameters)
            {
                // Split the values by comma and add quotes if the type is Text
                var whereValues = whereParameter.Value
                    .Replace(" ", "").Split(",")
                    .Select(x => whereParameter.Type == EColumnType.Text ? $"'{x}'" : $"{x}")
                    .ToList();

                string value = GetWhereValue(database, whereParameter.Operator, tableName, whereParameter.Key,
                    whereValues);

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

        string query = $"SELECT {field} FROM {tableName} {join} {whereString} {order} LIMIT {limit} OFFSET {(page - 1) * limit}";

        return query;
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
    private static string GetWhereValue(EDatabase database, EOperator queryOperator, string tableName,
        string columnName, List<string> whereValues) => queryOperator switch
    {
        EOperator.OR when database == EDatabase.Postgres => string.Join(" OR ",
            whereValues.Select(x => $"{tableName}.\"{columnName}\" = {x}")),

        EOperator.OR when database == EDatabase.SqlServer => string.Join(" OR ",
            whereValues.Select(x => $"{tableName}.{columnName} = {x}")),

        EOperator.IN when database == EDatabase.Postgres =>
            $"{tableName}.\"{columnName}\" IN ({string.Join(",", whereValues)})",

        EOperator.IN when database == EDatabase.SqlServer =>
            $"{tableName}.{columnName} IN ({string.Join(",", whereValues)})"
    };

    #endregion

    #region Validations

    static bool ValidateQueryJoinTable(QueryJoinTable joinTable) => string.IsNullOrEmpty(joinTable.TableName) ||
                                                                    joinTable.Fields == null ||
                                                                    !joinTable.Fields.Any() ||
                                                                    string.IsNullOrEmpty(joinTable.FirstTableColumn) ||
                                                                    string.IsNullOrEmpty(joinTable.LastTableColumn);

    #endregion
}