using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CogisoftConnector.Logic;
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
            public string Type { get; set; }

            [JsonProperty("e")]
            public E[] E { get; set; }

            public string GetDescription()
            {
                switch (Type)
                {
                    case "R":
                        return $"{D.ToShortDateString()}: Dzień pracujący, godziny: {string.Join(",", E.DistinctBy(ee => ee.Id).Select(ee => $"{ee.From} - {ee.To}"))}";
                    case "X":
                        return $"{D.ToShortDateString()}: Brak harmonogramu na ten dzień";
                    case "WR":
                        return $"{D.ToShortDateString()}: Dzień wolny";
                    case "W5":
                        return $"{D.ToShortDateString()}: Wolna sobota";
                    case "WN":
                        return $"{D.ToShortDateString()}: Wolna niedziela";
                    case "WŚ":
                        return E != null && E.Any()
                            ? $"{D.ToShortDateString()}: {string.Join(",", E.DistinctBy(ee => ee.Id).Select(ee => $"{ee.Name}"))}"
                            : $"{D.ToShortDateString()}: Wolne święto";
                    default:
                        return Type;
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

        [JsonIgnore]
        private readonly List<string> _workingDayTypesCollection = new List<string>() { "R" };

        public int GetWorkingDaysCount()
        {
            return timetable[0].Day.Count(day => _workingDayTypesCollection.Contains(day.Type));
        }

        public decimal GetWorkingHoursCount()
        {
            var shifts = timetable[0].Day
                .Where(day => _workingDayTypesCollection.Contains(day.Type))
                .SelectMany(day => day.E.DistinctBy(ee => ee.Id));

            return Convert.ToDecimal(shifts.Sum(e => (e.To - e.From).TotalHours));
        }

        public List<string> SerializeCalendarInformation()
        {
            return timetable[0].Day.Select(day => day.GetDescription()).ToList();
        }

        public bool CalendarIsReady()
        {
            return
                this.timetable != null && this.timetable[0] != null &&
                ("-1".Equals(this.timetable[0].Cid) || this.timetable[0].Cid == null);
        }
    }
}