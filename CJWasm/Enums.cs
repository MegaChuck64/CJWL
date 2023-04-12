
namespace CJWasm;


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

public enum WasmValueType
{
    I32 = 0x7f,
    I64 = 0x7e,
    F32 = 0x7d,
    F64 = 0x7c,
}

