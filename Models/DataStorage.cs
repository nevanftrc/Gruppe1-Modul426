using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyWordWPF_US5.Models
{
    /// <summary>
    /// Hilfsklasse, um Einstellungen und Wortlisten als JSON zu laden und zu speichern.
    /// </summary>
    public static class DataStorage
    {
        // Legen wir den Pfad im AppData-Verzeichnis an, damit die Datei nicht direkt sichtbar ist.
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasyWordWPF_US5"
        );

        // Datei, in der wir unsere Daten speichern
        private static readonly string StorageFilePath = Path.Combine(AppDataFolder, "UserData.json");

        /// <summary>
        /// Struktur, die alle Daten enthält, die wir als JSON speichern wollen.
        /// </summary>
        public class UserData
        {
            public bool IsGermanToEnglish { get; set; }

            // Statt (string, string) machen wir eine kleine Hilfsklasse für Serialisierung.
            public List<WordPair> WordList { get; set; } = new List<WordPair>();
            public List<WordPair> IncorrectWords { get; set; } = new List<WordPair>();
        }

        /// <summary>
        /// Für die korrekte Serialisierung brauchen wir eine Klasse mit Properties.
        /// </summary>
        public class WordPair
        {
            public string German { get; set; }
            public string English { get; set; }
        }

        /// <summary>
        /// Speichert die übergebenen Daten als JSON auf die Festplatte.
        /// </summary>
        public static void Save(UserData data)
        {
            try
            {
                // Falls das AppData-Verzeichnis noch nicht existiert, erstellen
                if (!Directory.Exists(AppDataFolder))
                    Directory.CreateDirectory(AppDataFolder);

                // JSON-Optionen (falls du es eingerückt haben möchtest)
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(data, options);

                File.WriteAllText(StorageFilePath, jsonString);
            }
            catch (Exception ex)
            {
                // Hier könnte man Logging einbauen oder eine Meldung anzeigen
                Console.WriteLine("Fehler beim Speichern der Daten: " + ex.Message);
            }
        }

        /// <summary>
        /// Lädt die Daten aus der JSON-Datei, falls sie existiert. Gibt null zurück, wenn nichts existiert oder Fehler.
        /// </summary>
        public static UserData Load()
        {
            try
            {
                if (!File.Exists(StorageFilePath))
                    return null; // Keine gespeicherten Daten vorhanden

                var jsonString = File.ReadAllText(StorageFilePath);
                var options = new JsonSerializerOptions();
                var data = JsonSerializer.Deserialize<UserData>(jsonString, options);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Laden der Daten: " + ex.Message);
                return null;
            }
        }
    }
}