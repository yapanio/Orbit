using System;
using System.Collections.Generic;

namespace Orbit.Models
{
    /// <summary>
    /// Model representing an individual tracked service entry.
    /// </summary>
    public class TrackedServiceItem
    {
        public string ServiceName { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Configuration model persisted to disk storing all developer-tracked services.
    /// </summary>
    public class TrackedConfigModel
    {
        public List<TrackedServiceItem> TrackedServices { get; set; } = new();
    }
}
