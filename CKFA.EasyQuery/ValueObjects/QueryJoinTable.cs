namespace CKFA.EasyQuery.ValueObjects;

public class QueryJoinTable
{
    public string TableName { get; set; }
    public List<QueryField> Fields { get; set; }
    public string FirstTableColumn { get; set; }
    public string LastTableColumn { get; set; }
}