using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using EasyWordWPF;
using EasyWordWPF_US5.Models;
using EasyWordWPF_US5.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static EasyWordWPF_US5.Models.DataStorage;



namespace EasyWordWPF_US5
{
    public partial class MainWindow : Window
    {

        private Buckets wordBuckets; // Instanz der bestehenden Bucket-Klasse
        public ObservableCollection<KeyValuePair<string, int>> BucketOverview { get; set; }

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
                string lessonName = Path.GetFileNameWithoutExtension(selectedFilePath);

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
                    LoadWordsFromFile(selectedFilePath);
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
        private string currentWord = string.Empty;
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
            wordBuckets = new Buckets(); // Initialisiere die Buckets
            BucketOverview = new ObservableCollection<KeyValuePair<string, int>>();

            BucketList.ItemsSource = BucketOverview; // Datenbindung für UI
            UpdateBucketOverview(); // Initiale Anzeige der Bucket-Werte
            CopyWords();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();
        }

        private void LoadUserData()
        {
            var loadedData = DataStorage.Load();

            isGermanToEnglish = loadedData.IsGermanToEnglish; // Lade die Spracheinstellung

            wordList.Clear();
            foreach (var wp in loadedData.WordList)
            {
                wordList.Add((wp.German, wp.English));
            }

            incorrectWords.Clear();
            foreach (var wp in loadedData.IncorrectWords)
            {
                incorrectWords.Add((wp.German, wp.English));
            }

            // Button-Inhalt anpassen (falls es ein Sprache-Wechsel-Button gibt)
            string imagePath = isGermanToEnglish ? "/germany.png" : "/uk.png";
            langswitchImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }
        

        private void UpdateBucketOverview()
        {
            BucketOverview.Clear(); // Zurücksetzen der alten Werte

            for (int i = 0; i < wordBuckets.bucket_count; i++)
            {
                BucketOverview.Add(new KeyValuePair<string, int>($"Bucket {i + 1}", wordBuckets.buckets[i].Count));
            }

            Dispatcher.Invoke(() =>
            {
                BucketList.ItemsSource = null;
                BucketList.ItemsSource = BucketOverview;
            });
        }

        // Methode zum Verschieben eines Wortes zwischen Buckets
        private void MoveWordToNextBucket(CSVlist word, int correctCount, int incorrectCount)
        {
            wordBuckets.MoveWord(word, correctCount, incorrectCount);
            UpdateBucketOverview(); // Aktualisiere die UI sofort
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
                    string lessonName = "Unknown";
                    UpdateStatistics(currentWord.German, currentWord.English, true, lessonName);
                    CSVlist wordObject = new CSVlist { de_words = currentWord.Item1, en_words = currentWord.Item2 };
                    myBucket.MoveWord(wordObject, 1, 0);
                    wordList.RemoveAt(currentWordIndex);
                }
                else
                {
                    string correctAnswer = isGermanToEnglish ? currentWord.Item2 : currentWord.Item1;
                    MessageBox.Show($"Falsch! Die richtige Antwort war: {correctAnswer}", "Falsch", MessageBoxButton.OK, MessageBoxImage.Error);
                    string lessonName = "Unknown";
                    UpdateStatistics(currentWord.German, currentWord.English, false, lessonName);
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
                        string lessonName = "Unknown";
                        UpdateStatistics(currentWord.Item1, currentWord.Item2, true, lessonName);
                        CSVlist wordObject = new CSVlist { de_words = currentWord.Item1, en_words = currentWord.Item2 };
                        myBucket.MoveWord(wordObject, 1, 0);
                        importedWordList.RemoveAt(currentWordIndex);
                    }
                    else
                    {
                        string correctAnswer = isGermanToEnglish ? currentWord.Item2 : currentWord.Item1;
                        MessageBox.Show($"Falsch! Die richtige Antwort war: {correctAnswer}", "Falsch", MessageBoxButton.OK, MessageBoxImage.Error);
                        string lessonName = "Unknown";
                        UpdateStatistics(currentWord.Item1, currentWord.Item2, false, lessonName);
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

        private void AddNewWordButton_Click(object sender, RoutedEventArgs e)
        {
            string newWord = Microsoft.VisualBasic.Interaction.InputBox("Geben Sie das neue deutsche Wort ein:", "Neues Wort hinzufügen", "");
            string newTranslation = Microsoft.VisualBasic.Interaction.InputBox($"Geben Sie die Übersetzung für '{newWord}' ein:", "Übersetzung hinzufügen", "");

            if (!string.IsNullOrWhiteSpace(newWord) && !string.IsNullOrWhiteSpace(newTranslation))
            {
                wordList.Add((newWord, newTranslation));
                MessageBox.Show($"Das Wortpaar '{newWord} - {newTranslation}' wurde hinzugefügt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Das Wort oder die Übersetzung darf nicht leer sein!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void UpdateStatistics(string german, string english, bool correct, string lessonName)
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

            stats.Lesson = lessonName;

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


        private void OpenLanguageSelection_Click(object sender, RoutedEventArgs e)
        {
            LanguageSelectionWindow languageWindow = new LanguageSelectionWindow();
            languageWindow.ShowDialog();
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

            string defaultFilename = $"{Environment.UserName}_{DateTime.Now:yyyy-MM-dd}";
            string inputFilename = Microsoft.VisualBasic.Interaction.InputBox(
                "Geben Sie einen Dateinamen ein (leer lassen für Standardnamen):",
                "Exportdatei speichern",
                defaultFilename
            ).Trim();

            string filename = string.IsNullOrWhiteSpace(inputFilename) ? defaultFilename : inputFilename;

            if (clicked)
            {
                string exportFilePath = Path.Combine(
                    exportClass.UseDefault || string.IsNullOrWhiteSpace(exportClass.UserPath)
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyWordExports")
                        : exportClass.UserPath,
                    $"{filename}.{exportClass.dataextension.ToLower()}"
                );

                if (File.Exists(exportFilePath))
                {
                    File.Delete(exportFilePath);
                    MessageBox.Show($"Datei wurde gelöscht: {exportFilePath}");
                }
                else
                {
                    MessageBox.Show("Keine Datei zum Löschen gefunden.");
                }

                clicked = false;
                return;
            }

            try
            {
                string loaderFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics.json");

                if (!File.Exists(loaderFilePath))
                {
                    MessageBox.Show("Statistics file nicht gefunden in AppData.");
                    return;
                }

                string jsonContent = File.ReadAllText(loaderFilePath);
                var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonContent);

                if (jsonObject == null)
                {
                    MessageBox.Show("Keine daten wurden gefunden unter statistics file.");
                    return;
                }

                string exportDirectory = exportClass.UseDefault || string.IsNullOrWhiteSpace(exportClass.UserPath)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyWordExports")
                    : exportClass.UserPath;

                Directory.CreateDirectory(exportDirectory);

                string fileExtension = exportClass.dataextension.ToLower();
                string exportFilePath = Path.Combine(exportDirectory, $"{filename}.{fileExtension}");

                int fileNumber = 1;
                while (File.Exists(exportFilePath))
                {
                    exportFilePath = Path.Combine(exportDirectory, $"{filename}_{fileNumber}.{fileExtension}");
                    fileNumber++;
                }

                foreach (var innerValue in jsonObject.Values)
                {
                    string lesson = innerValue.ContainsKey("Lesson") ? innerValue["Lesson"]?.ToString() ?? "Unbekannt" : "Unbekannt";
                    string germanWord = innerValue["German"]?.ToString() ?? string.Empty;
                    string englishWord = innerValue["English"]?.ToString() ?? string.Empty;
                    int correctCount = innerValue["CorrectCount"] != null ? (int)innerValue["CorrectCount"] : 0;
                    int incorrectCount = innerValue["IncorrectCount"] != null ? (int)innerValue["IncorrectCount"] : 0;
                    int bucketCount = innerValue["BucketCount"] != null ? (int)innerValue["BucketCount"] : 5;
                    int currentLocation = innerValue["CurrentLocation"] != null ? (int)innerValue["CurrentLocation"] : 0;

                    exportClass.ExporterMethod(
                        lesson: lesson, 
                        word: germanWord,
                        word2: englishWord,
                        one: correctCount,
                        two: incorrectCount,
                        comboboxValue: exportClass.dataextension,
                        filepath: exportFilePath,
                        Userdefined: exportClass.UseDefault,
                        filename: filename,
                        isGermanToEnglish: isGermanToEnglish,
                        bucketCount: bucketCount,
                        currentLocation: currentLocation
                    );
                }

                MessageBox.Show($"{exportClass.dataextension} daten wurden erfolgreich gespeichert unter {exportFilePath}");
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
        public List<(string German, string English)> GetWordList()
        {
            return wordList;
        }

        // Mit dieser Methode kann ein Wortpaar (basierend auf Deutsch und Englisch) aus der Kartei entfernt werden
        public void DeleteWord(string german, string english)
        {
            var item = wordList.FirstOrDefault(w => w.German == german && w.English == english);
            if (!string.IsNullOrEmpty(item.German))
            {
                wordList.Remove(item);
                MessageBox.Show($"Das Wortpaar '{german} - {english}' wurde gelöscht.",
                    "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Wortpaar nicht gefunden.",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event-Handler zum Öffnen des DeleteWordWindow
        private void OpenDeleteWordWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteWordWindow deleteWindow = new DeleteWordWindow(this);
            deleteWindow.ShowDialog();
        }
        private void ResetBuckets_Click(object sender, RoutedEventArgs e)
        {
            // Setzt alle Wortstatistiken so zurück, dass sie in Bucket 3 landen.
            statisticsService.ResetWordBuckets();
            MessageBox.Show("Alle Wörter wurden auf Bucket 3 zurückgesetzt.",
                            "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            // Aktualisiere die Anzeige der Bucket-Übersicht.
            UpdateBucketOverview();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var data = new DataStorage.UserData
            {
                IsGermanToEnglish = isGermanToEnglish,
                WordList = wordList.Select(w => new DataStorage.WordPair { German = w.Item1, English = w.Item2 }).ToList(),
                IncorrectWords = incorrectWords.Select(w => new DataStorage.WordPair { German = w.Item1, English = w.Item2 }).ToList()
            };

            DataStorage.Save(data);
        }
        private void OpenLessonSelection_Click(object sender, RoutedEventArgs e)
        {
            LessonSelectionWindow lessonWindow = new LessonSelectionWindow();
            if (lessonWindow.ShowDialog() == true)
            {
                List<string> selectedLessons = lessonWindow.SelectedLessons;
                LoadWordsFromLessons(selectedLessons);
            }
        }

        // Methode, um die Wörter basierend auf den ausgewählten Lektionen zu laden
        public void LoadWordsFromLessons(List<string> lessons)
        {
            // Bestehende Wortliste leeren
            wordList.Clear();

            // Den Ordner "words" im Ausgabeverzeichnis ermitteln
            string wordsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words");

            // Prüfen, ob der Ordner existiert
            if (!Directory.Exists(wordsFolder))
            {
                MessageBox.Show($"Der Ordner '{wordsFolder}' wurde nicht gefunden. Bitte stelle sicher, dass er in das Ausgabeverzeichnis kopiert wurde.",
                                "Ordner nicht gefunden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Falls "Alle" ausgewählt wurde, lade alle CSV-Dateien
            if (lessons.Contains("Alle"))
            {
                var csvFiles = Directory.GetFiles(wordsFolder, "*.csv");
                foreach (var file in csvFiles)
                {
                    LoadWordsFromFile(file);
                }
            }
            else
            {
                // Ansonsten nur die ausgewählten Lektionen laden
                foreach (var lesson in lessons)
                {
                    string filePath = Path.Combine(wordsFolder, lesson + ".csv");
                    if (File.Exists(filePath))
                    {
                        LoadWordsFromFile(filePath);
                    }
                }
            }

            MessageBox.Show("Die ausgewählten Lektionen wurden geladen.", "Erfolg",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Hilfsmethode zum Laden von Wörtern aus einer CSV-Datei
        private void LoadWordsFromFile(string filePath)
        {
            try
            {
                string lessonName = Path.GetFileNameWithoutExtension(filePath);
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(';');
                    if (parts.Length != 2) continue;
                    //wordList.Add((parts[0].Trim(), parts[1].Trim()));
                    string german = parts[0].Trim();
                    string english = parts[1].Trim();

                    UpdateStatistics(german, english, false, lessonName);

                    wordList.Add((german, english));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Datei {Path.GetFileName(filePath)}: {ex.Message}",
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CopyWords() 
        {
            string words = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../words");
            string sourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, words);
            // Ensure the destination directory exists
            string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words");
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Get all the files in the source directory and copy them to the destination directory
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);  // overwrite true to replace existing files
            }

            // Recursively copy all subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                //CopyDirectory(subDir, destSubDir);  // Recursively copy subdirectories
            }
        }
    }
}
//Test