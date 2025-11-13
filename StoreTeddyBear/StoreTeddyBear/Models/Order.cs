using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace StoreTeddyBear.Models;

public partial class Order
{
    public int IdOrder { get; set; }

    public int IdCustomer { get; set; }

    public DateTime? DateOrder { get; set; }

    public string? StatusOrder { get; set; }

    public string AdressOrder { get; set; } = null!;

    public decimal? TotalAmount { get; set; }

    [JsonIgnore]
    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();
}
