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
        /// <summary>
        /// Importiert die daten von den window
        /// </summary>
        /// <param name="filePath">den pfad</param>
        public void ImportData(string filePath)
        {
            Dictionary<string, WordStatistics> importedData = new Dictionary<string, WordStatistics>();
            try
            {
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".csv" || extension == ".txt")
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string header = reader.ReadLine(); // Skip header line
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] parts = line.Split(',');

                            if (parts.Length >= 6) // Ensure valid format
                            {
                                string word1 = parts[0].Trim();
                                string word2 = parts[1].Trim();
                                int correctCount = int.Parse(parts[2]);
                                int incorrectCount = int.Parse(parts[3]);
                                int bucketCount = int.Parse(parts[4]);
                                int currentLocation = int.Parse(parts[5]);

                                // Determine if word1 is German or English based on known word patterns
                                bool isWord1German = IsGermanWord(word1); // Detects if it's a German word
                                string german = isWord1German ? word1 : word2;
                                string english = isWord1German ? word2 : word1;

                                string key = $"{german}:{english}";

                                if (statistics.ContainsKey(key))
                                {
                                    // Update existing entry
                                    statistics[key].CorrectCount = correctCount;
                                    statistics[key].IncorrectCount = incorrectCount;
                                    statistics[key].CurrentLocation = currentLocation;
                                    statistics[key].BucketCount = bucketCount;
                                }
                                else
                                {
                                    // Add new entry
                                    statistics[key] = new WordStatistics
                                    {
                                        German = german,
                                        English = english,
                                        CorrectCount = correctCount,
                                        IncorrectCount = incorrectCount,
                                        CurrentLocation = currentLocation,
                                        BucketCount = bucketCount
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

                    Dictionary<string, WordStatistics> TempimportedData = new Dictionary<string, WordStatistics>();

                    foreach (var entry in importedList)
                    {
                        // Auto-detect which language comes first based on common patterns
                        bool isGermanFirst = IsGermanWord(entry.German); // Helper function to check if it's a German word

                        string key = isGermanFirst
                            ? $"{entry.German}:{entry.English}"  // German -> English
                            : $"{entry.English}:{entry.German}"; // English -> German

                        // Add entry to the dictionary
                        importedData[key] = entry;
                    }

                    // Merge the imported data into the main statistics dictionary
                    foreach (var entry in TempimportedData)
                    {
                        statistics[entry.Key] = entry.Value;
                    }
                }
                else
                {
                    MessageBox.Show("Ungültiges Dateiformat! Unterstützt: CSV, TXT, JSON.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Save updated statistics back to JSON
                string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imported_data.json");
                File.WriteAllText(savePath, JsonConvert.SerializeObject(statistics, Formatting.Indented));
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

            return germanIndicators.Any(indicator => word.Contains(indicator)) || DictonaryGermanWords.Contains(word.ToLower());
        }

        // A list of common German words for better detection
        private static readonly HashSet<string> DictonaryGermanWords = new HashSet<string>
            {
                "eins", "zwei", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun", "zehn", "elf",
                "hundert", "tausend", "Million", "Milliarde", "erste", "zweite", "dritte",
                "Montag", "Dienstag", "Samstag", "Daumen", "Mund", "Angestellte", "Sontag", "Donnerstag", "Freitag", "Juni",
                "Po", "eine", "wo","wieveil", "wie", "was", "warum", "wann","Bewerbung", "Beruf", "Arbeit", "Ausbildung", "Ohr",
                "Haar", "Gesicht", "Auge", "Nase", "Gewebe", "Kinn", "Wange", "Stirn", "Hals","Nacken","Brust","Bauch","Bein", "Arm",
                "Ellenbogen", "Fingernagel", "Kehle", "Lippe","Trommelfell","Knie","Rippe","Lunge","Leber","Blut","Darm","Niere",
                "Muskel","Skelett","Haut","Zunge","Knochen","Sehne", "Januar", "Februar", "April", "Mai", "Juli", "August", "September", "Oktober",
                "November","Dezember", "Lehrstelle"

            };

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
    }
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