namespace SatelliteOS;

public abstract class OSItem
{
    public string Name { get; set; }
    public OSFolder Parent { get; set; }

    public abstract string ItemName { get; }

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