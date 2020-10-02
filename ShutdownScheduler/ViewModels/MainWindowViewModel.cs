using ShutdownScheduler.Commands;
using ShutdownScheduler.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShutdownScheduler.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region variables
        private TimeSpan? _timeLeft;
        private Timer _timer;
        private System.Timers.Timer _countdownTimer;
        private DateTime? _timeToExecute;

        private Dictionary<AvailableScheduleModes, string> _modeCommand = new Dictionary<AvailableScheduleModes, string>
        {
            { AvailableScheduleModes.Shutdown, "-s  -f" },
            { AvailableScheduleModes.Restart, "-r -f" },
            { AvailableScheduleModes.Hibernate, "-h" },
            { AvailableScheduleModes.LogOff, "-l" }
        };

        private string _tooltip;

        public string Tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; NotifyOnPropertyChanged(nameof(Tooltip)); }
        }

        private string _countdown;

        public string Countdown
        {
            get { return _countdown; }
            set { _countdown = value; NotifyOnPropertyChanged(nameof(Countdown)); }
        }

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
        public ICommand ScheduleCommand => new RelayCommand(ScheduleExecute, (o) => 
        { 
            if (ScheduleTime.HasValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        });
        public ICommand CancelCommand => new RelayCommand(CancelExecute);

        private async void ScheduleExecute(object parameter)
        {
            var time = CalculateTime();

            await ScheduleTask(time);
        }

        private async void CancelExecute(object parameter)
        {
            await ClearTimer();
        }
        #endregion

        private Task ScheduleTask(double time)
        {
            _timer = new Timer(RunCommand, null, TimeSpan.FromSeconds(time), TimeSpan.Zero);
            _timeToExecute = _scheduleTime;

            _countdownTimer.Interval = 1000;
            _countdownTimer.Start();

            return Task.CompletedTask;
        }

        private void RunCommand(object state)
        {
            var command = _modeCommand[_selectedMode];

            ClearTimer();

            Process.Start("shutdown", $"{command}");
        }

        private Task ClearTimer()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _countdownTimer.Stop();
            Tooltip = null;
            Countdown = null;
            _timeToExecute = null;

            return Task.CompletedTask;
        }

        private void CountdownTick(object sender, EventArgs e)
        {
            _timeLeft = _timeToExecute - DateTimeOffset.Now;

            if (_timeLeft.Value.TotalSeconds < 0)
            {
                var now = DateTime.Now;
                _timeLeft = now.AddDays(1).AddSeconds(_timeLeft.Value.TotalSeconds) - now;
            }

            Tooltip = $"Scheduled {_selectedMode} at {_timeToExecute.Value.Hour:00}:{_timeToExecute.Value.Minute:00}";
            Countdown = $"{_timeLeft.Value.Hours:00}:{_timeLeft.Value.Minutes:00}:{_timeLeft.Value.Seconds:00}";

            CommandManager.InvalidateRequerySuggested();
        }

        private double CalculateTime()
        {
            if (_isNow)
            {
                return 0;
            }
            else
            {
                var now = DateTime.Now;
                var timeToShutdown = (_scheduleTime - now).Value.TotalSeconds;

                if (timeToShutdown < 0)
                {
                    timeToShutdown = (now.AddDays(1).AddSeconds(timeToShutdown) - now)
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
            _countdownTimer = new System.Timers.Timer();
            _countdownTimer.Elapsed += new System.Timers.ElapsedEventHandler(CountdownTick);
        }
    }
}
