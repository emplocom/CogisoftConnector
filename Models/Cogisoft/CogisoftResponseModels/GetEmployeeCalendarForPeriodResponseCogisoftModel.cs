using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CogisoftConnector.Models.Cogisoft.CogisoftResponseModels
{
    public class GetEmployeeCalendarForPeriodResponseCogisoftModel
    {
            [JsonProperty("f")]
            public string F { get; set; }

            [JsonProperty("timetable")]
            public Timetable[] timetable { get; set; }

        public class Timetable
        {
            [JsonProperty("cid")]
            public string Cid { get; set; }

            [JsonProperty("qf")]
            public bool Qf { get; set; }

            [JsonProperty("day")]
            public Day[] Day { get; set; }

            [JsonProperty("bc")]
            public object[] Bc { get; set; }
        }

        public class Day
        {
            [JsonProperty("d")]
            public DateTime D { get; set; }

            [JsonProperty("type")]
            public DayType Type { get; set; }

            [JsonProperty("e")]
            public E[] E { get; set; }

            public string GetDescription()
            {
                switch (Type)
                {
                    case DayType.R:
                        return $"{D.ToShortDateString()}: Dzień pracujący, godziny: {string.Join(",", E.Select(ee => $"{ee.From} - {ee.To}"))}";
                    case DayType.W5:
                        return $"{D.ToShortDateString()}: Wolna sobota";
                    case DayType.WN:
                        return $"{D.ToShortDateString()}: Wolna niedziela";
                    case DayType.WŚ:
                        return E != null && E.Any()
                            ? $"{D.ToShortDateString()}: {string.Join(",", E.Select(ee => $"{ee.Name}"))}"
                            : $"{D.ToShortDateString()}: Wolne święto";
                    default:
                        return Type.ToString();
                }
            }
        }

        public class E
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("from")]
            public TimeSpan From { get; set; }

            [JsonProperty("to")]
            public TimeSpan To { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("xsi.type")]
            public string XsiType { get; set; }
        }

        public enum DayType { R, W5, WN, WŚ };

        [JsonIgnore]
        private readonly List<DayType> _workingDayTypesCollection = new List<DayType>() { DayType.R };

        public int GetWorkingDaysCount()
        {
            return timetable[0].Day.Count(day => _workingDayTypesCollection.Contains(day.Type));
        }

        public decimal GetWorkingHoursCount()
        {
            var shifts = timetable[0].Day
                .Where(day => _workingDayTypesCollection.Contains(day.Type))
                .SelectMany(day => day.E);

            return Convert.ToDecimal(shifts.Sum(e => (e.To - e.From).TotalHours));
        }

        public List<string> SerializeCalendarInformation()
        {
            return timetable[0].Day.Select(day => day.GetDescription()).ToList();
        }
    }
}