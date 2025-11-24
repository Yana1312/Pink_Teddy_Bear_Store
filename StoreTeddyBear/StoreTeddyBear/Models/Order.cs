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

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public List<string> AvailableStatuses
    {
        get
        {
            var statuses = new List<string>();

            switch (StatusOrder)
            {
                case "в обработке":
                    statuses.Add("отгружен");
                    break;
                case "отгружен":
                    statuses.Add("доставлен");
                    break;
                case "доставлен":
                    break;
            }

            return statuses;
        }
    }
}
