using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet
{
    public interface IViewModelData
    {
        void Start(TimeSpan? duration = null);
        void Stop();
        TimeSpan TimeForToday { get; }
        TimeSpan TimeForThisWeek { get; }
    }

    public class ViewModelData : IViewModelData
    {
        private DateTime _startTime;
        private TimeSpan _timeForToday;
        private TimeSpan _timeForThisWeek;

        public TimeSpan TimeForToday
        {
            get => _timeForToday;
            private set => _timeForToday = value;
        }

        public TimeSpan TimeForThisWeek { get => _timeForThisWeek; private set => _timeForThisWeek = value; }


        private Dictionary<DayOfWeek, TimeSpan> _daysOfThisWeekTimes = new Dictionary<DayOfWeek, TimeSpan>();
        public TimeSpan this[DayOfWeek index]
        {
            get
            {
                var time = _daysOfThisWeekTimes.TryGetValue(index, out TimeSpan ts) ? ts : TimeSpan.Zero;
                return time;
            }
        }

        public ViewModelData()
        {
        }

        public void Start(TimeSpan? duration = null)
        {
            if (duration != null)
            {
                AddToTodayTime(duration.Value);
                return;
            }

            _startTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            var now = DateTime.UtcNow;
            var delta = now - _startTime;
            AddToTodayTime(delta);
        }

        private void AddToTodayTime(TimeSpan delta)
        {
            TimeForToday = TimeForToday.Add(delta);
            TimeForThisWeek = TimeForThisWeek.Add(delta);
            var dow = DateTime.UtcNow.DayOfWeek;
            var value = delta;
            if (_daysOfThisWeekTimes.TryGetValue(dow, out TimeSpan ts))
            {
                value += ts;
                _daysOfThisWeekTimes[dow] = value;
            }
            else
                _daysOfThisWeekTimes.Add(dow, value);
        }

        public void SetTime(DayOfWeek dow, TimeSpan value, bool lastWeek = false)
        {
                           
        }
    }
}
