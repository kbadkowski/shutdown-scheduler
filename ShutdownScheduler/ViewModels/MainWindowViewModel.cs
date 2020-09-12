using ShutdownScheduler.Commands;
using ShutdownScheduler.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace ShutdownScheduler.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region variables
        private Dictionary<AvailableScheduleModes, string> _modeCommand = new Dictionary<AvailableScheduleModes, string>
        {
            { AvailableScheduleModes.Shutdown, "-s" },
            { AvailableScheduleModes.Restart, "-r" },
            { AvailableScheduleModes.Hibernate, "-h" },
            { AvailableScheduleModes.LogOff, "-l" }
        };

        private bool _isNow = false;

        public bool IsNow
        {
            get { return _isNow; }
            set { _isNow = value; NotifyOnPropertyChanged(nameof(IsNow)); }
        }


        private DateTime? _scheduleTime;

        public DateTime? ScheduleTime
        {
            get { return _scheduleTime; }
            set { _scheduleTime = value; NotifyOnPropertyChanged(nameof(ScheduleTime)); }
        }

        private AvailableScheduleModes _selectedMode;

        public AvailableScheduleModes SelecetedMode
        {
            get { return _selectedMode; }
            set { _selectedMode = value; NotifyOnPropertyChanged(nameof(SelecetedMode)); }
        }

        private IReadOnlyCollection<AvailableScheduleModes> _availableModes;

        public IReadOnlyCollection<AvailableScheduleModes> AvailableModes
        {
            get { return _availableModes; }
            set { _availableModes = value; NotifyOnPropertyChanged(nameof(AvailableModes)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyOnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region commands
        public ICommand ScheduleCommand => new RelayCommand(ScheduleExecute);
        public ICommand CancelCommand => new RelayCommand(CancelExecute);

        private void ScheduleExecute(object parameter)
        {
            var time = CalculateTime();
            var command = _modeCommand[_selectedMode];

            Process.Start("shutdown", $"{command} -t {time} -f");
        }

        private void CancelExecute(object parameter)
        {
            Process.Start("shutdown", "-a");
        }
        #endregion

        private int CalculateTime()
        {
            if (_isNow)
            {
                return 0;
            }
            else
            {
                var now = DateTime.Now;
                var timeToShutdown = (int)(_scheduleTime - now).Value.TotalSeconds;

                if (timeToShutdown < 0)
                {
                    timeToShutdown = (int)(now.AddDays(1).AddSeconds(timeToShutdown) - now)
                        .TotalSeconds;
                }
                return timeToShutdown;
            }
        }

        public MainWindowViewModel()
        {
            _scheduleTime = DateTime.Now.AddMinutes(5);
            _availableModes = Enum
                .GetValues(typeof(AvailableScheduleModes))
                .Cast<AvailableScheduleModes>()
                .ToList();
            _selectedMode = AvailableScheduleModes.Shutdown;
        }
    }
}
