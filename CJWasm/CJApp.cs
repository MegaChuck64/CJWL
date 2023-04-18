
namespace CJWasm;

public class CJApp
{
    public List<CJFunction> Functions { get; private set; }

    public CJApp(List<CJFunction> functions)
    {
        Functions = functions;
    }
}

public class CJFunction
{
    public CJFunctionType FunctionType { get; set; }
    public CJType ReturnType { get; set; }
    public string Name { get; set; }
    public List<CJType> ParamTypes { get; set; }
    public List<string> Lines { get; set; }

    public CJFunction(
        string name, 
        CJFunctionType functionType, 
        CJType returnType, 
        List<CJType> paramTypes, 
        List<string> lines)
    {
        Name = name;
        FunctionType = functionType;
        ReturnType = returnType;
        ParamTypes = paramTypes;
        Lines = lines;
    }
}

public class CJStatement
{
    public CJStatementType Type { get; private set; }
    public string Line { get; private set; }
    
    public CJStatement(CJStatementType type, string line)
    {
        Type = type;
        Line = line;
    }
    
}

public enum CJStatementType
{
    New,
    Add,
    Sub,    
    Mul,
    Div,
    Mod,    
    If,
    End,
    Return
}

public enum CJFunctionType
{
    External,
    Internal
}

public enum CJType
{
    i32,
    i64,
    f32,
    f64,    
}

