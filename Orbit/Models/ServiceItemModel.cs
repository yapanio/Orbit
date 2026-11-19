using System;
using Orbit.Core;

namespace Orbit.Models
{
    /// <summary>
    /// Model đại diện cho một Windows Service trên giao diện.
    /// </summary>
    public class ServiceItemModel : ViewModelBase
    {
        private string _serviceName = string.Empty;
        private string _displayName = string.Empty;
        private string _status = "Unknown";
        private bool _canStop;
        private string _startType = "Unknown";
        private bool _isTracked;

        public string ServiceName
        {
            get => _serviceName;
            set => SetProperty(ref _serviceName, value);
        }

        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(IsRunning));
                    OnPropertyChanged(nameof(IsStopped));
                    OnPropertyChanged(nameof(IsTransitioning));
                }
            }
        }

        public bool CanStop
        {
            get => _canStop;
            set => SetProperty(ref _canStop, value);
        }

        public string StartType
        {
            get => _startType;
            set => SetProperty(ref _startType, value);
        }

        public bool IsTracked
        {
            get => _isTracked;
            set => SetProperty(ref _isTracked, value);
        }

        public bool IsRunning => string.Equals(_status, "Running", StringComparison.OrdinalIgnoreCase);
        public bool IsStopped => string.Equals(_status, "Stopped", StringComparison.OrdinalIgnoreCase);
        public bool IsTransitioning => _status.EndsWith("...", StringComparison.Ordinal);
    }
}
