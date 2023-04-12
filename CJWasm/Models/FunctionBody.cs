
namespace CJWasm.Models;

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