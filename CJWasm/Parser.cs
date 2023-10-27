using CJWasm.LEB128;
using CJWasm.Models;
using CJWasm.Sections;

namespace CJWasm;

public static class Parser
{
    public static Module Parse(List<string> lines)
    {
        var functionTypes = new Dictionary<string, FunctionType>();
        var functionBodies = new Dictionary<string, FunctionBody>();
        var functionLocalIndices = new Dictionary<string, Dictionary<string, int>>();
        
        var appStarted = false;
        lines = lines.Where(t => !string.IsNullOrWhiteSpace(t)).Select(c => c.Trim()).ToList();
        
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            
            if (!appStarted)
            {
                if (line.Equals("App"))
                {
                    appStarted = true;
                }
            }
            else
            {
                if (line.StartsWith(@"//"))
                {
                    //comment
                    continue;
                }
                else if (line.StartsWith("external"))
                {
                    var splt = line.Split(' ', '(', ')');
                    var returnType = splt[1];
                    var name = splt[2];
                    var paramsString = line
                        .Substring(
                            line.IndexOf('(') + 1,
                            line.IndexOf(')') - line.IndexOf('(') - 1);

                    var parms = paramsString.Split(new char[] { ',' , ' '}, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var parmTypes = new List<string>();
                    if (parms.Length > 0)
                    {
                        functionLocalIndices[name] = new Dictionary<string, int>();
                    }
                    for (int j = 0; j < parms.Length; j++)
                    {
                        parmTypes.Add(parms[j]);
                        j++;                        
                        functionLocalIndices[name].Add(parms[j], parmTypes.Count - 1);     
                    }

                    var paramBytes = new List<byte>();
                    var returnBytes = new List<byte>
                    {
                        GetValueType(returnType)
                    };
                    foreach (var paramType in parmTypes)
                    {
                        var parmTypeByte = GetValueType(paramType);
                        paramBytes.Add(parmTypeByte);
                    }
                    functionTypes.Add(name, new FunctionType(paramBytes, returnBytes));       
                    i++;
                    line = lines[i];
                    if (!line.StartsWith("{"))
                    {
                        throw new Exception("Expected '{'");
                    }
                    i++;
                    line = lines[i];
                    var waitingForBlockClose = 0;
                    while (!line.StartsWith("}"))
                    {
                        
                        if (lines.Count < i+1)
                        {
                            throw new Exception("Expected '}'");
                        }
                        
                        line = lines[i];
                        if (line.StartsWith(@"//"))
                            continue;

                        if (line.StartsWith("{"))
                            waitingForBlockClose++;

                        if (line.StartsWith("}"))
                        {
                            if (waitingForBlockClose > 0)
                                waitingForBlockClose--;
                            else
                                break;
                        }
                        


                        CJStatementType statementType;
                        if (line.StartsWith("new"))
                            statementType = CJStatementType.New;
                        else if (line.StartsWith("add"))
                            statementType = CJStatementType.Add;
                        else if (line.StartsWith("sub"))
                            statementType = CJStatementType.Sub;
                        else if (line.StartsWith("mul"))
                            statementType = CJStatementType.Mul;
                        else if (line.StartsWith("div"))
                            statementType = CJStatementType.Div;
                        else if (line.StartsWith("mod"))
                            statementType = CJStatementType.Mod;
                        else if (line.StartsWith("if"))
                            statementType = CJStatementType.If;
                        else if (line.StartsWith("return"))
                            statementType = CJStatementType.Return;
                        else if (line.StartsWith("end"))
                            statementType = CJStatementType.End;
                        else
                            throw new Exception("Unknown statement type");
                        
                        var bytes = 
                            ParseFunctionBodyLine(
                                statementType, 
                                name, 
                                line, 
                                ref functionLocalIndices, 
                                out List<Local> locals);
                        
                        if (functionBodies.ContainsKey(name))
                        {
                            functionBodies[name].Body.AddRange(bytes);
                            functionBodies[name].Locals.AddRange(locals);
                        }
                        else
                        {
                            functionBodies.Add(name, new FunctionBody(bytes, locals));
                        }
                        
                        i++;
                    }

                }
            }
        }

        var typeSection = 
            new TypeSection(functionTypes.Values.ToList());

        var functionSection =
            new FunctionSection(
                functionTypes
                .Select((k, i) => i).ToList());

        var exportSection = new ExportSection(
            functionTypes
            .Select((k, i) => new Export(k.Key, ExportType.Function, i))
            .ToList());

        var codeSection = new CodeSection(functionBodies.Values.ToList());

        var module = new Module(new List<Section>
        {
            typeSection,
            functionSection,
            exportSection,
            codeSection
        });


        return module;
    }

    public static List<byte> ParseFunctionBodyLine(
        CJStatementType statementType,
        string functionName,
        string line, 
        ref Dictionary<string, Dictionary<string, int>> parms,
        out List<Local> locals)
    {
        var bytes = new List<byte>();
        locals = new List<Local>();

        switch (statementType)
        {
            case CJStatementType.New:
                var newSplit = line.Split(' ');
                var newType = newSplit[1];
                var newName = newSplit[2];
                var newEqual = newSplit[3];
                var newVal = newSplit[4];
                locals.Add(new Local(GetValueType(newType)));
                parms[functionName][newName] = parms[functionName].Count;
                //i32.const val
                bytes.Add(GetConstOpValue(newType));
                bytes.AddRange(
                    GetValueBytes(ParseValueType(newType), newVal));

                //set local index
                bytes.Add(0x21);
                bytes.Add((byte)parms[functionName][newName]);
                break;
            case CJStatementType.Add:
                var addSplit = line.Split(' ');
                var addType = addSplit[1];
                var addName = addSplit[2];
                var addEqual = addSplit[3];
                var addLeft = addSplit[4];
                var addRight = addSplit[5];
                //local.get left
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][addLeft]);
                //local.get right
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][addRight]);

                //add
                bytes.Add(GetNumericOpValue(addType + ".add"));

                //local.set index
                bytes.Add(GetVariableAccessOpValue("set"));
                bytes.Add((byte)parms[functionName][addName]);

                break;
            case CJStatementType.Sub:
                var subSplit = line.Split(' ');
                var subType = subSplit[1];
                var subName = subSplit[2];
                var subEqual = subSplit[3];
                var subLeft = subSplit[4];
                var subRight = subSplit[5];
                //local.get left
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][subLeft]);
                //local.get right
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][subRight]);

                //sub
                bytes.Add(GetNumericOpValue(subType + ".sub"));

                //local.set index
                bytes.Add(GetVariableAccessOpValue("set"));
                bytes.Add((byte)parms[functionName][subName]);
                
                break;

            case CJStatementType.Mul:
                var mulSplit = line.Split(' ');
                var mulType = mulSplit[1];
                var mulName = mulSplit[2];
                var mulEqual = mulSplit[3];
                var mulLeft = mulSplit[4];
                var mulRight = mulSplit[5];
                //local.get left
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][mulLeft]);
                //local.get right
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][mulRight]);

                //mul
                bytes.Add(GetNumericOpValue(mulType + ".mul"));

                //local.set index
                bytes.Add(GetVariableAccessOpValue("set"));
                bytes.Add((byte)parms[functionName][mulName]);
                
                break;

            case CJStatementType.Div:
                var divSplit = line.Split(' ');
                var divType = divSplit[1];
                var divName = divSplit[2];
                var divEqual = divSplit[3];
                var divLeft = divSplit[4];
                var divRight = divSplit[5];
                //local.get left
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][divLeft]);
                //local.get right
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][divRight]);

                //div
                bytes.Add(GetNumericOpValue(divType + ".div_s"));

                //local.set index
                bytes.Add(GetVariableAccessOpValue("set"));
                bytes.Add((byte)parms[functionName][divName]);

                break;
            case CJStatementType.Mod:
                var modSplit = line.Split(' ');
                var modType = modSplit[1];
                var modName = modSplit[2];
                var modEqual = modSplit[3];
                var modLeft = modSplit[4];
                var modRight = modSplit[5];
                //local.get left
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][modLeft]);
                //local.get right
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][modRight]);

                //mod
                bytes.Add(GetNumericOpValue(modType + ".rem_s"));

                //local.set index
                bytes.Add(GetVariableAccessOpValue("set"));
                bytes.Add((byte)parms[functionName][modName]);

                break;
            case CJStatementType.If:
                var ifSplit = line.Split(' ');
                var ifType = ifSplit[1];
                var ifOp = ifSplit[2];
                var ifLeft = ifSplit[3];
                var ifRight = ifSplit[4];


                //push left var to stack
                //push right var to stack
                //conditional op

                //if
                //body
                //end
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][ifLeft]);
                
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][ifRight]);

                bytes.Add(GetComparisonOpValue(ifType + "." + ifOp));

                bytes.Add(GetControlFlowOpValue("if"));
                bytes.Add(GetConstOpValue("void"));
                break;
            case CJStatementType.End:
                bytes.Add(GetControlFlowOpValue("end"));
                break;
            case CJStatementType.Return:
                var returnSplit = line.Split(' ');
                //var returnType = returnSplit[1];
                var returnName = returnSplit[1];

                //local.get index
                bytes.Add(GetVariableAccessOpValue("get"));
                bytes.Add((byte)parms[functionName][returnName]);
                break;
            default:
                throw new Exception("unknown statement type");
        }

        return bytes;
    }
    
    public static WasmValueType ParseValueType(string type)
    {
        return type switch
        {
            "i32" => WasmValueType.I32,
            "i64" => WasmValueType.I64,
            "f32" => WasmValueType.F32,
            "f64" => WasmValueType.F64,
            _ => throw new Exception("Unknown type"),
        };
    }
    public static byte GetValueType(string type)
    {
        return type switch
        {
            "i32" => Convert.ToByte(WasmValueType.I32),
            "i64" => Convert.ToByte(WasmValueType.I64),
            "f32" => Convert.ToByte(WasmValueType.F32),
            "f64" => Convert.ToByte(WasmValueType.F64),
            _ => throw new Exception("Invalid type"),
        };
    }

    public static List<byte> GetValueBytes(WasmValueType type, string value)
    {
        var bytes = new List<byte>();
        switch (type)
        {
            case WasmValueType.I32:
                bytes.AddLEB128Unsigned(Convert.ToByte(int.Parse(value)));
                break;
            case WasmValueType.I64:
                bytes.AddLEB128Unsigned(Convert.ToByte(long.Parse(value)));
                break;
            case WasmValueType.F32:
                bytes.AddLEB128Unsigned(Convert.ToByte(float.Parse(value)));
                break;
            case WasmValueType.F64:
                bytes.AddLEB128Unsigned(Convert.ToByte(double.Parse(value)));
                break;
            default:
                throw new Exception("Unknown type");
        }

        return bytes;
    }
    public static byte GetConstOpValue(string type)
    {
        return type switch
        {
            "void" => 0x40,
            "i32" => 0x41,
            "i64" => 0x42,
            "f32" => 0x43,
            "f64" => 0x44,
            _ => throw new Exception("Invalid type"),
        };
    }

    public static byte GetVariableAccessOpValue(string type)
    {
        return type switch
        {
            "get" => 0x20,
            "set" => 0x21,
            _ => throw new Exception("Invalid type"),
        };
    }

    
    public static byte GetControlFlowOpValue(string type)
    {
        return type switch
        {
            "block" => 0x02,
            "loop" => 0x03,
            "if" => 0x04,
            "else" => 0x05,
            "end" => 0x0b,
            "br" => 0x0c,
            "br_if" => 0x0d,
            "return" => 0x0f,
            _ => throw new Exception("Invalid type"),
        };
    }

    public static byte GetComparisonOpValue(string type)
    {
        return type switch
        {
            "i32.eqz" => 0x45,
            "i32.eq" => 0x46,
            "i32.ne" => 0x47,
            "i32.lt_s" => 0x48,
            "i32.lt_u" => 0x49,
            "i32.gt_s" => 0x4a,
            "i32.gt_u" => 0x4b,
            "i32.le_s" => 0x4c,
            "i32.le_u" => 0x4d,
            "i32.ge_s" => 0x4e,
            "i32.ge_u" => 0x4f,
            "i64.eqz" => 0x50,
            "i64.eq" => 0x51,
            "i64.ne" => 0x52,
            "i64.lt_s" => 0x53,
            "i64.lt_u" => 0x54,
            "i64.gt_s" => 0x55,
            "i64.gt_u" => 0x56,
            "i64.le_s" => 0x57,
            "i64.le_u" => 0x58,
            "i64.ge_s" => 0x59,
            "i64.ge_u" => 0x5a,
            "f32.eq" => 0x5b,
            "f32.ne" => 0x5c,
            "f32.lt" => 0x5d,
            "f32.gt" => 0x5e,
            "f32.le" => 0x5f,
            "f32.ge" => 0x60,
            "f64.eq" => 0x61,
            "f64.ne" => 0x62,
            "f64.lt" => 0x63,
            "f64.gt" => 0x64,
            "f64.le" => 0x65,
            "f64.ge" => 0x66,
            _ => throw new Exception("Invalid type"),
        };
    }
    public static byte GetNumericOpValue(string type)
    {
        return type switch
        {
            "i32.clz" => 0x67,
            "i32.ctz" => 0x068,
            "i32.popcnt" => 0x69,
            "i32.add" => 0x6A,
            "i32.sub" => 0x6B,
            "i32.mul" => 0x6C,
            "i32.div_s" => 0x6D,
            "i32.div_u" => 0x6E,
            "i32.rem_s" => 0x6F,
            "i32.rem_u" => 0x70,
            "i32.and" => 0x71,
            "i32.or" => 0x72,
            "i32.xor" => 0x73,
            "i32.shl" => 0x74,
            "i32.shr_s" => 0x75,
            "i32.shr_u" => 0x76,
            "i32.rotl" => 0x77,
            "i32.rotr" => 0x78,
            "i64.clz" => 0x79,
            "i64.ctz" => 0x7A,
            "i64.popcnt" => 0x7B,
            "i64.add" => 0x7C,
            "i64.sub" => 0x7D,
            "i64.mul" => 0x7E,
            "i64.div_s" => 0x7F,
            "i64.div_u" => 0x80,
            "i64.rem_s" => 0x81,
            "i64.rem_u" => 0x82,
            "i64.and" => 0x83,
            "i64.or" => 0x84,
            "i64.xor" => 0x85,
            "i64.shl" => 0x86,
            "i64.shr_s" => 0x87,
            "i64.shr_u" => 0x88,
            "i64.rotl" => 0x89,
            "i64.rotr" => 0x8A,
            "f32.abs" => 0x8B,
            "f32.neg" => 0x8C,
            "f32.ceil" => 0x8D,
            "f32.floor" => 0x8E,
            "f32.trunc" => 0x8F,
            "f32.nearest" => 0x90,
            "f32.sqrt" => 0x91,
            "f32.add" => 0x92,
            "f32.sub" => 0x93,
            "f32.mul" => 0x94,
            "f32.div" => 0x95,
            "f32.min" => 0x96,
            "f32.max" => 0x97,
            "f32.copysign" => 0x98,
            "f64.abs" => 0x99,
            "f64.neg" => 0x9A,
            "f64.ceil" => 0x9B,
            "f64.floor" => 0x9C,
            "f64.trunc" => 0x9D,
            "f64.nearest" => 0x9E,
            "f64.sqrt" => 0x9F,
            "f64.add" => 0xA0,
            "f64.sub" => 0xA1,
            "f64.mul" => 0xA2,
            "f64.div" => 0xA3,
            "f64.min" => 0xA4,
            "f64.max" => 0xA5,
            "f64.copysign" => 0xA6,
            _ => throw new Exception("Invalid type"),
        };
    }
    

}