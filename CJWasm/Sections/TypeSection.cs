using CJWasm.LEB128;
using CJWasm.Models;

namespace CJWasm.Sections;


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
