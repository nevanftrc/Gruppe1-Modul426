using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EasyWordWPF_US5.Models  // ✅ Namespace hinzufügen
{
    public static class DataStorage
    {
        private static readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_data.json");

        public class UserData
        {
            public bool IsGermanToEnglish { get; set; } = true;
            public List<WordPair> WordList { get; set; } = new List<WordPair>();
            public List<WordPair> IncorrectWords { get; set; } = new List<WordPair>();
        }

        public class WordPair
        {
            public string German { get; set; }
            public string English { get; set; }
        }

        public static void Save(UserData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static UserData Load()
        {
            if (!File.Exists(filePath))
                return new UserData();

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<UserData>(json);
        }
    }
}
