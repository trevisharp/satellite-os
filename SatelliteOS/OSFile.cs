namespace SatelliteOS;

public class OSFile : OSItem
{
    public string Extension { get; set; }
    public string Content { get; set; }

    public override string ItemName => $"{Name}.{Extension}";
}