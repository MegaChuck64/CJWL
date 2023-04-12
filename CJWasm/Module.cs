
using CJWasm.Sections;

namespace CJWasm;

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