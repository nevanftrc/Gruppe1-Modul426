using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using EasyWordWPF;
using EasyWordWPF_US5.Models;
using Microsoft.Win32;

namespace EasyWordWPF_US5
{
    public partial class MainWindow : Window
    {

        public bool isCaseSensitive { get; set; } = true; // Standardmäßig Groß-/Kleinschreibung beachten


        // Vergleichsmethode, die isCaseSensitive berücksichtigt
        private bool CompareAnswer(string input, string correctAnswer)
        {
            return isCaseSensitive
                ? input.Trim().Equals(correctAnswer)  // Beachtet die Groß-/Kleinschreibung
                : input.Trim().Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);  // Ignoriert die Groß-/Kleinschreibung
        }









        private void OpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            // Erstelle ein OpenFileDialog-Objekt
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",  // Zeige nur CSV-Dateien an
                Title = "Wählen Sie eine CSV-Datei aus"
            };

            // Zeige den Dialog an und überprüfe, ob der Benutzer eine Datei ausgewählt hat
            if (openFileDialog.ShowDialog() == true)
            {
                // Der Benutzer hat eine Datei ausgewählt
                string selectedFilePath = openFileDialog.FileName;

                try
                {
                    var newCsvData = File.ReadAllLines(selectedFilePath).ToList();
                    List<(string, string)> tempList = new List<(string, string)>();

                    // CSV-Datei zeilenweise einlesen
                    foreach (var line in newCsvData)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split(';');
                        if (parts.Length != 2)
                        {
                            MessageBox.Show($"Fehlerhafte Zeile: {line}", "Importfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        tempList.Add((parts[0].Trim(), parts[1].Trim()));  // Wörter zur Liste hinzufügen
                    }

                    // Aktualisiere die Wortliste
                    wordList = tempList;
                    incorrectWords.Clear();  // Leere die Liste der falschen Antworten
                    MessageBox.Show("Die CSV-Datei wurde erfolgreich geladen.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Hinweis, dass das Quiz manuell gestartet werden muss
                    MessageBox.Show("Klicken Sie auf 'Quiz Starten', um das Quiz zu beginnen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Importieren der Datei: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private Buckets myBucket;
        private StatisticsService statisticsService;
        // Eigenschaften für die Software-Informationen


        public string DeveloperName { get; set; } = "Gruppe1";



        
        public string Version { get; set; } = "1.0.0";
        // BuildDate dynamisch setzen
        public string BuildDate { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");


        private List<(string German, string English)> wordList = new List<(string German, string English)>();
        private List<(string, string)> incorrectWords = new List<(string, string)>();
        private Random random = new Random();
        private int currentWordIndex = -1;
        private bool isGermanToEnglish = true; // Default mode is D->E

        private List<string> currentCsvData = new List<string>();

        public MainWindow()
        {
            //InitializeComponent();
            DataContext = this; // Setze den DataContext auf die aktuelle Instanz der MainWindow-Klasse

            InitializeComponent();
            myBucket = new Buckets();
            statisticsService = new StatisticsService();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();


        }

        private void LoadUserData()
        {
            var loadedData = DataStorage.Load();
            if (loadedData != null)
            {
                // Sprachmodus wiederherstellen
                isGermanToEnglish = loadedData.IsGermanToEnglish;

                // WordList wiederherstellen
                wordList.Clear();
                foreach (var wp in loadedData.WordList)
                {
                    wordList.Add((wp.German, wp.English));
                }

                // incorrectWords wiederherstellen
                incorrectWords.Clear();
                foreach (var wp in loadedData.IncorrectWords)
                {
                    incorrectWords.Add((wp.German, wp.English));
                }

                // Button-Inhalt anpassen
                string imagePath = isGermanToEnglish ? "/germany.png" : "/uk.png";
                langswitchImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
        }


        private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Select a CSV file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var newCsvData = File.ReadAllLines(openFileDialog.FileName).ToList();
                    List<(string, string)> tempList = new List<(string, string)>();

                    foreach (var line in newCsvData)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split(';');
                        if (parts.Length != 2)
                        {
                            MessageBox.Show($"Fehlerhafte Zeile: {line}", "Importfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        tempList.Add((parts[0].Trim(), parts[1].Trim()));
                    }

                    if (currentCsvData.Any())
                    {
                        var result = MessageBox.Show(
                            "Es existiert bereits ein CSV-Datensatz. Möchten Sie die neue Datei hinzufügen oder die bestehende ersetzen?",
                            "CSV-Import",
                            MessageBoxButton.YesNoCancel,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            currentCsvData = newCsvData;
                            wordList = tempList;
                            incorrectWords.Clear();
                            MessageBox.Show("Die alte CSV-Datei wurde ersetzt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            currentCsvData.AddRange(newCsvData);
                            wordList.AddRange(tempList);
                            MessageBox.Show("Die CSV-Dateien wurden kombiniert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Der Import wurde abgebrochen.", "Abbruch", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    else
                    {
                        currentCsvData = newCsvData;
                        wordList = tempList;
                        incorrectWords.Clear();
                        MessageBox.Show("Die CSV-Datei wurde erfolgreich geladen.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Importieren der Datei: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            if (!wordList.Any())
            {
                MessageBox.Show("Bitte importieren Sie eine Wörterliste.", "Keine Wörter vorhanden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            incorrectWords.Clear();
            StartQuizLoop();
        }

        private void StartQuizLoop()
        {
            while (wordList.Any())
            {
                currentWordIndex = random.Next(wordList.Count);
                var currentWord = wordList[currentWordIndex];

                string question = isGermanToEnglish
                    ? $"Wie lautet die englische Übersetzung von '{currentWord.Item1}'?"
                    : $"Wie lautet die deutsche Übersetzung von '{currentWord.Item2}'?";

                string input = Microsoft.VisualBasic.Interaction.InputBox(question, "Wortquiz", "");

                if (string.IsNullOrWhiteSpace(input))
                {
                    MessageBox.Show("Eingabe abgebrochen. Quiz wird beendet.", "Abbruch", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                bool isCorrect = isGermanToEnglish
                    ? (isCaseSensitive ? input.Trim().Equals(currentWord.Item2) : input.Trim().Equals(currentWord.Item2, StringComparison.OrdinalIgnoreCase))
                    : (isCaseSensitive ? input.Trim().Equals(currentWord.Item1) : input.Trim().Equals(currentWord.Item1, StringComparison.OrdinalIgnoreCase));

                if (isCorrect)
                {
                    MessageBox.Show("Korrekt!", "Richtig", MessageBoxButton.OK, MessageBoxImage.Information);
                    wordList.RemoveAt(currentWordIndex);
                }
                else
                {
                    string correctAnswer = isGermanToEnglish ? currentWord.Item2 : currentWord.Item1;
                    MessageBox.Show($"Falsch! Die richtige Antwort war: {correctAnswer}", "Falsch", MessageBoxButton.OK, MessageBoxImage.Error);
                    incorrectWords.Add(currentWord);
                }
            }
        }


        private void ResetStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie wirklich alle Statistiken zurücksetzen?", "Bestätigung", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                statisticsService.ResetStatistics();
                MessageBox.Show("Statistiken wurden zurückgesetzt.", "Info", MessageBoxButton.OK);
            }
        }

        private void SwitchModeButton_Click(object sender, RoutedEventArgs e)
        {
            isGermanToEnglish = !isGermanToEnglish;
            string mode = isGermanToEnglish ? "Deutsch -> Englisch" : "Englisch -> Deutsch";
            MessageBox.Show($"Modus geändert: {mode}", "Moduswechsel", MessageBoxButton.OK, MessageBoxImage.Information);
            string imagePath = isGermanToEnglish ? "/germany.png" : "/uk.png";
            langswitchImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void UpdateBucketCountLabel()
        {
            numtxtbo.Content = myBucket.bucket_count.ToString();
        }
        private void bucketAdd(object sender, RoutedEventArgs e)
        {
            try
            {
                myBucket.bucket_add(1); // Add one bucket
                UpdateBucketCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler for removing a bucket
        private void bucketRem(object sender, RoutedEventArgs e)
        {
            try
            {
                myBucket.bucket_remove(1); // Remove one bucket
                UpdateBucketCountLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Daten für das Speichern vorbereiten
            var data = new DataStorage.UserData
            {
                IsGermanToEnglish = isGermanToEnglish,
                WordList = new List<DataStorage.WordPair>(),
                IncorrectWords = new List<DataStorage.WordPair>()
            };

            // incorrectWords in die speicherbare Klasse umwandeln
            foreach (var w in incorrectWords)
            {
                data.IncorrectWords.Add(new DataStorage.WordPair
                {
                    German = w.Item1,
                    English = w.Item2
                });
            }

            // wordList in die speicherbare Klasse umwandeln
            foreach (var w in wordList)
            {
                data.WordList.Add(new DataStorage.WordPair
                {
                    German = w.Item1,
                    English = w.Item2
                });
            }

            // Jetzt speichern
            DataStorage.Save(data);
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(this);  // Übergibt die Instanz von MainWindow
            settingsWindow.Show();
        }

        private void OpenInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoDialog infoDialog = new InfoDialog();
            infoDialog.Show();
        }
    }
}
//Test