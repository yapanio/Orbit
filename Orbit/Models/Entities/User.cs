using System;
using System.Collections.Generic;

namespace Orbit.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<FavoriteService> FavoriteServices { get; set; } = new List<FavoriteService>();
}
