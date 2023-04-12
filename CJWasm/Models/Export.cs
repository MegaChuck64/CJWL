
namespace CJWasm.Models;

public class Export
{
    public string Name { get; private set; }
    public ExportType Type { get; private set; }
    public int Index { get; private set; }
    public Export(string name, ExportType type, int index)
    {
        Name = name;
        Type = type;
        Index = index;
    }
}
