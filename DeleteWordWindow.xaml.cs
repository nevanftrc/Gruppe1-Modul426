using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EasyWordWPF_US5
{
    /// <summary>
    /// Interaktionslogik für DeleteWordWindow.xaml
    /// </summary>
    public partial class DeleteWordWindow : Window
    {
        private MainWindow _mainWindow;

        // Hilfsklasse zur Darstellung eines Wortpaares im ListBox-Eintrag
        public class WordPairDisplay
        {
            public string German { get; set; }
            public string English { get; set; }
            public string DisplayText => $"{German} - {English}";
        }

        public DeleteWordWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadWordList();
        }

        private void LoadWordList()
        {
            // Hole die aktuelle Wortliste aus MainWindow
            List<(string German, string English)> words = _mainWindow.GetWordList();
            List<WordPairDisplay> displayList = words
                .Select(w => new WordPairDisplay { German = w.German, English = w.English })
                .ToList();
            WordListBox.ItemsSource = displayList;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (WordListBox.SelectedItem is WordPairDisplay selected)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Möchten Sie das Wortpaar '{selected.DisplayText}' wirklich löschen?",
                    "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Lösche das ausgewählte Wortpaar aus der Kartei (MainWindow)
                    _mainWindow.DeleteWord(selected.German, selected.English);
                    LoadWordList();
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie ein Wortpaar aus.", "Hinweis",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
