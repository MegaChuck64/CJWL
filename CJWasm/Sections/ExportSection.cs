using CJWasm.LEB128;
using CJWasm.Models;
using System.Text;

namespace CJWasm.Sections;

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