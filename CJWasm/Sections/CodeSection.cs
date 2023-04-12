using CJWasm.LEB128;
using CJWasm.Models;

namespace CJWasm.Sections;

public class CodeSection : Section
{
    public List<FunctionBody> FunctionBodies { get; private set; }
    public CodeSection(List<FunctionBody> functionBodies)
    {
        FunctionBodies = functionBodies;
    }

    public override byte[] GetBytes()
    {
        var output = new List<byte>();
        output.AddLEB128Unsigned(Convert.ToByte(FunctionBodies.Count));

        foreach (var functionBody in FunctionBodies)
        {
            var localTypes = functionBody.Locals.GroupBy(t => t.Type).ToArray();

            var body = functionBody.Body;
            var bodySize = body.Count + localTypes.Length * 2 + 1 + 1; //add function end and locals and local count here
            output.AddLEB128Unsigned(Convert.ToByte(bodySize));
            //unique local types            
            output.AddLEB128Unsigned(Convert.ToByte(localTypes.Length));
            foreach (var local in localTypes)
            {
                output.AddLEB128Unsigned(Convert.ToByte(local.Count()));
                output.AddLEB128Unsigned(Convert.ToByte(local.Key));
            }
            output.AddRange(body.ToArray());
            //end of function body
            output.Add(0x0b);
        }

        var sectionSize = output.Count;


        output.InsertLEB128Unsigned(0, Convert.ToByte(SectionType.Code));
        output.InsertLEB128Unsigned(1, Convert.ToByte(sectionSize));

        return output.ToArray();
    }
}
