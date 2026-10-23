using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using Orbit.Models;

namespace Orbit.Services
{
    /// <summary>
    /// Service responsible for querying and controlling Windows Services via ServiceController.
    /// Strictly manages OS handles by ensuring proper disposal of ServiceController instances.
    /// </summary>
    public class WindowsServiceManager
    {
        /// <summary>
        /// Retrieves all Windows Services installed on the local machine.
        /// Disposes each ServiceController instance after reading its properties.
        /// </summary>
        public List<ServiceItemModel> GetAllServices()
        {
            var result = new List<ServiceItemModel>();
            ServiceController[] services;

            try
            {
                services = ServiceController.GetServices();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to query Windows Services: {ex.Message}", ex);
            }

            foreach (var sc in services)
            {
                try
                {
                    string status = sc.Status.ToString();
                    bool canStop = false;
                    bool canPause = false;
                    string startType = "Unknown";

                    try
                    {
                        canStop = sc.CanStop;
                    }
                    catch
                    {
                        // Some system services restrict querying capabilities
                    }

                    try
                    {
                        canPause = sc.CanPauseAndContinue;
                    }
                    catch
                    {
                    }

                    try
                    {
                        startType = sc.StartType.ToString();
                    }
                    catch
                    {
                    }

                    result.Add(new ServiceItemModel
                    {
                        ServiceName = sc.ServiceName,
                        DisplayName = string.IsNullOrWhiteSpace(sc.DisplayName) ? sc.ServiceName : sc.DisplayName,
                        Status = status,
                        CanStop = canStop,
                        CanPauseAndContinue = canPause,
                        StartType = startType
                    });
                }
                finally
                {
                    sc.Dispose();
                }
            }

            return result.OrderBy(s => s.DisplayName).ToList();
        }

        /// <summary>
        /// Gets the current status string of a given service.
        /// </summary>
        public string GetServiceStatus(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                return sc.Status.ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Refreshes the properties of an existing ServiceItemModel.
        /// </summary>
        public void RefreshService(ServiceItemModel model)
        {
            if (model == null) return;
            try
            {
                using var sc = new ServiceController(model.ServiceName);
                model.Status = sc.Status.ToString();
                try { model.CanStop = sc.CanStop; } catch { }
                try { model.CanPauseAndContinue = sc.CanPauseAndContinue; } catch { }
                try { model.StartType = sc.StartType.ToString(); } catch { }
            }
            catch
            {
                // In case service was uninstalled or inaccessible
            }
        }

        /// <summary>
        /// Starts a Windows service and waits until it reaches Running status or times out.
        /// </summary>
        public void StartService(string serviceName, TimeSpan timeout)
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                return;
            }

            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }

        /// <summary>
        /// Stops a Windows service and waits until it reaches Stopped status or times out.
        /// </summary>
        public void StopService(string serviceName, TimeSpan timeout)
        {
            using var sc = new ServiceController(serviceName);

            if (!sc.CanStop)
            {
                throw new InvalidOperationException($"Service '{serviceName}' cannot be stopped by system policy.");
            }

            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                return;
            }

            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }

        /// <summary>
        /// Restarts a Windows service by stopping it and then starting it again.
        /// </summary>
        public void RestartService(string serviceName, TimeSpan timeout)
        {
            using var sc = new ServiceController(serviceName);

            var halfTimeout = TimeSpan.FromTicks(timeout.Ticks / 2);

            if (sc.Status != ServiceControllerStatus.Stopped)
            {
                if (!sc.CanStop)
                {
                    throw new InvalidOperationException($"Service '{serviceName}' cannot be stopped.");
                }

                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, halfTimeout);
            }

            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, halfTimeout);
        }
    }
}
