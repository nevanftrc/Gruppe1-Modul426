using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EasyWordWPF_US5.Models
{
    public class StatisticsService
    {
        private const string FilePath = "statistics.json";
        private Dictionary<string, WordStatistics> statistics;

        public StatisticsService()
        {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                statistics = JsonConvert.DeserializeObject<Dictionary<string, WordStatistics>>(json) ?? new Dictionary<string, WordStatistics>();
            }
            else
            {
                statistics = new Dictionary<string, WordStatistics>();
            }
        }

        public void SaveStatistics()
        {
            var json = JsonConvert.SerializeObject(statistics, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public WordStatistics GetOrCreateStatistics(string word1, string word2, bool isGermanToEnglish)
        {
            string german = isGermanToEnglish ? word1 : word2;
            string english = isGermanToEnglish ? word2 : word1;
            string key = $"{german}:{english}";
            if (!statistics.TryGetValue(key, out var stats))
            {
                stats = new WordStatistics { German = german, English = english };
                statistics[key] = stats;
            }
            return stats;
        }

        public void ResetStatistics()
        {
            statistics.Clear();
            SaveStatistics();
        }
    }

    public class WordStatistics
    {
        public string German { get; set; }
        public string English { get; set; }
        public int CorrectCount { get; set; } = 0;
        public int IncorrectCount { get; set; } = 0;
    }
}
