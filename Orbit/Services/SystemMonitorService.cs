using System;
using System.Diagnostics;

namespace Orbit.Services
{
    /// <summary>
    /// Service for reading real-time CPU and RAM utilization metrics via Windows Performance Counters.
    /// </summary>
    public class SystemMonitorService : IDisposable
    {
        private readonly PerformanceCounter? _cpuCounter;
        private readonly PerformanceCounter? _ramCounter;
        private readonly double _totalPhysicalMemoryMb;
        private bool _disposed;

        public SystemMonitorService()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                _totalPhysicalMemoryMb = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024.0 * 1024.0);

                // Warm-up initial read (the first read of PerformanceCounter always returns 0)
                _cpuCounter.NextValue();
                _ramCounter.NextValue();
            }
            catch
            {
                // In case of restricted permissions or counter initialization failure
                _cpuCounter = null;
                _ramCounter = null;
            }
        }

        /// <summary>
        /// Reads current overall CPU usage percentage (0 - 100).
        /// </summary>
        public virtual float GetCpuUsage()
        {
            if (_disposed || _cpuCounter == null) return 0f;

            try
            {
                var value = _cpuCounter.NextValue();
                return Math.Clamp((float)Math.Round(value, 1), 0f, 100f);
            }
            catch
            {
                return 0f;
            }
        }

        /// <summary>
        /// Reads current physical RAM usage percentage (0 - 100) matching Task Manager.
        /// </summary>
        public virtual float GetRamUsage()
        {
            if (_disposed || _ramCounter == null || _totalPhysicalMemoryMb <= 0) return 0f;

            try
            {
                var availableMb = _ramCounter.NextValue();
                var usedMb = _totalPhysicalMemoryMb - availableMb;
                var percent = (usedMb / _totalPhysicalMemoryMb) * 100.0;
                return Math.Clamp((float)Math.Round(percent, 1), 0f, 100f);
            }
            catch
            {
                return 0f;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cpuCounter?.Dispose();
                _ramCounter?.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
