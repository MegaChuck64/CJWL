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

int lineCount = 0;
int scope = 0;
//list index is scope level,
//dict key is variable name,
//dict value is variable value
var variables = new List<Dictionary<string, object>>();
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
            variables.Add(new Dictionary<string, object>());
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

                    if (expressionTokens.Length == 1)
                    {
                        var value = expressionTokens[0];
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            variables[scope].Add(name, value.Substring(1, value.Length - 2));
                        }
                        else if (int.TryParse(value, out var intValue))
                        {
                            variables[scope].Add(name, intValue);
                        }
                        else if (float.TryParse(value, out var floatValue))
                        {
                            variables[scope].Add(name, floatValue);
                        }
                        else if (bool.TryParse(value, out var boolValue))
                        {
                            variables[scope].Add(name, boolValue);
                        }
                        else if (variables[scope].TryGetValue(value, out object? varVal))
                        {
                            variables[scope].Add(name, varVal);
                        }
                        else
                        {
                            throw new CJException($"Unknown value type: {value}", lineCount);
                        }
                    }
                    else
                    {

                    }
                }

            }
        }
    }

    lineCount++;
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
object? EvaluateExpression(string[] tokens)
{
    if (IsOperator(tokens[0]))
        throw new CJException("Expression cannot start with an operator.", lineCount);

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
                    throw new CJException($"Cannot perform operation on values of different types: {valType} and {newValType}", lineCount);
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
                            _ => throw new CJException($"Unknown value type: {valType}", lineCount)
                        },
                        "-" => valType switch
                        {
                            "int" => Convert.ToInt32(val) - Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) - Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineCount)
                        },
                        "*" => valType switch
                        {
                            "int" => Convert.ToInt32(val) * Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) * Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineCount)
                        },
                        "/" => valType switch
                        {
                            "int" => Convert.ToInt32(val) / Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) / Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineCount)
                        },
                        "%" => valType switch
                        {
                            "int" => Convert.ToInt32(val) * Convert.ToInt32(newVal),
                            "float" => Convert.ToSingle(val) * Convert.ToSingle(newVal),
                            _ => throw new CJException($"Unknown value type: {valType}", lineCount)
                        },
                        _ => throw new CJException($"Unknown operator: {op}", lineCount)
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