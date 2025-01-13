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
        public void exporterMethod(string word, string word2, int one, int two, string comboboxValue, string filename, bool Userdefined)
        {
            // Get the AppData path and create a new subfolder for the application
            string appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyWordExports");
            Directory.CreateDirectory(appDataPath); // Ensure the folder exists

            // Generate the filename
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string generatedFilename = string.IsNullOrWhiteSpace(filename) ? date : filename;

            // Initialize fullPath to the inherited JSON path if Userdefined is false
            string fullPath = !Userdefined && comboboxValue.Equals("JSON", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(appDataPath, "default.json") // Example JSON path
                : string.Empty;

            switch (comboboxValue)
            {
                case "JSON":
                    {
                        // If Userdefined is true, allow setting a new path
                        if (Userdefined)
                        {
                            fullPath = Path.Combine(appDataPath, $"{generatedFilename}.json");
                        }
                        else
                        {
                            // Inherit default JSON path
                            fullPath = Path.Combine(appDataPath, "default.json");
                        }

                        // Save JSON content (example)
                        string jsonContent = $"{{ \"German\": \"{word}\", \"eng\": \"{word2}\", \"one\": {one}, \"two\": {two} }}";
                        File.WriteAllText(fullPath, jsonContent);

                        //MessageBox.Show($"JSON data saved under {fullPath}");
                        break;
                    }
                case "CSV":
                    {
                        // Create the file path with .csv extension
                        fullPath = Path.Combine(appDataPath, $"{generatedFilename}.csv");

                        // Write data in CSV format
                        string csvContent = $"{word}, {word2}, {one}, {two}{Environment.NewLine}";
                        File.AppendAllText(fullPath, csvContent);

                        //MessageBox.Show($"CSV data saved under {fullPath}");
                        break;
                    }
                case "TXT":
                    {
                        // Create the file path with .txt extension
                        fullPath = Path.Combine(appDataPath, $"{generatedFilename}.txt");

                        // Write data in TXT format
                        string txtContent = $"{word}, {word2}, {one}, {two}{Environment.NewLine}";
                        File.AppendAllText(fullPath, txtContent);

                        //MessageBox.Show($"TXT data saved under {fullPath}");
                        break;
                    }
                default:
                    {
                        MessageBox.Show("No value was found.");
                        break;
                    }
            }
        }
    }
}