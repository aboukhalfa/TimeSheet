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
        internal class Data
        {
            internal Dictionary<DayOfWeek, TimeSpan> DaysOfThisWeekTimes { get; set; }
            internal bool IsStarted { get; set; }

            internal DateTime RefTime { get; set; }
        }

        public void WriteDataToFile(Dictionary<DayOfWeek, TimeSpan> daysOfThisWeekTimes, bool isStarted, DateTime refTime)
        {
            Data data = new Data { DaysOfThisWeekTimes = daysOfThisWeekTimes, IsStarted = isStarted, RefTime = refTime };

            //var serializer = new JsonSerializer();
            //using (StreamWriter sw = new StreamWriter(@"jsonData.txt"))
            //using (JsonWriter writer = new JsonTextWriter(sw))
            //{
            //    serializer.Serialize(writer, data);
            //    //serializer.Serialize(writer, refTime);
            //    //serializer.Serialize(writer, daysOfThisWeekTimes);
            //}

            using (StreamWriter sw = new StreamWriter(@"jsonData.txt"))
            {
                var s = JsonConvert.SerializeObject(data, Formatting.Indented);
                sw.Write(s);
            }
        }

        public (Dictionary<DayOfWeek, TimeSpan> daysOfThisWeekTimes, bool isStarted, DateTime refTime) ReadDataFromFile()
        {
            //var serializer = new JsonSerializer();
            //using (StreamReader sr = new StreamReader(@"jsonData.txt"))
            //using (JsonReader reader = new JsonTextReader(sr))
            //{
            //    var data = serializer.Deserialize<Data>(reader);
               
            //    return (data.DaysOfThisWeekTimes, data.IsStarted, data.RefTime);
            //}

            using (StreamReader sr = new StreamReader(@"jsonData.txt"))
            {
                var s = sr.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Data>(s);
                return (data.DaysOfThisWeekTimes, data.IsStarted, data.RefTime);
            }
        }
    }
}
