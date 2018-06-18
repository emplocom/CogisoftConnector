using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CogisoftConnector.Models.Cogisoft.CogisoftResponseModels
{
    public class GetEmployeeCalendarForPeriodResponseCogisoftModel
    {
        public class E
        {
            public string type { get; set; }
            public string id { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public string name { get; set; }
        }

        public class D
        {
            public string date { get; set; }
            public string type { get; set; }
            public List<E> e { get; set; }
        }

        public class M
        {
            public string nr { get; set; }
            public string balance { get; set; }
        }

        public class Calendar
        {
            public List<D> d { get; set; }
            public List<M> m { get; set; }
        }

        public Calendar calendar { get; set; }

        [JsonIgnore]
        private List<string> freeDayTypesCollection = new List<string>() {"W5","WN", "WŚ"};

        [JsonIgnore]
        private List<string> workingDayTypesCollection = new List<string>() { "W" };

        public int GetWorkingDaysCount()
        {
            return calendar.d.Count(day => workingDayTypesCollection.Contains(day.type));
        }

        public decimal GetWorkingHoursCount()
        {
            var shifts = calendar.d
                .Where(day => workingDayTypesCollection.Contains(day.type))
                .SelectMany(day => day.e.Where(ee => ee.type.Equals("shift")));

            return Convert.ToDecimal(shifts.Sum(e => (DateTime.Parse(e.to) - DateTime.Parse(e.from)).TotalHours));
        }

        public List<string> SerializeCalendarInformation()
        {
            var serializedInfo = new List<string>();

            foreach (var day in calendar.d)
            {
                if (workingDayTypesCollection.Contains(day.type))
                {
                    serializedInfo.Add($"{day.date}: Dzień pracujący, godziny: {string.Join(",", day.e.Where(ee => ee.type.Equals("shift")).Select(ee => $"{ee.from} - {ee.to}"))}");
                }
                else
                {
                    serializedInfo.Add($"{day.date}: Dzień wolny, typ: {day.type}, dod. inf.: {string.Join(",", day.e.Select(ee => $"{ee.type}, {ee.name}"))}");
                }
            }

            return serializedInfo;
        }
    }
}