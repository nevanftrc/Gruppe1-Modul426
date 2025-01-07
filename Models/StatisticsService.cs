using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Newtonsoft.Json;

namespace EasyWordWPF_US5.Models
{
    public static class StatisticsManager
    {
        private const string FilePath = "statistics.json";

        public static Dictionary<string, WordStatistics> LoadStatistics()
        {
            if (!File.Exists(FilePath)) return new Dictionary<string, WordStatistics>();

            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, WordStatistics>>(json) ?? new Dictionary<string, WordStatistics>();
        }

        public static void SaveStatistics(Dictionary<string, WordStatistics> statistics)
        {
            var json = JsonConvert.SerializeObject(statistics, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
    }

}
