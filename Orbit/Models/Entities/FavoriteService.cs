using System;
using System.Collections.Generic;

namespace Orbit.Models.Entities;

public partial class FavoriteService
{
    public int FavoriteId { get; set; }

    public int UserId { get; set; }

    public string ServiceName { get; set; } = null!;

    public DateTime AddedDate { get; set; }

    public virtual User User { get; set; } = null!;
}
