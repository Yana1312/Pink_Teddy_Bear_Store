using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoreTeddyBear.Models;

public partial class Review
{
    public int IdReview { get; set; }

    public string ArticulToy { get; set; } = null!;

    public int IdCustomer { get; set; }

    public sbyte RatingReview { get; set; }

    public string? CommentReview { get; set; }

    public DateTime? DateReview { get; set; }

    [JsonIgnore]
    public virtual Toy ArticulToyNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual Useransadmin IdCustomerNavigation { get; set; } = null!;
}
