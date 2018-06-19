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

        //[JsonIgnore]
        //private Dictionary<string, string> freeDayTypesDict = new Dictionary<string, string>() { {"W5", "dzień wolny"}, {"WN", "niedziela"}, {"WŚ", string.Empty} };

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
                .SelectMany(day => day.e);

            return Convert.ToDecimal(shifts.Sum(e => (TimeSpan.Parse(e.to) - TimeSpan.Parse(e.from)).TotalHours));
        }

        public List<string> SerializeCalendarInformation()
        {
            var serializedInfo = new List<string>();

            foreach (var day in calendar.d)
            {
                if (workingDayTypesCollection.Contains(day.type))
                {
                    serializedInfo.Add(
                        $"{day.date}: Dzień pracujący, godziny: {string.Join(",", day.e.Select(ee => $"{ee.from} - {ee.to}"))}");
                }
                else
                {
                    string vacationDayType;

                    switch (day.type)
                    {
                        case "W5":
                            vacationDayType = "Dzień wolny";
                            break;
                        case "WN":
                            vacationDayType = "Niedziela";
                            break;
                        case "WŚ":
                            vacationDayType = day.e != null && day.e.Any()
                                ? $"{string.Join(",", day.e.Select(ee => $"{ee.name}"))}"
                                : "WŚ";
                            break;
                        default:
                            vacationDayType = day.type;
                            break;
                    }

                    serializedInfo.Add(
                        $"{day.date}: {vacationDayType}");
                }
            }

            return serializedInfo;
        }
    }
}