using EasyWordWPF.Model;
using EasyWordWPF_US5.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyWordWPF
{
    public partial class Wordlearner : Window
    {
        private Buckets myBucket;
        private CSVlist currentWord;
        private Random random;
        private int incorrectAttempts;
        private IncorrectList incorrectList;

        public Wordlearner(Buckets bucket)
        {
            InitializeComponent();
            myBucket = bucket;
            random = new Random();
            incorrectList = new IncorrectList();
            ShowNextWord();
        }

        // Show the next random word
        private void ShowNextWord()
        {
            try
            {
                currentWord = myBucket.GetWeightedRandomWord();

                // Update the English word
                english.Content = currentWord.en_words;

                // Clear previous input and feedback
                de_input.Clear();
                result.Content = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Check user's input
        private void check(object sender, RoutedEventArgs e)
        {
            if (currentWord == null)
            {
                MessageBox.Show("Kein Wort zum Überprüfen.", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // User input
            string userInput = de_input.Text.Trim().ToLower();

            // Correct answers parsed from the de_words string (split by commas and trimmed)
            var correctAnswers = currentWord.de_words
                                            .Split(',')
                                            .Select(word => word.Trim().ToLower())
                                            .ToList();

            // Determine the current bucket of the word
            int currentBucket = -1;
            for (int i = 0; i < myBucket.bucket_count; i++)
            {
                if (myBucket.buckets[i].Contains(currentWord))
                {
                    currentBucket = i;
                    break;
                }
            }

            if (currentBucket == -1)
            {
                MessageBox.Show("Das aktuelle Wort wurde in keinem Eimer gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if the input matches any of the correct answers
            if (correctAnswers.Any(answer => string.Equals(userInput, answer, StringComparison.OrdinalIgnoreCase)))
            {
                result.Content = "Richtig!";
                result.Foreground = Brushes.Green;

                // Move to the next bucket if correct
                if (currentBucket < myBucket.bucket_count - 1)
                {
                    myBucket.MoveWord(currentWord, 1,0);
                }

                incorrectList.RemoveFromIncorrect(currentWord); // Remove from incorrect list
                incorrectAttempts = 0;
                ShowNextWord();
            }
            else
            {
                result.Content = $"Falsch! Korrekt: {string.Join(", ", correctAnswers)}";
                result.Foreground = Brushes.Red;
                incorrectAttempts++;

                // Move to the previous bucket if incorrect
                if (currentBucket > 0)
                {
                    myBucket.MoveWord(currentWord, 0,1);
                }

                incorrectList.AddToIncorrect(currentWord); // Add to incorrect list

                if (incorrectAttempts == 2)
                {
                    result.Content += "\nDer nächste Versuch...";
                    incorrectAttempts = 0;
                    ShowNextWord();
                }
            }
        }


        // Handle Enter key to trigger the check button
        private void DeInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // Check if Enter key is pressed
            {
                // Simulate button click for checkerbtn
                if (checkerbtn != null && checkerbtn.IsEnabled)
                {
                    checkerbtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); // Simulate button click
                }

                // Prevent default Enter key behavior
                e.Handled = true;
            }
        }
    }
}
