namespace CJ_Lang;

public class Interpreter
{
    public static void Execute(string code)
    {

    }
}

public class CJApp
{
    public Dictionary<string, CJObject> Variables { get; set; }
    public Dictionary<string, CJRoutine> Routines { get; set; }

    public CJApp()
    {
        Variables = new Dictionary<string, CJObject>();
        Routines = new Dictionary<string, CJRoutine>();
    }

    public CJObject GetVariable(string name)
    {
        return Variables[name];
    }

    public CJRoutine GetRoutine(string name)
    {
        return Routines[name];
    }

    public void AddVariable(string name, CJObject obj)
    {
        Variables.Add(name, obj);
    }

    public void AddRoutine(string name, CJRoutine routine)
    {
        Routines.Add(name, routine);
    }
    
}

public class CJRoutine
{
    public List<string> Lines { get; set; }

    public CJRoutine()
    {
        Lines = new List<string>();
    }

    public CJRoutine(string[] lines)
    {
        Lines = new List<string>(lines);
    }
}

public abstract class CJStatement
{
    public string Code { get; set; }

    public CJStatement(string code)
    {
        Code = code;
    }
    
    public abstract void Execute(CJRoutine routine);
}

public class CJObject
{
    public CJType Type { get; set; }
    public object? Value { get; set; }

    public CJObject(CJType type, string value)
    {
        Type = type;
        Value = type switch
        {
            CJType.CJString => value,
            CJType.CJInt => int.Parse(value),
            CJType.CJFloat => float.Parse(value),
            CJType.CJBool => bool.Parse(value),
            
            _ => default
        };
        Value = value;
    }

 
}

public enum CJType
{
    CJString,
    CJInt,
    CJFloat,
    CJBool,
    CJStringArray,
    CJIntArray,
    CJFloatArray,
    CJBoolArray,    
}