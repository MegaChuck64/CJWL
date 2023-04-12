
namespace CJWasm.Models;

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
