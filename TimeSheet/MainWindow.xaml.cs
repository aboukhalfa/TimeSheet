using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TimeSheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        DispatcherTimer _timer = new DispatcherTimer();

        DateTime _baseTime = DateTime.MinValue;

        AllTimes _times;

        private bool _isStarted;
        public bool IsStarted {
            get => _isStarted;
            set
            {
                if (value != _isStarted)
                {
                    ProcessIsStartedChange(value);

                    _isStarted = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void ProcessIsStartedChange(bool value)
        {
            var dt = DateTime.UtcNow;
            dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            if (value)
            {
                _baseTime = dt;
            }
            else
            {
                Time += dt - _baseTime;
            }
        }

        private TimeSpan _time = TimeSpan.Zero;
        public TimeSpan Time
        {
            get => _time;
            set
            {
                if (value != _time)
                {
                    _time = value;
                    UpdatePers();
                    NotifyPropertyChanged();
                }
            }
        }

        private void UpdatePers()
        {
            var dt = DateTime.UtcNow;
            var dow = dt.DayOfWeek;
            _times.Days[dow] = Time;

            //*** to do
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            //Fail quick
            try
            {
                _times = AllTimes.ReadDataFromFile();

                ReadDataFromPers();
            }
            catch (Exception)
            {
                _times = new AllTimes();
            }
        }

        private void ReadDataFromPers()
        {
            _baseTime = _times.BaseTime;
            Time = _times.Time;
            IsStarted = _times.IsStarted;
        }

        private void WriteDataToPers()
        {
            _times.IsStarted = IsStarted;
            _times.BaseTime = _baseTime;
            _times.Time = Time;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _timer.Stop();
            base.OnClosing(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;            
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            var dt = DateTime.UtcNow;
            dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            Time += dt - _baseTime;
            _baseTime = dt;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            IsStarted = !IsStarted;

            if (IsStarted)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }

            WriteDataToPers();

            AllTimes.WriteDataToFile(_times);
        }
    }
    
    public class AllTimes
    {
        public static void WriteDataToFile(AllTimes times)
        {
            var serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(@"json.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, times);
            }
        }

        public static AllTimes ReadDataFromFile()
        {
            var serializer = new JsonSerializer();
            using (StreamReader sr = new StreamReader(@"json.txt"))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var times = serializer.Deserialize<AllTimes>(reader);
                return times;
            }           
        }
        public bool IsStarted { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime BaseTime { get; set; }
        public Dictionary<DayOfWeek, TimeSpan> Days { get; } = new Dictionary<DayOfWeek, TimeSpan>();
        public Dictionary<int, TimeSpan> Weeks { get; } = new Dictionary<int, TimeSpan>();
    }   

    public class MyBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool))
                return Visibility.Collapsed;

            var bvalue = (bool)value;
            bvalue = parameter == null ? bvalue : !bvalue;

            return bvalue ? Visibility.Visible : Visibility.Hidden;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
