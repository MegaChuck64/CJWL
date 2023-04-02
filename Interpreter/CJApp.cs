namespace Interpreter;

public class CJApp
{
    public Dictionary<string, CJClass> Classes { get; set; }
    public CJMethod Entry { get; set; }

}

public class CJClass
{
    public Dictionary<string, string> Properties { get; set; }
    public Dictionary<string, CJMethod> Methods { get; set; }
}

public class CJMethod
{
    public Dictionary<string, string> Parameters { get; set; }
    public List<CJStatement> Statements { get; set; }
    public Dictionary<string, object> LocalVariables { get; set; }
}


public class CJStatement
{
    public StatementType Type { get; set; }
    public string Code { get; set; }

    public CJStatement(string code)
    {
        Code = code;
        var split = Code.Trim().Split(' ');
        Type = split[0] switch
        {
            "app" => StatementType.AppDeclaration,
            "entry" => StatementType.EntryDeclaration,
            "class" => StatementType.ClassDeclaration,
            "void" => StatementType.MethodDeclaration,
            "if" => StatementType.IfDeclaration,
            "elif" => StatementType.ElifDeclaration,
            "else" => StatementType.ElseDeclaration,
            "foreach" => StatementType.ForeachDeclaration,
            "while" => StatementType.WhileDeclaration,
            _ when split[^1].EndsWith(":") => StatementType.MethodCall,
            _ when split[^1].EndsWith(")") => StatementType.MethodCall,
            _ => StatementType.FieldDeclaration
        };

    }
    public object Execute()
    {
        string[] split;
        switch (Type)
        {
            case StatementType.AppDeclaration:
                return Code.Split(new char[] { ' ', ':' })[1];
            case StatementType.EntryDeclaration:
                split = Code.Split(new char[] { ' ', ':', ',', '(', ')' });
                var entryArgs = new Dictionary<string, string>();
                for (int i = 1; i < split.Length; i+=2)
                {
                    var typ = split[i];
                    var nam = split[i + 1];

                    entryArgs.Add(nam, typ);
                }
                return entryArgs;
            case StatementType.ClassDeclaration:
                split = Code.Split(new char[] { ' ', ':', '(', ')', ',' });
                var className = split[0];
                var classObj = new CJClass();
                var props = new Dictionary<string, string>();
                for (int i = 1; i < split.Length; i += 2)
                {
                    var typ = split[i];
                    var nam = split[i + 1];
                    props.Add(nam, typ);
                }
                classObj.Methods = new Dictionary<string, CJMethod>();
                classObj.Properties = props;
                return (className, classObj);

            case StatementType.FieldDeclaration:

                return Code;

            case StatementType.MethodDeclaration:
                split = Code.Split(new char[] { ' ', ':', '(', ')' });
                var returnType = split[0];
                var name = split[1];
                var method = new CJMethod()
                {
                    LocalVariables = new Dictionary<string, object>(),
                    Statements = new List<CJStatement>(),
                };
                var parms = new Dictionary<string, string>();
                for (int i = 2; i < split.Length; i+=2)
                {
                    var typ = split[i];
                    var nam = split[i + 1];
                    parms.Add(nam, typ);
                }

                method.Parameters = parms;
                return (name, returnType, method);
                
            //case StatementType.MethodCall:
            //    break;
            //case StatementType.ForeachDeclaration:
            //    break;
            //case StatementType.IfDeclaration:
            //    break;
            //case StatementType.ElifDeclaration:
            //    break;
            //case StatementType.ElseDeclaration:
            //    break;
            default:
                return Code;
        }
    }

    
}


public enum StatementType
{
    AppDeclaration,
    EntryDeclaration,
    ClassDeclaration,
    FieldDeclaration,
    MethodDeclaration,
    MethodCall,
    ForeachDeclaration,
    IfDeclaration,
    ElifDeclaration,
    ElseDeclaration,
    WhileDeclaration,
}
