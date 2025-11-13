using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoreTeddyBear.Models;

public partial class Orderitem
{
    public int IdOrderItem { get; set; }

    public int IdOrder { get; set; }

    public string ArticulToy { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    [JsonIgnore]
    public virtual Toy ArticulToyNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual Order IdOrderNavigation { get; set; } = null!;
}
