using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using EasyWordWPF;
using Newtonsoft.Json;

namespace EasyWordWPF_US5.Models
{
    public class StatisticsService
    {
        private const string FilePath = "statistics.json";
        private Dictionary<string, WordStatistics> statistics;
        public MainWindow main {  get; set; }
        public CSVlist CSVlist { get; set; }
        private Buckets _bucket;

        public StatisticsService(Buckets bucket)
        {
            _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));

            if (_bucket.buckets == null || _bucket.bucket_count == 0)
            {
                MessageBox.Show("Buckets are not initialized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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

            stats.BucketCount = main?.ReturnValueLBL() ?? 5;
            // Get the current bucket index
            // Find the bucket index based on movement logic
            int movement = stats.IncorrectCount - stats.CorrectCount;
            int middleBucket = stats.BucketCount / 2;

            // Ensure the calculated index is within range
            stats.CurrentLocation = Math.Max(0, Math.Min(stats.BucketCount - 1, middleBucket - movement));

            return stats;
        }

        public void ResetStatistics()
        {
            statistics.Clear();
            SaveStatistics();
        }

        public void ImportData (){ }
    }

    public class WordStatistics
    {
        public string German { get; set; }
        public string English { get; set; }
        public int CorrectCount { get; set; } = 0;
        public int IncorrectCount { get; set; } = 0;
        public int CurrentLocation { get; set; } = -1;
        public int BucketCount { get; set; }
    }
}
