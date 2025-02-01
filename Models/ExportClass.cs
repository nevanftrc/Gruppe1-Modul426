using EasyWordWPF;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace EasyWordWPF_US5.Models
{
    /// <summary>
    /// Diese Klasse wird verwendet fürs exportieren und das schreiben für System einstellungen
    /// </summary>
    public class ExportClass
    {
        public string Germanword { get; set; }
        public string Englishword { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public string DefaultPath { get; set; }
        public string UserPath { get; set; }
        public bool UseDefault { get; set; }
        public string dataextension { get; set; }
        public bool UserBucketCount { get; set; }
        public int Buckets { get; set; }
        private readonly string appSettingsFilePath;
        private Buckets _bucket;
        public SettingsWindow settingsWindow { get; set; }
        public MainWindow main {  get; set; }
        public List<string> ExtensionsList { get; set; }
        /// <summary>
        /// Der constructor von Export/importer Klasse
        /// </summary>
        public ExportClass()
        {
            Germanword = string.Empty;
            Englishword = string.Empty;
            CorrectCount = 0;
            IncorrectCount = 0;
            DefaultPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics.json");
            appSettingsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            UserPath = string.Empty;
            UseDefault = true;
            dataextension = "JSON";
            // Initialize available extensions
            ExtensionsList = new List<string> { "JSON", "CSV", "TXT" };
        }
        /// <summary>
        /// Diese Methode Updatet die werte für appsettings JSON
        /// </summary>
        /// <param name="useDefault">Der Default Pfad bool</param>
        /// <param name="userPath">Benutzerdefnierter Pfad</param>
        /// <param name="extension">Datentyp</param>
        /// <param name="BucketCount">Anzahl einmer</param>
        /// <param name="bucket">Bool ob man nicht standart will</param>
        public void UpdateSettings(bool useDefault, string userPath, string extension, int BucketCount, bool bucket)
        {
            UseDefault = useDefault;
            UserPath = userPath.Replace('\\', '/'); // Normalize path for Linux/Windows compatibility

            var updatedSettings = new
            {
                DefaultPath = DefaultPath,
                UserPath = UserPath,
                UseDefault = UseDefault,
                DataExtension = extension,
                UserBucketCount = bucket,
                Buckets = BucketCount
            };

            string json = JsonConvert.SerializeObject(updatedSettings, Formatting.Indented);
            File.WriteAllText(appSettingsFilePath, json);
        }
        /// <summary>
        /// Der standart von appsettings werte
        /// </summary>
        public void EnsureAppSettings()
        {
            if (!File.Exists(appSettingsFilePath))
            {
                var defaultSettings = new
                {
                    DefaultPath = DefaultPath,
                    UserPath = UserPath,
                    UseDefault = UseDefault,
                    DataExtension = dataextension,
                    UserBucketCount = false,
                    Buckets = 5
                };

                string json = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                File.WriteAllText(appSettingsFilePath, json);
            }
        }
        /// <summary>
        /// List die werte von appsettings.json
        /// </summary>
        public void ReadSettings()
        {
            try
            {
                if (!File.Exists(appSettingsFilePath))
                {
                    Debug.WriteLine($"Settings file wurde nicht gefunden unter {appSettingsFilePath}. Es wird neu geschrieben..");
                    EnsureAppSettings();
                }

                var json = File.ReadAllText(appSettingsFilePath);
                var settings = JsonConvert.DeserializeObject<ExportClass>(json);

                if (settings != null)
                {
                    DefaultPath = settings.DefaultPath;
                    UserPath = settings.UserPath ?? string.Empty; // Null-safe assignment
                    UseDefault = settings.UseDefault;
                    dataextension = settings.dataextension;
                    UserBucketCount = settings.UserBucketCount;
                    Buckets = settings.Buckets;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ein fehler ist aufgetreten" + ex);
            }
        }
        /// <summary>
        /// Exportiert die daten in drei verschiedne typen (json,csv und txt)
        /// </summary>
        /// <param name="word">Deutsch</param>
        /// <param name="word2">Englisch</param>
        /// <param name="one">Korrekt</param>
        /// <param name="two">Falsch</param>
        /// <param name="comboboxValue">datentype</param>
        /// <param name="filepath">Der pfad</param>
        /// <param name="Userdefined">Wenn falsch wird es angepasst zu filepath</param>
        /// <param name="filename">den namen</param>
        public void exporterMethod(string word, string word2, int one, int two, string comboboxValue, string filepath, bool Userdefined, string filename, bool isGermanToEnglish)
        {
            // Get the AppData path and create a new subfolder for the application
            string appDataPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyWordExports");
            Directory.CreateDirectory(appDataPath); // Ensure the folder exists

            // Generate the filename
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string generatedFilename = string.IsNullOrWhiteSpace(filename) ? date : filename;

            // Determine the correct save path
            string savePath = (!string.IsNullOrWhiteSpace(filepath) && filepath != "Kein Pfad Vorhanden")
                ? filepath // Use the defined filepath if it's valid
                : appDataPath;

            // Determine the full file path with the correct extension
            string fullPath = System.IO.Path.HasExtension(savePath)
                ? savePath // If filepath already contains a full file path, use it directly
                : System.IO.Path.Combine(savePath, $"{generatedFilename}.{comboboxValue.ToLower()}");

            // Define correct labels based on quiz mode
            string firstLabel = isGermanToEnglish ? "German" : "English";
            string secondLabel = isGermanToEnglish ? "English" : "German";

            ReadSettings();

            int bucketCount = main?.ReturnValueLBL() ?? Buckets;

            int movement = two - one;

            int middleBucket = bucketCount / 2;

            int currentLocation = Math.Max(0, Math.Min(bucketCount - 1, middleBucket - movement));

            if (!isGermanToEnglish)
            {
                (word, word2) = (word2, word); // Swap words for English-to-German mode
            }

            switch (comboboxValue)
            {
                case "JSON":
                    {
                        // Create a new JSON object for the current entry with dynamic labels
                        var newEntry = new Dictionary<string, object>
                {
                    { firstLabel, word },
                    { secondLabel, word2 },
                    { "CorrectCount", one },
                    { "IncorrectCount", two },
                    { "BucketCount", bucketCount },
                    { "CurrentLocation", currentLocation }
                };

                        List<object> entries;

                        if (File.Exists(fullPath))
                        {
                            string existingJson = File.ReadAllText(fullPath);
                            try
                            {
                                entries = JsonConvert.DeserializeObject<List<object>>(existingJson) ?? new List<object>();
                            }
                            catch (JsonSerializationException)
                            {
                                var singleEntry = JsonConvert.DeserializeObject<object>(existingJson);
                                entries = new List<object> { singleEntry };
                            }
                        }
                        else
                        {
                            entries = new List<object>();
                        }

                        entries.Add(newEntry);
                        string jsonContent = JsonConvert.SerializeObject(entries, Formatting.Indented);
                        File.WriteAllText(fullPath, jsonContent);
                        break;
                    }
                case "CSV":
                case "TXT":
                    {
                        bool fileExists = File.Exists(fullPath);
                        using (StreamWriter writer = new StreamWriter(fullPath, true))
                        {
                            if (fileExists)
                            {
                                File.Delete(fullPath);
                                Debug.WriteLine("Der letzte heutige Stats wurde gelöscht.");
                            }
                            // If the file doesn't exist, write a header first
                            if (!fileExists)
                            {
                                writer.WriteLine($"Erstes Wort,Zweites Wort,CorrectCount,IncorrectCount,BucketCount,CurrentLocation");
                            }

                            // Append the new word entry
                            writer.WriteLine($"{word},{word2},{one},{two},{bucketCount},{currentLocation}");
                        }
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Kein Wert wurde gefunden.");
                        break;
                    }
            }
        }
    }
}