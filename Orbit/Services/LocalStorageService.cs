using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Orbit.Models;

namespace Orbit.Services
{
    /// <summary>
    /// Service responsible for persisting tracked services to local JSON storage.
    /// </summary>
    public class LocalStorageService
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public LocalStorageService(string? customPath = null)
        {
            _filePath = customPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tracked_services.json");
        }

        /// <summary>
        /// Loads the set of tracked service names from the local JSON file.
        /// </summary>
        public HashSet<string> LoadTrackedServices()
        {
            var config = LoadConfig();
            return new HashSet<string>(config.TrackedServices.Select(t => t.ServiceName), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Loads the full configuration model including dates.
        /// </summary>
        public TrackedConfigModel LoadConfig()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new TrackedConfigModel();
                }

                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new TrackedConfigModel();
                }

                var config = JsonSerializer.Deserialize<TrackedConfigModel>(json, JsonOptions);
                return config ?? new TrackedConfigModel();
            }
            catch
            {
                return new TrackedConfigModel();
            }
        }

        /// <summary>
        /// Saves the list of tracked service names to the local JSON file.
        /// </summary>
        public void SaveTrackedServices(IEnumerable<string> serviceNames)
        {
            try
            {
                var existing = LoadConfig();
                var existingDict = existing.TrackedServices.ToDictionary(t => t.ServiceName, StringComparer.OrdinalIgnoreCase);

                var updatedList = new List<TrackedServiceItem>();
                foreach (var name in serviceNames.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (existingDict.TryGetValue(name, out var item))
                    {
                        updatedList.Add(item);
                    }
                    else
                    {
                        updatedList.Add(new TrackedServiceItem
                        {
                            ServiceName = name,
                            AddedDate = DateTime.UtcNow
                        });
                    }
                }

                var config = new TrackedConfigModel { TrackedServices = updatedList };
                var json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save tracked services: {ex.Message}");
            }
        }
    }
}
