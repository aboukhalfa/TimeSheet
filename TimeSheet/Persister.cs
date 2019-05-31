using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet
{
    public interface IPersister
    {
        void WriteDataToFile(Dictionary<DayOfWeek, TimeSpan> daysOfThisWeekTimes, bool isStarted, DateTime refTime);
        (Dictionary<DayOfWeek, TimeSpan> daysOfThisWeekTimes, bool isStarted, DateTime refTime) ReadDataFromFile();
    }

    public class Persister : IPersister
    {
        public class Data
        {
            public Dictionary<DayOfWeek, TimeSpan> DaysOfThisWeekTimes { get; set; }
            public bool IsStarted { get; set; }            
            public DateTime RefTime { get; set; }
        }

        public void WriteDataToFile(Dictionary<DayOfWeek, TimeSpan> daysOfThisWeekTimes, bool isStarted, DateTime refTime)
        {
            if (!daysOfThisWeekTimes.Values.Any())
                daysOfThisWeekTimes.Add(DateTime.UtcNow.DayOfWeek, TimeSpan.Zero);

            Data data = new Data { DaysOfThisWeekTimes = daysOfThisWeekTimes, IsStarted = isStarted, RefTime = refTime };         

            using (StreamWriter sw = new StreamWriter(@"jsonData.txt"))
            {

                var s = JsonConvert.SerializeObject(data);
                sw.Write(s);
            }
        }

        public (Dictionary<DayOfWeek, TimeSpan> daysOfThisWeekTimes, bool isStarted, DateTime refTime) ReadDataFromFile()
        {
            using (StreamReader sr = new StreamReader(@"jsonData.txt"))
            {
                var s = sr.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Data>(s);
                return (data.DaysOfThisWeekTimes, data.IsStarted, data.RefTime);
            }
        }
    }
}
