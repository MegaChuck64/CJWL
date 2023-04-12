// See https://aka.ms/new-console-template for more information
using CJWasm.LEB128;
using System.Text;

Console.WriteLine("Hello, World!");

using var fs = new FileStream(@"Y:\source\2023\March\CJWL\CJWasm\output.wasm", FileMode.OpenOrCreate);
var bytes = Run();
fs.Write(bytes);



byte[] Run()
{

    var parms = new List<byte>();
    parms.AddLEB128Unsigned(Convert.ToByte(ValueType.I32));
    parms.AddLEB128Unsigned(Convert.ToByte(ValueType.I32));

    var returns = new List<byte>();
    returns.AddLEB128Unsigned(Convert.ToByte(ValueType.I32));

    var funcTypes = new List<FunctionType>
    {
        new FunctionType(parms, returns)
    };

    var typeSection = new TypeSection(funcTypes);

    //index 0
    var functionSection = new FunctionSection(new List<int> { 0 });

    var exports = new List<Export>
    {
        new Export("addTwoPlusTen", ExportType.Function, 0),
    };
    var exportSection = new ExportSection(exports);


    var funcBodies = new List<FunctionBody>()
    {
        new FunctionBody(
            new List<byte>()
            {
                0x41, 0x0A, //i32.const 10
                
                0x21, 0x02, //local.set 2

                0x42, 0xcc, 0x21, //i64.const cc21 = 4300 //todo: use LEB128 class instead of hardcoding
                
                0x21, 0x03, //local.set 3
                
                0x20, 0x00, //local.get 0
                
                0x20, 0x01, //local.get 1
                
                0x6a, //i32.add
                
                0x20, 0x02, //local.get 2
                
                0x6a, //i32.add                            
            },
        
            new List<Local>
            {
                new Local(1, (byte)ValueType.I32),
                new Local(1, (byte)ValueType.I64),
            }
        )
    };
    var codeSection = new CodeSection(funcBodies);
    

    var module = new Module(new List<Section>
    {
        typeSection,
        functionSection,
        exportSection,
        codeSection
    });


    return module.ToBytes();
}


public class Module
{
    public List<Section> Sections { get; private set; }
    
    public Module(List<Section> sections)
    {
        Sections = sections;
    }

    public byte[] ToBytes()
    {
        var bytes = new List<byte>();
        //wasm magic number
        bytes.AddRange(new byte[] { 0x00, 0x61, 0x73, 0x6D });
        //wasm version
        bytes.AddRange(new byte[] { 0x01, 0x00, 0x00, 0x00 });
        
        foreach (var section in Sections)
        {
            bytes.AddRange(section.GetBytes());
        }

        return bytes.ToArray();
    }
}


public abstract class Section
{
    public abstract byte[] GetBytes();
}

public class TypeSection : Section
{
    public List<FunctionType> FunctionTypes { get; private set; }
    public TypeSection(List<FunctionType> functionTypes)
    {
        FunctionTypes = functionTypes;
        
    }

    public override byte[] GetBytes()
    {
        var output = new List<byte>();
        output.AddLEB128Unsigned(Convert.ToByte(FunctionTypes.Count));
        
        foreach (var functionType in FunctionTypes)
        {
            output.AddLEB128Unsigned(0x60);
            output.AddLEB128Unsigned(Convert.ToByte(functionType.Parameters.Count));
            output.AddRange(functionType.Parameters);
            output.AddLEB128Unsigned(Convert.ToByte(functionType.Results.Count));
            output.AddRange(functionType.Results);
        }

        var sectionSize = output.Count;
        
        output.InsertLEB128Unsigned(0, Convert.ToByte(SectionType.Type));
        output.InsertLEB128Unsigned(1, Convert.ToByte(sectionSize));

        return output.ToArray();
    }
}

public class FunctionSection : Section
{
    public List<int> SignatureIndexes { get; private set; }
    public FunctionSection(List<int> signatureIndexes)
    {
        SignatureIndexes = signatureIndexes;
    }

    public override byte[] GetBytes()
    {
        var output = new List<byte>();
        output.AddLEB128Unsigned(Convert.ToByte(SignatureIndexes.Count));

        foreach (var signatureIndex in SignatureIndexes)
        {
            output.AddLEB128Unsigned(Convert.ToByte(signatureIndex));
        }

        var sectionSize = output.Count;

        output.InsertLEB128Unsigned(0, Convert.ToByte(SectionType.Function));
        output.InsertLEB128Unsigned(1, Convert.ToByte(sectionSize));

        return output.ToArray();
    }
}


public class ExportSection : Section
{
    public List<Export> Exports { get; private set; }
    public ExportSection(List<Export> exports)
    {
        Exports = exports;
    }

    public override byte[] GetBytes()
    {
        var output = new List<byte>();
        output.AddLEB128Unsigned(Convert.ToByte(Exports.Count));

        foreach (var export in Exports)
        {
            output.AddLEB128Unsigned(Convert.ToByte(export.Name.Length));
            output.AddRange(Encoding.UTF8.GetBytes(export.Name));
            output.Add((byte)export.Type);
            output.AddLEB128Unsigned(Convert.ToByte(export.Index));
        }

        var sectionSize = output.Count;

        output.InsertLEB128Unsigned(0, Convert.ToByte(SectionType.Export));
        output.InsertLEB128Unsigned(1, Convert.ToByte(sectionSize));

        return output.ToArray();
    }
}

public class CodeSection : Section
{
    public List<FunctionBody> FunctionBodies { get; private set; }
    public CodeSection(List<FunctionBody> functionBodies)
    {
        FunctionBodies = functionBodies;
    }

    public override byte[] GetBytes()
    {
        var output = new List<byte>();
        output.AddLEB128Unsigned(Convert.ToByte(FunctionBodies.Count));

        foreach (var functionBody in FunctionBodies)
        {
            var localTypes = functionBody.Locals.GroupBy(t => t.Type).ToArray();

            var body = functionBody.Body;
            var bodySize = body.Count + (localTypes.Length * 2) + 1 + 1; //add function end and locals and local count here
            output.AddLEB128Unsigned(Convert.ToByte(bodySize));
            //unique local types            
            output.AddLEB128Unsigned(Convert.ToByte(localTypes.Length));
            foreach (var local in localTypes)
            {
                output.AddLEB128Unsigned(Convert.ToByte(local.Count()));
                output.AddLEB128Unsigned(Convert.ToByte(local.Key));                
            }
            output.AddRange(body.ToArray());            
            //end of function body
            output.Add(0x0b);
        }

        var sectionSize = output.Count;


        output.InsertLEB128Unsigned(0, Convert.ToByte(SectionType.Code));
        output.InsertLEB128Unsigned(1, Convert.ToByte(sectionSize));

        return output.ToArray();
    }
}

public class FunctionBody
{
    public List<byte> Body { get; private set; }
    public List<Local> Locals { get; private set; }
    
    public FunctionBody(List<byte> body, List<Local> locals)
    {
        Body = body;
        Locals = locals;
    }
}


public class FunctionType
{
    public List<byte> Parameters { get; private set; }
    public List<byte> Results { get; private set; }
    public FunctionType(List<byte> parameters, List<byte> results)
    {
        Parameters = parameters;
        Results = results;
    }
}

public class Export
{
    public string Name { get; private set; }
    public ExportType Type { get; private set; }
    public int Index { get; private set; }
    public Export(string name, ExportType type, int index)
    {
        Name = name;
        Type = type;
        Index = index;
    }
}

public class Local
{
    public byte Count { get; set; }
    public byte Type { get; set; }

    public Local(byte count, byte type)
    {
        Count = count;
        Type = type;
    }
}


public enum ExportType
{
    Function = 0x00,
    Table = 0x01,
    Memory = 0x02,
    Global = 0x03
}



public enum SectionType
{
    Custom = 0x00,
    Type = 0x01,
    Import = 0x02,
    Function = 0x03,
    Table = 0x04,
    Memory = 0x05,
    Global = 0x06,
    Export = 0x07,
    Start = 0x08,
    Element = 0x09,
    Code = 0x0a,
    Data = 0x0b,
    DataCount = 0x0c,
}

public enum ValueType
{
    I32 = 0x7f,
    I64 = 0x7e,
    F32 = 0x7d,
    F64 = 0x7c,
}



/* 
    var output = new List<byte>
    {
        //wasm magic number
        0x0,0x61,   0x73,0x6D,
        //version
        0x01,0x00,  0x00,0x00,
        
        //new section | type - 5
        //section type ID (1=type) and size 5-bytes | 'type' section contains function signatures
        0x01,0x05,  
        //number of types 
        0x01,
        //func
        0x60,
        //num params
        0x00,
        //num of results
        0x01,
        //result type
        0x7f,

        //new section | function - 2
        //section type ID (3=function) and size 2-bytes | 'function' section contains indexes of the function signature
        0x03, 0x02,
        //number of functions
        0x01,
        //index of function
        0x0,

        //new section | export - 8
        //section type ID (7=export) and size 8-bytes | 'export' section defines export name with the index to our function 
        0x07, 0x08,
        //number of exports
        0x01,
        //export name length
        0x04,
        //export name ("main")
        0x6d, 0x61, 0x69, 0x6e,
        //export type (0=function)
        0x00,
        //export index
        0x00,

        //new section | code - 10
        //section type ID (10=code) and size 7-bytes | 'code' section contains the function body
        0x0a, 0x07,
        //number of functions
        0x01,
        //function body size
        0x05,
        //number of locals
        0x00,

        //function body
        //load constant
        0x41,
        //constant value
        0x2a,
        //return
        0x0f,
        //end of function body
        0x0b,

    };
 */