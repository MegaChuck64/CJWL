

public class Module
{
    public string Name { get; set; }
    public int Version { get; set; }

    public CJClass Entry { get; set; }
    public List<CJClass> Classes { get; set; }
}

public class CJClass
{
    public string Name { get; set; }    
}

