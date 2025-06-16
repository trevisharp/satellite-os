using System.Collections.Generic;

namespace SatelliteOS;

public class OSFolder : OSItem
{
    public List<OSItem> Content { get; set; } = [];
}