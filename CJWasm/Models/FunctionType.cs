
namespace CJWasm.Models;

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
