using CKFA.EasyQuery.Enums;

namespace CKFA.EasyQuery.ValueObjects;

public class QueryWhere
{
    public EOperator Operator { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public EColumnType Type { get; set; }
}