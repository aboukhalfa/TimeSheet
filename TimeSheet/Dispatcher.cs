using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet
{
    public interface IViewModelData
    {
        void Start(TimeSpan? duration = null);
        void Stop();
        TimeSpan TodayTime { get; }
        TimeSpan ThisWeekTime { get; }
    }

    public class ViewModelData : IViewModelData, INotifyPropertyChanged
    {
        private IPersister _persister;

        public TimeSpan TodayTime
        {
            get
            {
                var dt = DateTime.UtcNow;
                dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                var time = _daysOfThisWeekTimes.TryGetValue(dt.DayOfWeek, out TimeSpan ts) ?
                    ts : TimeSpan.Zero;
                if (IsStarted)
                {
                    time = time.Add(dt - StartTime);
                }

                return time;
            }
        }

        public TimeSpan ThisWeekTime
        {
            get
            {
                var dt = DateTime.UtcNow;
                dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                var time =_daysOfThisWeekTimes.Values
                    .Aggregate(TimeSpan.Zero, (seed, next) => seed.Add(next));
                if (IsStarted)
                {
                    time = time.Add(dt - StartTime);
                }

                return time;
            }
        }

        public TimeSpan TodayDelta
        {
            get => TodayTime - TimeSpan.FromHours(8);
        }

        public TimeSpan ThisWeekDelta =>
            _daysOfThisWeekTimes.Values
                .Aggregate(TimeSpan.Zero, (seed, next) => seed.Add(next - TimeSpan.FromHours(8)));

        public bool IsStarted { get; private set; }
        public DateTime StartTime { get; private set; }

        private Dictionary<DayOfWeek, TimeSpan> _daysOfThisWeekTimes = new Dictionary<DayOfWeek, TimeSpan>();
        public TimeSpan this[DayOfWeek index]
        {
            get
            {
                return _daysOfThisWeekTimes.TryGetValue(index, out TimeSpan ts) ? ts : TimeSpan.Zero;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewModelData(Dictionary<DayOfWeek, TimeSpan> daysOfThisWeekTimes = null, IPersister persister = null)
        {
            if (daysOfThisWeekTimes != null)
            {
                _daysOfThisWeekTimes = daysOfThisWeekTimes;
            }
            else if (persister != null)
            {
                _persister = persister;
                try
                {
                    (daysOfThisWeekTimes, IsStarted, StartTime) = _persister.ReadDataFromFile();
                    _daysOfThisWeekTimes = daysOfThisWeekTimes ?? new Dictionary<DayOfWeek, TimeSpan>();
                }
                catch (Exception)
                {
                }
            }
        }    

        // Notify all
        // Assume 8h/day 40h/week
        // Assume all surplus goes to the last week day
        // Time remaining day +/- (8h based) (toggle => exit time)
        // Accumulated surplus
        // Total week time

        public void Start(TimeSpan? duration = null)
        {
            if (duration != null)
            {
                AddToTodayTime(duration.Value);
            }
            else
            {
                var dt = DateTime.UtcNow;
                dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

                if (!IsStarted)
                {
                    StartTime = dt;
                    IsStarted = true;
                }
            }

            if (_persister != null)
            {
                _persister.WriteDataToFile(_daysOfThisWeekTimes, IsStarted, StartTime);
            }
        }

        public void Stop()
        {
            var now = DateTime.UtcNow;
            now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            var delta = now - StartTime;
            AddToTodayTime(delta);
            IsStarted = false;
            if (_persister != null)
            {
                _persister.WriteDataToFile(_daysOfThisWeekTimes, IsStarted, StartTime);
            }
        }

        private void AddToTodayTime(TimeSpan delta)
        {
            var dow = DateTime.UtcNow.DayOfWeek;
            var value = delta;
            if (_daysOfThisWeekTimes.TryGetValue(dow, out TimeSpan ts))
            {
                value += ts;
                _daysOfThisWeekTimes[dow] = value;
            }
            else
                _daysOfThisWeekTimes.Add(dow, value);

            NotifyPropertyChanged("TimeForToday");
            NotifyPropertyChanged("TimeForThisWeek");
            NotifyPropertyChanged("DaySurplus");
            NotifyPropertyChanged("WeekSurplus");
        }

        [Conditional("DEBUG")]
        public void SetTime(DayOfWeek dow, TimeSpan value, bool lastWeek = false)
        {
            _daysOfThisWeekTimes[dow] = value;
        }

        public void Tick()
        {
            NotifyPropertyChanged("TodayTime");
            NotifyPropertyChanged("ThisWeekTime");
            NotifyPropertyChanged("TodayDelta");
            NotifyPropertyChanged("ThisWeekDelta");
        }
    }
}
