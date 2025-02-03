﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using EasyWordWPF;
using EasyWordWPF_US5.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static EasyWordWPF_US5.Models.DataStorage;

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
                    var newCsvData = System.IO.File.ReadAllLines(selectedFilePath).ToList();
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
        private ExportClass exportClass;
        private SettingsWindow settingsWindow;
        private ImportWindow importWindow;
        public NewDictonary Newdictonary;
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
            Newdictonary = new NewDictonary();
            myBucket = new Buckets();
            importWindow = new ImportWindow(myBucket);
            statisticsService = new StatisticsService(myBucket) { main = this };
            settingsWindow = new SettingsWindow(this);
            //Initialize json
            exportClass = new ExportClass();
            exportClass.EnsureAppSettings();
            exportClass.ReadSettings();
            SetBucketCountLabel();
            this.Closing += MainWindow_Closing;

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
                    var newCsvData = System.IO.File.ReadAllLines(openFileDialog.FileName).ToList();
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

        string importlocationpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imported_data.json");

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {

            if (File.Exists(importlocationpath)) 
            {
                var result = MessageBox.Show("Wollen sie den geladen stand benützen?", "Verify", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    if (!wordList.Any())
                    {
                        MessageBox.Show("Bitte importieren Sie eine Wörterliste.", "Keine Wörter vorhanden", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    MessageBox.Show("Es wird ein neuer stand verwendet.");
                    incorrectWords.Clear();
                    StartQuizLoop();
                }
                if (result == MessageBoxResult.Yes)
                {
                    incorrectWords.Clear();
                    StartQuizLoopImported();
                }
            }
            if (!File.Exists(importlocationpath))
            {
                if (!wordList.Any())
                {
                    MessageBox.Show("Bitte importieren Sie eine Wörterliste.", "Keine Wörter vorhanden", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                incorrectWords.Clear();
                StartQuizLoop();
            }
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
                    UpdateStatistics(currentWord.German, currentWord.English, true);
                    CSVlist wordObject = new CSVlist { de_words = currentWord.Item1, en_words = currentWord.Item2 };
                    myBucket.MoveWord(wordObject, 1, 0);
                    wordList.RemoveAt(currentWordIndex);
                }
                else
                {
                    string correctAnswer = isGermanToEnglish ? currentWord.Item2 : currentWord.Item1;
                    MessageBox.Show($"Falsch! Die richtige Antwort war: {correctAnswer}", "Falsch", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatistics(currentWord.German, currentWord.English, false);
                    CSVlist wordObject = new CSVlist { de_words = currentWord.Item1, en_words = currentWord.Item2 };
                    myBucket.MoveWord(wordObject, 0, 1);
                    incorrectWords.Add(currentWord);
                }
            }
        }
        /// <summary>
        /// spielt den quiz mit dem importierte daten
        /// </summary>
        private void StartQuizLoopImported()
        {
            string importedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imported_data.json");

            if (!File.Exists(importedFilePath))
            {
                MessageBox.Show("Keine importierten Daten gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Load and parse JSON content
                string jsonContent = File.ReadAllText(importedFilePath);
                var importedData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, dynamic>>>(jsonContent);
                var importedWordList = importedData?.Values
                    .Select(entry => (entry["German"].ToString(), entry["English"].ToString()))
                    .Where(entry => isGermanToEnglish ? IsGermanWord(entry.Item1) : IsEnglishWord(entry.Item2))  // Filter German or English words
                    .ToList();

                if (importedWordList == null || !importedWordList.Any())
                {
                    MessageBox.Show("Die importierte Wortliste enthält keine passenden Wörter.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                while (importedWordList.Any())
                {
                    int currentWordIndex = random.Next(importedWordList.Count);
                    var currentWord = importedWordList[currentWordIndex];

                    string question = isGermanToEnglish
                        ? $"Wie lautet die englische Übersetzung von '{currentWord.Item1}'?"
                        : $"Wie lautet die deutsche Übersetzung von '{currentWord.Item2}'?";

                    string input = Microsoft.VisualBasic.Interaction.InputBox(question, "Importiertes Wortquiz", "");

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
                        UpdateStatistics(currentWord.Item1, currentWord.Item2, true);
                        CSVlist wordObject = new CSVlist { de_words = currentWord.Item1, en_words = currentWord.Item2 };
                        myBucket.MoveWord(wordObject, 1, 0);
                        importedWordList.RemoveAt(currentWordIndex);
                    }
                    else
                    {
                        string correctAnswer = isGermanToEnglish ? currentWord.Item2 : currentWord.Item1;
                        MessageBox.Show($"Falsch! Die richtige Antwort war: {correctAnswer}", "Falsch", MessageBoxButton.OK, MessageBoxImage.Error);
                        UpdateStatistics(currentWord.Item1, currentWord.Item2, false);
                        CSVlist wordObject = new CSVlist { de_words = currentWord.Item1, en_words = currentWord.Item2 };
                        myBucket.MoveWord(wordObject, 0, 1);
                        incorrectWords.Add((currentWord.Item1, currentWord.Item2));
                    }
                }

                MessageBox.Show("Alle importierten Wörter wurden abgefragt!", "Quiz beendet", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der importierten Daten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Checks if a word is likely German based on common patterns and dictionary entries.
        /// </summary>
        private bool IsGermanWord(string word)
        {
            string[] germanIndicators = { "ä", "ö", "ü", "ss", "sch", "ch", "ung", "keit", "heit", "zig", "pf" };

            return germanIndicators.Any(indicator => word.Contains(indicator)) || Newdictonary.GetWords().Contains(word.ToLower());
        }
        /// <summary>
        /// Checks if a word is likely English (opposite of IsGermanWord).
        /// </summary>
        private bool IsEnglishWord(string word)
        {
            return !IsGermanWord(word);  // If it's not German, assume it's English
        }

        private void UpdateStatistics(string german, string english, bool correct)
        {
            var stats = statisticsService.GetOrCreateStatistics(german, english, isGermanToEnglish);

            if (correct)
            {
                stats.CorrectCount++;
            }
            else
            {
                stats.IncorrectCount++;
            }

            // Save statistics after updating
            statisticsService.SaveStatistics();
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
        /// <summary>
        /// passt die anzahl an
        /// </summary>
        private void UpdateBucketCountLabel()
        { 
            numtxtbo.Content = myBucket.bucket_count.ToString();
        }
        /// <summary>
        /// applies die nummer
        /// </summary>
        private void SetBucketCountLabel()
        {
            exportClass.ReadSettings(); // Load latest settings

            int savedBucketCount = exportClass.Buckets; // Get bucket count from JSON

            // Ensure UI updates correctly
            numtxtbo.Content = exportClass.UserBucketCount ? savedBucketCount.ToString() : "5";

            Debug.WriteLine($"[DEBUG] Loaded Bucket Count: {savedBucketCount}, UseDefault: {exportClass.UseDefault}");
        }
        /// <summary>
        /// addiert eimer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// entfernt eimer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            exportClass.ReadSettings();
            settingsWindow.checkcheckboxsettings(exportClass.UseDefault);
            settingsWindow.checkcheckboxeimer(exportClass.UserBucketCount);

        }
        private bool clicked = false;
        /// <summary>
        /// Export click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // Declare clicked as a class-level variable
        private void OnFileExport_Click(object sender, RoutedEventArgs e)
        {
            exportClass.ReadSettings();

            // Toggle clicked state
            if (clicked)
            {
                // Delete the previously generated file if it exists
                string appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyWordExports");
                string username = Environment.UserName;
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string filename = $"{username}_{date}.{exportClass.dataextension.ToLower()}";
                string exportFilePath = Path.Combine(exportClass.UseDefault || string.IsNullOrWhiteSpace(exportClass.UserPath)
                    ? appDataPath
                    : exportClass.UserPath, filename);

                if (File.Exists(exportFilePath))
                {
                    File.Delete(exportFilePath);
                    MessageBox.Show($"Datei wurde gelöscht: {exportFilePath}");
                }
                else
                {
                    MessageBox.Show("Keine Datei zum Löschen gefunden.");
                }

                // Reset clicked flag
                clicked = false;
                return;
            }

            try
            {
                // Path to the JSON statistics file
                string loaderFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics.json");

                // Ensure the directory for exports exists
                string appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyWordExports");
                Directory.CreateDirectory(appDataPath);

                // Generate the base filename
                string username = Environment.UserName;
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string filename = $"{username}_{date}";

                // Check if statistics file exists
                if (!System.IO.File.Exists(loaderFilePath))
                {
                    MessageBox.Show("Statistics file nicht gefunden in AppData.");
                    return;
                }

                // Load and parse JSON content
                string jsonContent = System.IO.File.ReadAllText(loaderFilePath);
                var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonContent);

                if (jsonObject != null)
                {
                    string lastExportPath = string.Empty;
                    string exportDirectory = exportClass.UseDefault || string.IsNullOrWhiteSpace(exportClass.UserPath)
                        ? appDataPath
                        : exportClass.UserPath;

                    // Ensure directory exists
                    Directory.CreateDirectory(exportDirectory);

                    // Determine a unique filename to prevent overwriting
                    string fileExtension = exportClass.dataextension.ToLower();
                    string exportFilePath = Path.Combine(exportDirectory, $"{filename}.{fileExtension}");

                    int fileNumber = 1;
                    while (File.Exists(exportFilePath))
                    {
                        exportFilePath = Path.Combine(exportDirectory, $"{filename}_{fileNumber}.{fileExtension}");
                        fileNumber++;
                    }

                    lastExportPath = exportFilePath;

                    foreach (var innerValue in jsonObject.Values)
                    {
                        // Extract the relevant fields
                        string germanWord = innerValue["German"]?.ToString() ?? string.Empty;
                        string englishWord = innerValue["English"]?.ToString() ?? string.Empty;
                        int correctCount = innerValue["CorrectCount"] != null ? (int)innerValue["CorrectCount"] : 0;
                        int incorrectCount = innerValue["IncorrectCount"] != null ? (int)innerValue["IncorrectCount"] : 0;

                        // Call the export method with the extracted data
                        exportClass.ExporterMethod(
                            word: germanWord,
                            word2: englishWord,
                            one: correctCount,
                            two: incorrectCount,
                            comboboxValue: exportClass.dataextension,
                            filepath: exportFilePath,
                            Userdefined: exportClass.UseDefault,
                            filename: filename,
                            isGermanToEnglish: isGermanToEnglish
                        );
                    }

                    MessageBox.Show($"{exportClass.dataextension} daten wurden erfolgreich gespeichert unter {lastExportPath}");
                }
                else
                {
                    MessageBox.Show("Keine daten wurden gefunden unter statistics file.");
                }

                // Set clicked to true to enable delete mode next time
                clicked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void OpenInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoDialog infoDialog = new InfoDialog();
            infoDialog.Show();
        }
        private void ResetAllWordsButton_Click(object sender, RoutedEventArgs e)
        {
            // Sicherheitsabfrage, damit der Benutzer den Vorgang bestätigen muss
            if (MessageBox.Show("Möchten Sie wirklich alle Wörter zurücksetzen? " +
                                "Dies löscht auch die Statistik.",
                                "Bestätigung",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // 1) Wörter leeren
                wordList.Clear();
                incorrectWords.Clear();

                // 2) Statistik zurücksetzen
                statisticsService.ResetStatistics();

                // 3) Leere User-Daten speichern
                var emptyData = new DataStorage.UserData
                {
                    IsGermanToEnglish = isGermanToEnglish,
                    // Oder auf einen Standardwert setzen:
                    // IsGermanToEnglish = true,
                    WordList = new List<DataStorage.WordPair>(),
                    IncorrectWords = new List<DataStorage.WordPair>()
                };
                DataStorage.Save(emptyData);

                // Hinweis an den Benutzer
                MessageBox.Show("Alle Wörter und Statistiken wurden gelöscht. " +
                                "Bitte importieren Sie neue Wörter, um fortzufahren.",
                                "Erfolg",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// benendet die anwendung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Wollen sie die App Beenden?", "Exit", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; // Prevent the window from closing
            }
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        /// <summary>
        /// returns den wert von label
        /// </summary>
        /// <returns>anzahl</returns>
        public int ReturnValueLBL()
        {
            UpdateBucketCountLabel(); // Ensure label is updated before reading

            return myBucket.bucket_count;
        }
        private void ImportFile_Click(object sender, RoutedEventArgs e) 
        {
            ImportWindow ImporterWindow = new ImportWindow(myBucket); 
            ImporterWindow.Show();
        }
    }
}
//Test