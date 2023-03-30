namespace Interpreter;

public class CJApp
{
    public string Name { get; set; }
    

}

public class CJClass : CJScope
{
    public string Name { get; set; }
    public List<CJVariable> ConstructorParams { get; set; }
    public List<CJStatement> ConstructorStatements { get; set; }
    
}

public class CJMethod : CJScope
{
    public string Name { get; set; }
    public List<CJVariable> Params { get; set; }
    public List<CJStatement> Statements { get; set; }
}

public abstract class CJStatement
{
    public CJStatementType Type { get; private set; }    
    
    public string Data { get; private set; }

    public CJStatement(CJStatementType type, string data)
    {
        Type = type;
        Data = data;
    }    
}

public class CJAssignmentStatement : CJStatement
{

    public List<CJVariable> TargetReferences { get; private set; }
    public CJVariable ValueHolder { get; private set; }
    public CJAssignmentStatement(string data) : base(CJStatementType.Assignment, data)
    {
        var splt = data.Split('=');
        if (splt.Length < 2)
            throw new Exception("improperly formated assignment statement");

        TargetReferences = new List<CJVariable>();

        ValueHolder = new CJVariable
        {
            Name = splt[^1].Trim(),            
        };
    }
}

public abstract class CJScope
{
    public List<CJVariable> Variables { get; set; }
    public List<CJMethod> Methods { get; set; }
}


public enum CJStatementType
{
    Assignment,
    If,
    While,
    For,
    Return,
    Break,
    Continue,
    Expression,
    Block
}

public class CJVariable
{
    public CJType Type { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}

public enum CJType
{
    String,
    Integer,
    Float,
    Boolean,
    Object,
    Array,
    Null
}
