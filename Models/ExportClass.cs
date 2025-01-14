using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EasyWordWPF_US5.Models
{
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

        private readonly string appSettingsFilePath;

        public SettingsWindow settingsWindow { get; set; }
        public List<string> ExtensionsList { get; set; }

        public ExportClass()
        {
            Germanword = string.Empty;
            Englishword = string.Empty;
            CorrectCount = 0;
            IncorrectCount = 0;
            DefaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics.json");
            appSettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            UserPath = string.Empty;
            UseDefault = true;
            dataextension = "JSON";
            // Initialize available extensions
            ExtensionsList = new List<string> { "JSON", "CSV", "TXT" };
        }

        public void UpdateSettings(bool useDefault, string userPath, string extension)
        {
            UseDefault = useDefault;
            UserPath = userPath.Replace('\\', '/'); // Normalize path for Linux/Windows compatibility

            var updatedSettings = new
            {
                DefaultPath = DefaultPath,
                UserPath = UserPath,
                UseDefault = UseDefault,
                DataExtension = extension
            };

            string json = JsonConvert.SerializeObject(updatedSettings, Formatting.Indented);
            File.WriteAllText(appSettingsFilePath, json);
        }

        public void EnsureAppSettings()
        {
            if (!File.Exists(appSettingsFilePath))
            {
                var defaultSettings = new
                {
                    DefaultPath = DefaultPath,
                    UserPath = UserPath,
                    UseDefault = UseDefault,
                    DataExtension = dataextension
                };

                string json = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                File.WriteAllText(appSettingsFilePath, json);
            }
        }

        public void ReadSettings()
        {
            try
            {
                if (!File.Exists(appSettingsFilePath))
                {
                    Console.WriteLine($"Settings file not found at {appSettingsFilePath}. Creating default settings.");
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ein fehler ist aufgetreten" + ex);
            }
        }
        public void exporterMethod(string word, string word2, int one, int two, string comboboxValue, string filepath, bool Userdefined, string filename)
        {
            // Get the AppData path and create a new subfolder for the application
            string appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyWordExports");
            Directory.CreateDirectory(appDataPath); // Ensure the folder exists

            // Generate the filename
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string generatedFilename = string.IsNullOrWhiteSpace(filepath) ? date : filename;

            // Initialize fullPath to the inherited JSON path if Userdefined is false
            string fullPath = string.Empty;

            // Precompute the path before entering the switch
            if (Userdefined && !string.IsNullOrWhiteSpace(filepath))
            {
                fullPath = Path.Combine(filepath, $"{generatedFilename}.json");
            }
            else if (comboboxValue.Equals("JSON", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = Path.Combine(appDataPath, $"{generatedFilename}.json");
            }
            else
            {
                fullPath = Path.Combine(appDataPath, $"{generatedFilename}.{comboboxValue.ToLower()}");
            }

            switch (comboboxValue)
            {
                case "JSON":
                    {
                        if (Userdefined)
                        {
                            fullPath = Path.Combine(appDataPath, $"{generatedFilename}.json");
                        }
                        else
                        {
                            // Default JSON path
                            fullPath = Path.Combine(appDataPath,  $"{generatedFilename}.json");
                        }

                        // Create a new JSON object for the current entry
                        var newEntry = new
                        {
                            German = word,
                            English = word2,
                            CorrectCount = one,
                            IncorrectCount = two
                        };

                        // Prepare to handle the JSON file
                        List<object> entries;

                        if (File.Exists(fullPath))
                        {
                            // Read the existing JSON file content
                            string existingJson = File.ReadAllText(fullPath);

                            try
                            {
                                // Try to parse as an array
                                entries = JsonConvert.DeserializeObject<List<object>>(existingJson) ?? new List<object>();
                            }
                            catch (JsonSerializationException)
                            {
                                // If parsing fails, treat the file as a single object
                                var singleEntry = JsonConvert.DeserializeObject<object>(existingJson);
                                entries = new List<object> { singleEntry };
                            }
                        }
                        else
                        {
                            // If the file doesn't exist, start with a new list
                            entries = new List<object>();
                        }

                        // Add the new entry to the list
                        entries.Add(newEntry);

                        // Serialize the updated list to JSON
                        string jsonContent = JsonConvert.SerializeObject(entries, Formatting.Indented);

                        // Write the updated JSON back to the file
                        File.WriteAllText(fullPath, jsonContent);

                        break;

                    }
                case "CSV":
                    {
                        // Create the file path with .csv extension
                        fullPath = Path.Combine(appDataPath, $"{generatedFilename}.csv");

                        // Write data in CSV format
                        string csvContent = $"{word}, {word2}, {one}, {two}{Environment.NewLine}";
                        File.AppendAllText(fullPath, csvContent);

                        break;
                    }
                case "TXT":
                    {
                        // Create the file path with .txt extension
                        fullPath = Path.Combine(appDataPath, $"{generatedFilename}.txt");

                        // Write data in TXT format
                        string txtContent = $"{word}, {word2}, {one}, {two}{Environment.NewLine}";
                        File.AppendAllText(fullPath, txtContent);

                        break;
                    }
                default:
                    {
                        MessageBox.Show("No value was found.");
                        break;
                    }
            }
        }

        internal void exporterMethod(string word, string word2, int one, int two, string comboboxValue, string filename, bool Userdefined)
        {
            throw new NotImplementedException();
        }
    }
}