using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoreTeddyBear.Models;

public partial class Useransadmin
{
    public static Useransadmin CreateUser(string email, string name, string password)
    {
        return new Useransadmin
        {
            EmailUsers = email,
            NameUsers = name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            StatusUsersProfile = "активный",
            RoleUsers = "пользователь",
            Reviews = new List<Review>()
        };
    }

    public int IdCustomer { get; set; }

    public string EmailUsers { get; set; } = null!;

    public string NameUsers { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string StatusUsersProfile { get; set; } = null!;

    public string RoleUsers { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
