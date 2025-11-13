using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoreTeddyBear.Models;

public partial class Toy
{
    public string ArticulToy { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Descriptionn { get; set; }

    public decimal Price { get; set; }

    public string? Height { get; set; }

    public string? Weight { get; set; }

    public int QuantityInStock { get; set; }

    [JsonIgnore]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [JsonIgnore]
    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
