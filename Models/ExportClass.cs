using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            }
        }

    }
}
