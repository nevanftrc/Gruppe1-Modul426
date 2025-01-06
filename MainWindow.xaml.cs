using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace EasyWordWPF_US5
{
    public partial class MainWindow : Window
    {
        private List<(string German, string English)> wordList = new List<(string, string)>();
        private List<(string, string)> incorrectWords = new List<(string, string)>();
        private Random random = new Random();
        private int currentWordIndex = -1;
        private bool isGermanToEnglish = true; // Default mode is D->E Test

        //test
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    List<(string, string)> tempList = new List<(string, string)>();

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue; // Ignore empty lines

                        var parts = line.Split(';');
                        if (parts.Length != 2)
                        {
                            MessageBox.Show($"Fehlerhafte Zeile: {line}", "Importfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue; // Skip invalid lines
                        }
                        tempList.Add((parts[0].Trim(), parts[1].Trim()));
                    }

                    wordList = tempList;
                    incorrectWords.Clear();
                    MessageBox.Show("Wörterliste erfolgreich importiert!", "Import abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("Bitte importieren Sie zuerst eine Wörterliste.", "Keine Wörter vorhanden", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    ? input.Trim().Equals(currentWord.Item2, StringComparison.OrdinalIgnoreCase)
                    : input.Trim().Equals(currentWord.Item1, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    MessageBox.Show("Korrekt!", "Richtig", MessageBoxButton.OK, MessageBoxImage.Information);
                    wordList.RemoveAt(currentWordIndex);
                }
                else
                {
                    string correctAnswer = isGermanToEnglish ? currentWord.Item2 : currentWord.Item1;
                    MessageBox.Show($"Falsch! Die richtige Antwort war: {correctAnswer}", "Falsch", MessageBoxButton.OK, MessageBoxImage.Error);
                    incorrectWords.Add(currentWord); // Add to incorrect list
                }
            }

            // Handle next iteration with incorrect words
            if (incorrectWords.Any())
            {
                wordList = new List<(string, string)>(incorrectWords);
                incorrectWords.Clear();
                MessageBox.Show("Ein neuer Durchlauf mit falsch beantworteten Wörtern beginnt.", "Nächster Durchlauf", MessageBoxButton.OK, MessageBoxImage.Information);
                StartQuizLoop();
            }
            else
            {
                MessageBox.Show("Alle Wörter wurden erfolgreich beantwortet!", "Quiz abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SwitchModeButton_Click(object sender, RoutedEventArgs e)
        {
            isGermanToEnglish = !isGermanToEnglish;
            string mode = isGermanToEnglish ? "Deutsch -> Englisch" : "Englisch -> Deutsch";
            MessageBox.Show($"Modus geändert: {mode}", "Moduswechsel", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
