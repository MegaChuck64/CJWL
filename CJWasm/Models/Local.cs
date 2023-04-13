
namespace CJWasm.Models;

public class Local
{
    public byte Type { get; set; }

    public Local(byte type)
    {
        Type = type;
    }
}
