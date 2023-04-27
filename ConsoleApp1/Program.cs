using CKFA.EasyQuery.Enums;
using CKFA.EasyQuery.Services;
using CKFA.EasyQuery.ValueObjects;


string tablename = "Table1";
string fields = "field1, field2, field3";

#region Where Clauses

List<QueryWhere> whereClauses = new List<QueryWhere>()
{
    new()
    {
        Key = "stringWithIn",
        Type = EColumnType.Text,
        Operator = EOperator.IN,
        Value = "ALAN, EDWARD"
    },
    new()
    {
        Key = "intWithIn",
        Type = EColumnType.Int,
        Operator = EOperator.IN,
        Value = "1, 2"
    },
    new()
    {
        Key = "stringWithOr",
        Type = EColumnType.Text,
        Operator = EOperator.OR,
        Value = "Alan, Edward"
    },
    new()
    {
        Key = "intWithOr",
        Type = EColumnType.Int,
        Operator = EOperator.OR,
        Value = "1, 2"
    }
};

#endregion

#region Join Tables

List<QueryJoinTable> joinTables = new List<QueryJoinTable>()
{
    new()
    {
        TableName = "Table2",
        FirstTableColumn = "id",
        LastTableColumn = "id",
        Fields = new()
        {
            new ()
            {
                Field = "field2",
                Alias = "RightField2"
            }
        }
    },
    new()
    {
        TableName = "Table3",
        FirstTableColumn = "id",
        LastTableColumn = "id",
        Fields = new()
        {
            new()
            {
                Field = "field3",
                Alias = "RightField3"
            }
        }
    }
};

#endregion

var querySqlServer = QueryBuilder.BuildQuery(EDatabase.SqlServer, tablename, fields, whereClauses, null, joinTables,returnType:QueryReturn.Query);
var queryPostgres = QueryBuilder.BuildQuery(EDatabase.Postgres, tablename, fields, whereClauses, null, joinTables, returnType: QueryReturn.Query);

Console.WriteLine($"\nQuery SQL Server: {querySqlServer.Query}\n");
Console.WriteLine($"\nQuery Postgres: {queryPostgres.Query}\n");
