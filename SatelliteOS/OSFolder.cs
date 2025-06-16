using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SatelliteOS;

public class OSFolder : OSItem
{
    [JsonIgnore]
    public IEnumerable<OSItem> Content => 
        Folders.AsEnumerable<OSItem>().Concat(Files);

    public List<OSFile> Files { get; set; } = [];
    public List<OSFolder> Folders { get; set; } = [];

    public void Add(OSItem item)
    {
        if (item is OSFile file)
            Files.Add(file);
        if (item is OSFolder folder)
            Folders.Add(folder);
    }
    public void Remove(OSItem item)
    {
        if (item is OSFile file)
            Files.Remove(file);
        if (item is OSFolder folder)
            Folders.Remove(folder);
    }

    [JsonIgnore]
    public override string ItemName => $"{Name}/";
}