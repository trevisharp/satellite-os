using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SatelliteOS;

internal class OSFolder : OSItem
{
    [JsonIgnore]
    public IEnumerable<OSItem> Content
    {
        get
        {
            foreach (var file in Files)
                yield return file;
            foreach (var folders in Folders)
                yield return folders;
        }
    }

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