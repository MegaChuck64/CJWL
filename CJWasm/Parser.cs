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
        switch (type)
        {
            case "i32":
                return WasmValueType.I32;
            case "i64":
                return WasmValueType.I64;
            case "f32":
                return WasmValueType.F32;
            case "f64":
                return WasmValueType.F64;
            default:
                throw new Exception("Unknown type");
        }
    }
    public static byte GetValueType(string type)
    {
        switch (type)
        {
            case "i32":
                return Convert.ToByte(WasmValueType.I32);
            case "i64":
                return Convert.ToByte(WasmValueType.I64);
            case "f32":
                return Convert.ToByte(WasmValueType.F32);
            case "f64":
                return Convert.ToByte(WasmValueType.F64);
            default:
                throw new Exception("Invalid type");
        }
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
        switch (type)
        {
            case "void":
                return 0x40;
            case "i32":
                return 0x41;
            case "i64":
                return 0x42;
            case "f32":
                return 0x43;
            case "f64":
                return 0x44;
            default:
                throw new Exception("Invalid type");
        }
    }

    public static byte GetVariableAccessOpValue(string type)
    {
        switch (type)
        {
            case "get":
                return 0x20;
            case "set":
                return 0x21;
            default:
                throw new Exception("Invalid type");
        }
    }

    
    public static byte GetControlFlowOpValue(string type)
    {
        switch (type)
        {
            case "block":
                return 0x02;
            case "loop":
                return 0x03;
            case "if":
                return 0x04;
            case "else":
                return 0x05;
            case "end":
                return 0x0b;
            case "br":
                return 0x0c;
            case "br_if":
                return 0x0d;
            case "return":
                return 0x0f;
            default:
                throw new Exception("Invalid type");
        }
    }

    public static byte GetComparisonOpValue(string type)
    {
        switch (type)
        {
            case "i32.eqz":
                return 0x45;
            case "i32.eq":
                return 0x46;
            case "i32.ne":
                return 0x47;
            case "i32.lt_s":
                return 0x48;
            case "i32.lt_u":
                return 0x49;
            case "i32.gt_s":
                return 0x4a;
            case "i32.gt_u":
                return 0x4b;
            case "i32.le_s":
                return 0x4c;
            case "i32.le_u":
                return 0x4d;
            case "i32.ge_s":
                return 0x4e;
            case "i32.ge_u":
                return 0x4f;
            case "i64.eqz":
                return 0x50;
            case "i64.eq":
                return 0x51;
            case "i64.ne":
                return 0x52;
            case "i64.lt_s":
                return 0x53;
            case "i64.lt_u":
                return 0x54;
            case "i64.gt_s":
                return 0x55;
            case "i64.gt_u":
                return 0x56;
            case "i64.le_s":
                return 0x57;
            case "i64.le_u":
                return 0x58;
            case "i64.ge_s":
                return 0x59;
            case "i64.ge_u":
                return 0x5a;
            case "f32.eq":
                return 0x5b;
            case "f32.ne":
                return 0x5c;
            case "f32.lt":
                return 0x5d;
            case "f32.gt":
                return 0x5e;
            case "f32.le":
                return 0x5f;
            case "f32.ge":
                return 0x60;
            case "f64.eq":
                return 0x61;
            case "f64.ne":
                return 0x62;
            case "f64.lt":
                return 0x63;
            case "f64.gt":
                return 0x64;
            case "f64.le":
                return 0x65;
            case "f64.ge":
                return 0x66;                                    
            default:
                throw new Exception("Invalid type");
        }
    }
    public static byte GetNumericOpValue(string type)
    {
        switch (type)
        {
            case "i32.clz":
                return 0x67;
            case "i32.ctz":
                return 0x068;
            case "i32.popcnt":
                return 0x69;
            case "i32.add":
                return 0x6A;
            case "i32.sub":
                return 0x6B;
            case "i32.mul":
                return 0x6C;
            case "i32.div_s":
                return 0x6D;
            case "i32.div_u":
                return 0x6E;
            case "i32.rem_s":
                return 0x6F;
            case "i32.rem_u":
                return 0x70;
            case "i32.and":
                return 0x71;
            case "i32.or":
                return 0x72;
            case "i32.xor":
                return 0x73;
            case "i32.shl":
                return 0x74;
            case "i32.shr_s":
                return 0x75;
            case "i32.shr_u":
                return 0x76;
            case "i32.rotl":
                return 0x77;
            case "i32.rotr":
                return 0x78;
            case "i64.clz":
                return 0x79;
            case "i64.ctz":
                return 0x7A;
            case "i64.popcnt":
                return 0x7B;
            case "i64.add":
                return 0x7C;
            case "i64.sub":
                return 0x7D;
            case "i64.mul":
                return 0x7E;
            case "i64.div_s":
                return 0x7F;
            case "i64.div_u":
                return 0x80;
            case "i64.rem_s":
                return 0x81;
            case "i64.rem_u":
                return 0x82;
            case "i64.and":
                return 0x83;
            case "i64.or":
                return 0x84;
            case "i64.xor":
                return 0x85;
                case "i64.shl":
                return 0x86;
            case "i64.shr_s":
                return 0x87;

            case "i64.shr_u":

                return 0x88;
            case "i64.rotl":
                return 0x89;
            case "i64.rotr":
                return 0x8A;
            case "f32.abs":
                return 0x8B;
            case "f32.neg":
                return 0x8C;
            case "f32.ceil":
                return 0x8D;
            case "f32.floor":
                return 0x8E;
            case "f32.trunc":
                return 0x8F;
            case "f32.nearest":
                return 0x90;
            case "f32.sqrt":
                return 0x91;
            case "f32.add":
                return 0x92;
            case "f32.sub":
                return 0x93;
            case "f32.mul":
                return 0x94;
            case "f32.div":
                return 0x95;
            case "f32.min":
                return 0x96;
            case "f32.max":
                return 0x97;
            case "f32.copysign":
                return 0x98;
            case "f64.abs":
                return 0x99;
            case "f64.neg":
                return 0x9A;
            case "f64.ceil":
                return 0x9B;
            case "f64.floor":
                return 0x9C;
            case "f64.trunc":
                return 0x9D;
            case "f64.nearest":
                return 0x9E;
            case "f64.sqrt":
                return 0x9F;

            case "f64.add":
                return 0xA0;
            case "f64.sub":
                return 0xA1;
            case "f64.mul":
                return 0xA2;
            case "f64.div":
                return 0xA3;
            case "f64.min":
                return 0xA4;
            case "f64.max":
                return 0xA5;
            case "f64.copysign":
                return 0xA6;

            default:
                throw new Exception("Invalid type");
        }
    }
    

}