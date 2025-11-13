using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoreTeddyBear.Models;

public partial class Inventory
{
    public int IdInventory { get; set; }

    public string ArticulToy { get; set; } = null!;

    public int QuantityToys { get; set; }

    [JsonIgnore]
    public virtual Toy ArticulToyNavigation { get; set; } = null!;
}
