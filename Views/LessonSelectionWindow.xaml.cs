using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace EasyWordWPF_US5
{
    public partial class LessonSelectionWindow : Window
    {
        public List<LessonItem> LessonItems { get; set; }
        public List<string> SelectedLessons { get; private set; }

        public LessonSelectionWindow()
        {
            InitializeComponent();
            LoadLessons();
            LessonListBox.ItemsSource = LessonItems;
        }

        private void LoadLessons()
        {
            LessonItems = new List<LessonItem>();
            // Füge den "Alle" Eintrag hinzu
            LessonItems.Add(new LessonItem { Name = "Alle", IsSelected = true });

            // Pfad zum "words"-Ordner (im Basisverzeichnis der App)
            string wordsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words");

            if (Directory.Exists(wordsFolder))
            {
                // Alle CSV-Dateien im Ordner "words"
                var csvFiles = Directory.GetFiles(wordsFolder, "*.csv");
                foreach (var file in csvFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    // Füge nur hinzu, falls nicht schon vorhanden (z. B. durch "Alle")
                    if (!LessonItems.Any(li => li.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        LessonItems.Add(new LessonItem { Name = fileName, IsSelected = false });
                    }
                }
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            // Wenn "Alle" ausgewählt ist, wird ausschließlich "Alle" verwendet
            var alleItem = LessonItems.FirstOrDefault(li => li.Name.Equals("Alle", StringComparison.OrdinalIgnoreCase));
            if (alleItem != null && alleItem.IsSelected)
            {
                SelectedLessons = new List<string> { "Alle" };
            }
            else
            {
                SelectedLessons = LessonItems
                    .Where(li => li.IsSelected)
                    .Select(li => li.Name)
                    .ToList();
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class LessonItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
