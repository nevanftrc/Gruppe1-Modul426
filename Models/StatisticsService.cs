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
        public MainWindow main { get; set; }
        public CSVlist CSVlist { get; set; }
        public ExportClass Importer { get; set; }
        public NewDictonary Newdictonary { get; private set; }

        private Buckets _bucket;

        public StatisticsService(Buckets bucket)
        {
            _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));

            if (_bucket.buckets == null || _bucket.bucket_count == 0)
            {
                MessageBox.Show("Buckets are not initialized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadStatistics();
            Newdictonary = new NewDictonary();
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

        public WordStatistics GetOrCreateStatistics(string lesson, string word1, string word2, bool isGermanToEnglish)
        {
            string german = isGermanToEnglish ? word1 : word2;
            string english = isGermanToEnglish ? word2 : word1;
            string key = $"{german}:{english}";

            if (!statistics.TryGetValue(key, out var stats))
            {
                stats = new WordStatistics { Lesson = lesson, German = german, English = english };
                statistics[key] = stats;
            }

            else
            {
                stats.Lesson = lesson;
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

        public string GetLessonForWord(string key)
        {
            if (statistics.TryGetValue(key, out var stats))
            {
                return stats.Lesson;  
            }
            return null;  
        }

        public void ResetStatistics()
        {
            statistics.Clear();
            SaveStatistics();
        }
        /// <summary>
        /// Importiert die daten von den window
        /// </summary>
        /// <param name="filePath">den pfad</param>
        public void ImportData(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLower();
                Dictionary<string, WordStatistics> importedData = new Dictionary<string, WordStatistics>();

                if (extension == ".csv" || extension == ".txt")
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string header = reader.ReadLine(); // Skip header line
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] parts = line.Split(',');

                            if (parts.Length >= 7) // Ensure valid format
                            {
                                string lesson = parts[0].Trim();
                                string word1 = parts[1].Trim();
                                string word2 = parts[2].Trim();
                                int correctCount = int.Parse(parts[3]);
                                int incorrectCount = int.Parse(parts[4]);
                                int bucketCount = int.Parse(parts[5]);
                                int currentLocation = int.Parse(parts[6]);

                                // Detect language and assign correctly
                                bool isWord1German = IsGermanWord(word1);
                                string german = isWord1German ? word1 : word2;
                                string english = isWord1German ? word2 : word1;
                                string key = $"{german}:{english}";

                                if (!importedData.ContainsKey(key))
                                {
                                    importedData[key] = new WordStatistics
                                    {
                                        Lesson = lesson,  
                                        German = german,
                                        English = english,
                                        CorrectCount = correctCount,
                                        IncorrectCount = incorrectCount,
                                        BucketCount = bucketCount,
                                        CurrentLocation = currentLocation
                                    };
                                }
                            }
                        }
                    }
                }
                else if (extension == ".json")
                {
                    string jsonContent = File.ReadAllText(filePath);
                    List<WordStatistics> importedList = JsonConvert.DeserializeObject<List<WordStatistics>>(jsonContent)
                                                        ?? new List<WordStatistics>();

                    foreach (var entry in importedList)
                    {
                        bool isGermanFirst = IsGermanWord(entry.German);

                        string german = isGermanFirst ? entry.German : entry.English;
                        string english = isGermanFirst ? entry.English : entry.German;
                        string key = $"{german}:{english}";

                        if (!importedData.ContainsKey(key))
                        {
                            importedData[key] = new WordStatistics
                            {
                                Lesson = entry.Lesson ?? "Unbekannt",
                                German = german,
                                English = english,
                                CorrectCount = entry.CorrectCount,
                                IncorrectCount = entry.IncorrectCount,
                                BucketCount = entry.BucketCount,
                                CurrentLocation = entry.CurrentLocation
                            };
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Ungültiges Dateiformat! Unterstützt: CSV, TXT, JSON.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Merge imported data into the main statistics dictionary
                foreach (var entry in importedData)
                {
                    statistics[entry.Key] = entry.Value;
                }

                // Save updated statistics back to JSON if file doesn't already exist
                string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imported_data.json");
                if (!File.Exists(savePath))
                {
                    File.WriteAllText(savePath, JsonConvert.SerializeObject(statistics, Formatting.Indented));
                }

                MessageBox.Show("Daten erfolgreich importiert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Importieren der Daten:\n{ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Kontrolliert ob es eine deutsches wort ist für switchs
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private bool IsGermanWord(string word)
        {
            // Common German word patterns: Umlauts, "ch", "sch", and certain endings
            string[] germanIndicators = { "ä", "ö", "ü", "ss", "sch", "ch", "ung", "keit", "heit", "zig", "pf" };

            return germanIndicators.Any(indicator => word.Contains(indicator)) || Newdictonary.GetWords().Contains(word.ToLower());
        }
        /// <summary>
        /// Umwandelt die werte
        /// </summary>
        public void ImportDataTypes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Datei nicht gefunden!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ImportData(filePath);
        }
        public void ResetWordBuckets()
        {
            // Ermittle die Anzahl der Buckets, Standard ist 5
            int bucketCount = main?.ReturnValueLBL() ?? 5;
            // Wenn mindestens 3 Buckets vorhanden sind, entspricht Bucket 3 dem Index 2,
            // ansonsten wird als Ziel der mittlere Bucket gewählt.
            int targetBucket = (bucketCount >= 3) ? 2 : bucketCount / 2;

            // Setze für jedes vorhandene Statistik-Objekt die Zähler zurück und
            // weise den Ziel-Bucket zu.
            foreach (var stat in statistics.Values)
            {
                stat.CorrectCount = 0;
                stat.IncorrectCount = 0;
                stat.CurrentLocation = targetBucket;
                stat.BucketCount = bucketCount;
            }
            SaveStatistics();
        }

    }
}
    public class WordStatistics
    {
        public string Lesson { get; set; }
        public string German { get; set; }
        public string English { get; set; }
        public int CorrectCount { get; set; } = 0;
        public int IncorrectCount { get; set; } = 0;
        public int CurrentLocation { get; set; } = -1;
        public int BucketCount { get; set; }
    }

