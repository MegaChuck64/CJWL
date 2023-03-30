
if (args == null || args.Length == 0) throw new Exception("No arguments specified");

var path = args[0];
var info = new FileInfo(path);

if (!info.Exists) throw new Exception("File not found");

if (!info.Extension.Equals(".cj", StringComparison.OrdinalIgnoreCase)) throw new Exception("File extension must be .cj");

var lines = File.ReadAllText(path);


