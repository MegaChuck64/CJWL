using CJWasm;
using CJWasm.LEB128;
using CJWasm.Models;
using CJWasm.Sections;

#if DEBUG
{
    args = new string[]
    {
        "-s", "test.cjwl",
        "-o", "output.wasm",
    };
}
#endif

var source = string.Empty;
var output = string.Empty;



for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-s":
            i++;
            source = args[i];
            break;
        case "-o":
            i++;
            output = args[i];
            break;
        default:
            Console.WriteLine("Unknown argument: " + args[i]);
            break;
    }
}
if (string.IsNullOrEmpty(source))
{
    Console.WriteLine("No source file specified.");
    return;
}
if (string.IsNullOrEmpty(output))
{
    Console.WriteLine("No output file specified.");
    return;
}

var lines = File.ReadAllLines(source).ToList();
using var fs = new FileStream(output, FileMode.Create);

var bytes = Compile(lines);
fs.Write(bytes);


byte[] Compile(List<string> lines)
{
    var module = Parser.Parse(lines);

    return module.ToBytes();
}

byte[] Run()
{

    var parms = new List<byte>();
    parms.AddLEB128Unsigned(Convert.ToByte(WasmValueType.I32));
    parms.AddLEB128Unsigned(Convert.ToByte(WasmValueType.I32));

    var returns = new List<byte>();
    returns.AddLEB128Unsigned(Convert.ToByte(WasmValueType.I32));

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
                new Local((byte)WasmValueType.I32),
                new Local((byte)WasmValueType.I64),
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




/* 
0000000: 0061 736d                                 ; WASM_BINARY_MAGIC
0000004: 0100 0000                                 ; WASM_BINARY_VERSION
; section "Type" (1)
0000008: 01                                        ; section code
0000009: 00                                        ; section size (guess)
000000a: 01                                        ; num types
; func type 0
000000b: 60                                        ; func
000000c: 02                                        ; num params
000000d: 7f                                        ; i32
000000e: 7f                                        ; i32
000000f: 01                                        ; num results
0000010: 7f                                        ; i32
0000009: 07                                        ; FIXUP section size
; section "Function" (3)
0000011: 03                                        ; section code
0000012: 00                                        ; section size (guess)
0000013: 01                                        ; num functions
0000014: 00                                        ; function 0 signature index
0000012: 02                                        ; FIXUP section size
; section "Export" (7)
0000015: 07                                        ; section code
0000016: 00                                        ; section size (guess)
0000017: 01                                        ; num exports
0000018: 0d                                        ; string length
0000019: 6164 6454 776f 506c 7573 5465 6e         addTwoPlusTen  ; export name
0000026: 00                                        ; export kind
0000027: 00                                        ; export func index
0000016: 11                                        ; FIXUP section size
; section "Code" (10)
0000028: 0a                                        ; section code
0000029: 00                                        ; section size (guess)
000002a: 01                                        ; num functions
; function body 0
000002b: 00                                        ; func body size (guess)
000002c: 02                                        ; local decl count
000002d: 01                                        ; local type count
000002e: 7f                                        ; i32
000002f: 01                                        ; local type count
0000030: 7e                                        ; i64
0000031: 41                                        ; i32.const
0000032: 0a                                        ; i32 literal
0000033: 21                                        ; local.set
0000034: 02                                        ; local index
0000035: 42                                        ; i64.const
0000036: cc21                                      ; i64 literal
0000038: 21                                        ; local.set
0000039: 03                                        ; local index
000003a: 20                                        ; local.get
000003b: 00                                        ; local index
000003c: 20                                        ; local.get
000003d: 01                                        ; local index
000003e: 6a                                        ; i32.add
000003f: 20                                        ; local.get
0000040: 02                                        ; local index
0000041: 6a                                        ; i32.add
0000042: 0b                                        ; end
000002b: 17                                        ; FIXUP func body size
0000029: 19                                        ; FIXUP section size
; section "name"
0000043: 00                                        ; section code
0000044: 00                                        ; section size (guess)
0000045: 04                                        ; string length
0000046: 6e61 6d65                                name  ; custom section name
000004a: 02                                        ; local name type
000004b: 00                                        ; subsection size (guess)
000004c: 01                                        ; num functions
000004d: 00                                        ; function index
000004e: 00                                        ; num locals
000004b: 03                                        ; FIXUP subsection size
0000044: 0a                                        ; FIXUP section size
 */