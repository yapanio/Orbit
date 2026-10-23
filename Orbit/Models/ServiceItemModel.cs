using Orbit.Core;

namespace Orbit.Models
{
    /// <summary>
    /// Represents a Windows Service item for display and interaction in the UI.
    /// Inherits from ViewModelBase to support live property updates.
    /// </summary>
    public class ServiceItemModel : ViewModelBase
    {
        private string _serviceName = string.Empty;
        private string _displayName = string.Empty;
        private string _status = "Unknown";
        private bool _canStop;
        private bool _canPauseAndContinue;
        private string _startType = "Unknown";
        private bool _isTracked;
        private string _description = string.Empty;

        /// <summary>
        /// The unique system identifier for the Windows Service (e.g., "mssqlserver").
        /// </summary>
        public string ServiceName
        {
            get => _serviceName;
            set => SetProperty(ref _serviceName, value);
        }

        /// <summary>
        /// The friendly display name of the service.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Current status ("Running", "Stopped", "Starting...", "Stopping...", "Paused", etc.).
        /// </summary>
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

        /// <summary>
        /// Indicates if the service can be stopped once running.
        /// </summary>
        public bool CanStop
        {
            get => _canStop;
            set => SetProperty(ref _canStop, value);
        }

        /// <summary>
        /// Indicates if the service can be paused and continued.
        /// </summary>
        public bool CanPauseAndContinue
        {
            get => _canPauseAndContinue;
            set => SetProperty(ref _canPauseAndContinue, value);
        }

        /// <summary>
        /// The startup type (e.g., Automatic, Manual, Disabled).
        /// </summary>
        public string StartType
        {
            get => _startType;
            set => SetProperty(ref _startType, value);
        }

        /// <summary>
        /// Indicates if this service is pinned/tracked by the developer.
        /// </summary>
        public bool IsTracked
        {
            get => _isTracked;
            set => SetProperty(ref _isTracked, value);
        }

        /// <summary>
        /// Description of the service.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsRunning => string.Equals(_status, "Running", StringComparison.OrdinalIgnoreCase);
        public bool IsStopped => string.Equals(_status, "Stopped", StringComparison.OrdinalIgnoreCase);
        public bool IsTransitioning => _status.EndsWith("...", StringComparison.Ordinal);
    }
}
