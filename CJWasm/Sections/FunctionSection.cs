using CJWasm.LEB128;

namespace CJWasm.Sections;

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