#if DEBUG
args = new string[]
{
    "TestScript.cj"
};
#endif

if (args.Length == 0)
{
    Console.WriteLine("No file specified.");
    return;
}


var lines = new string[] { };//File.ReadAllLines(args[0]);


void Run()
{
    int lineCount = 0;
    int scope = 0;
    //list index is scope level,
    //dict key is variable name,
    //dict value is variable value 
    var variables = new List<Dictionary<string, object?>>();


    //list index is scope level
    //dict key is routine name 
    //dict value is list of lines in that routine
    var routines = new List<Dictionary<string, List<string>>>();
    bool appCreated = false;

    var res = EvaluateExpression(new string[] { "1", "+", "2", "-", "2", "+", "5" });

    var gg = 0;
    foreach (var line in lines)
    {
        if (!appCreated)
        {
            if (line.Trim() == "App:")
            {
                appCreated = true;
                scope++;
                variables.Add(new Dictionary<string, object?>());
            }
        }
        else
        {
            if (line.StartsWith("var"))
            {
                var splt = line.Split(' ');
                if (splt[0] == "var")
                {
                    var name = splt[1];
                    if (splt[2] == "=")
                    {
                        var expressionTokens = splt.Skip(3).ToArray();

                        if (expressionTokens.Length == 0)
                            throw new CJException("No expression specified after '='.", lineCount);


                        var expressionResult = EvaluateExpression(expressionTokens);

                        //add variable to scope
                        variables[scope].Add(name, expressionResult);
                    }
                    else
                    {
                        throw new CJException("incomplete expression", lineCount);
                    }

                }
            }
            else if (line.StartsWith("jump"))
            {
                var splt = line.Split(' ');
                var labelDest = splt[1];

                if (splt.Length > 2)
                {
                    //conditional label

                }
                else
                {
                    //jumpt to label
                }

            }
        }

        lineCount++;
    }
}


///add references to jump blocks
void FirstPass(List<string> lines)
{
    var appStarted = false;
    var lineNum = 0;
    var scope = 0;
    foreach (var line in lines)
    {        
        if (!appStarted)
        {
            if (line == "App:")
            {
                appStarted = true;
                scope++;
            }
            else
                throw new CJException("invalid start to app", lineNum);                        
        }
        else
        {
            if (line.EndsWith(':'))
            {
                var split = line.Split(' ');
                if (split.Length == 1)
                {
                    
                }

                scope++;                
            }
        }

        lineNum++;
    }
}

//1 + 2 - 2 + 5
//exp == 6
//1
//+
//2
//-
//2
//+
//5
object? EvaluateExpression(string[] tokens, int lineNum)
{
    if (IsOperator(tokens[0]))
        throw new CJException("Expression cannot start with an operator.", lineNum);

    object? val = null;
    var op = string.Empty;
    var valType = string.Empty;
    foreach (var token in tokens)
    {
        if (string.IsNullOrEmpty(valType))
        {
            valType = ParseValueType(token);
            val = ParseValue(token);
        }
        else
        {
            if (IsOperator(token))
            {
                op = token;
            }
            else
            {
                var newValType = ParseValueType(token);
                var newVal = ParseValue(token);
                if (valType != newValType)
                {
                    throw new CJException($"Cannot perform operation on values of different types: {valType} and {newValType}", lineNum);
                }
                else
                {
                    val = op switch
                    {
                        "+" => valType switch
                        {
                            "int" => Convert.ToInt32(val) + Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) + Convert.ToSingle(newVal),
                            "string" => Convert.ToString(val) + Convert.ToString(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineNum)
                        },
                        "-" => valType switch
                        {
                            "int" => Convert.ToInt32(val) - Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) - Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineNum)
                        },
                        "*" => valType switch
                        {
                            "int" => Convert.ToInt32(val) * Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) * Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineNum)
                        },
                        "/" => valType switch
                        {
                            "int" => Convert.ToInt32(val) / Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) / Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineNum)
                        },
                        "%" => valType switch
                        {
                            "int" => Convert.ToInt32(val) * Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) * Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineNum)
                        },
                        _ => throw new CJException($"Unknown operator: {op}", lineNum)
                    };
                }
            }
        }
    }

    return val;
    
}

string ParseValueType(string val)
{
    if (val.StartsWith("\"") && val.EndsWith("\""))
    {
        return "string";
    }
    else if (int.TryParse(val, out var intValue))
    {
        return "int";
    }
    else if (float.TryParse(val, out var floatValue))
    {
        return "float";
    }
    else if (bool.TryParse(val, out var boolValue))
    {
        return "bool";
    }
    else if (variables[scope].TryGetValue(val, out object? varVal))
    {
        return varVal.GetType().Name;
    }
    else
    {
        throw new CJException($"Unknown value type: {val}", lineCount);
    }
}
object? ParseValue(string val)
{
    if (val.StartsWith("\"") && val.EndsWith("\""))
    {
        return val.Substring(1, val.Length - 2);
    }
    else if (int.TryParse(val, out var intValue))
    {
        return intValue;
    }
    else if (float.TryParse(val, out var floatValue))
    {
        return floatValue;
    }
    else if (bool.TryParse(val, out var boolValue))
    {
        return boolValue;
    }
    else if (variables[scope].TryGetValue(val, out object? varVal))
    {
        return varVal;
    }
    else
    {
        throw new CJException($"Unknown value type: {val}", lineCount);
    }
}


bool IsOperator(string token)
{
    return token == "+" || token == "-" || token == "*" || token == "/" || token == "%" || token == "&&" || token == "||";
}

object EvaluateExpressionChunk(string left, string op, string right)
{
    if (int.TryParse(left, out var leftInt) && int.TryParse(right, out var rightInt))
    {
        return op switch
        {
            "+" => leftInt + rightInt,
            "-" => leftInt - rightInt,
            "*" => leftInt * rightInt,
            "/" => leftInt / rightInt,
            "%" => leftInt % rightInt,
            _ => throw new CJException($"Unknown operator: {op}", lineCount)
        };
    }
    else if (float.TryParse(left, out var leftFloat) && float.TryParse(right, out var rightFloat))
    {
        return op switch
        {
            "+" => leftFloat + rightFloat,
            "-" => leftFloat - rightFloat,
            "*" => leftFloat * rightFloat,
            "/" => leftFloat / rightFloat,
            "%" => leftFloat % rightFloat,
            _ => throw new CJException($"Unknown operator: {op}", lineCount)
        };
    }
    else if (left.StartsWith("\"") && left.EndsWith("\"") && right.StartsWith("\"") && right.EndsWith("\""))
    {
        return op switch
        {
            "+" => left.Substring(1, left.Length - 2) + right.Substring(1, right.Length - 2),
            _ => throw new CJException($"Unknown operator: {op}", lineCount)
        };
    }
    else if (bool.TryParse(left, out var leftBool) && bool.TryParse(right, out var rightBool))
    {
        return op switch
        {
            "&&" => leftBool && rightBool,
            "||" => leftBool || rightBool,
            _ => throw new CJException($"Unknown operator: {op}", lineCount)
        };
    }
    else if (variables[scope].TryGetValue(left, out object? leftVar) && variables[scope].TryGetValue(right, out object? rightVar))
    {
        return op switch
        {
            "+" => EvaluateExpressionChunk(leftVar.ToString(), op, rightVar.ToString()),
            "-" => EvaluateExpressionChunk(leftVar.ToString(), op, rightVar.ToString()),
            "*" => EvaluateExpressionChunk(leftVar.ToString(), op, rightVar.ToString()),
            "/" => EvaluateExpressionChunk(leftVar.ToString(), op, rightVar.ToString()),
            "%" => EvaluateExpressionChunk(leftVar.ToString(), op, rightVar.ToString()),
            "&&" => EvaluateExpressionChunk(leftVar.ToString(), op, rightVar.ToString()),
            "||" => EvaluateExpressionChunk(leftVar.ToString(), op, rightVar.ToString()),
            _ => throw new CJException($"Unknown operator: {op}", lineCount)
        };

    }
    else
    {
        throw new CJException($"Unknown value type: {left} {op} {right}", lineCount);
    }

}



public class CJException : Exception
{
    public CJException(string message, int line) : base($"{{{line + 1}}}: {message}")
    {

    }


}