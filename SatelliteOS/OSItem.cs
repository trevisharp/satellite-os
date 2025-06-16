using System.Text.Json.Serialization;

namespace SatelliteOS;

public abstract class OSItem
{
    public string Name { get; set; }
    
    [JsonIgnore]
    public OSFolder Parent { get; set; }

    public abstract string ItemName { get; }

    [JsonIgnore]
    public string FullPath
    {
        get
        {
            string path = "";
            var it = this;
            while (it != null)
            {
                path = it.Name + "/" + path;
                it = it.Parent;
            }
            return path;
        }
    }
}