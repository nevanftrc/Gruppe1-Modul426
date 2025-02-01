using EasyWordWPF_US5;
using EasyWordWPF_US5.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;
namespace EasyWordWPF
{
    /// <summary>
    /// Diese Klasse Behält die menge und den zufalls generator von wörter in der wort liste
    /// </summary>
    public class Buckets
    {
        public int bucket_count => buckets.Count; // Number of buckets
        public List<List<CSVlist>> buckets { get; private set; } // Buckets storing words

        public Buckets(int initialBucketCount = -1) // Use -1 as a flag
        {
            buckets = new List<List<CSVlist>>();
            
            // Read saved bucket count from settings
            ExportClass exportClass = new ExportClass();
            exportClass.ReadSettings();

            // If no valid value is provided, use the saved value or fallback to default (3)
            int bucketCount = (initialBucketCount > 0) ? initialBucketCount : exportClass.Buckets;
            if (bucketCount <= 0) bucketCount = 5; // Final fallback

            for (int i = 0; i < bucketCount; i++)
            {
                buckets.Add(new List<CSVlist>());
            }
        }
        // Fill the first bucket with words from a file
        /// <summary>
        /// Füllt den Eimer
        /// </summary>
        /// <param name="filepath"></param>
        /// <exception cref="Exception"></exception>
        public void fill_bucket(string filepath)
        {
            List<CSVlist> words = CSVlist.insert_data(filepath); // Load words from file
            if (buckets.Count > 0)
            {
                buckets[0].AddRange(words); // Start by putting all words in the first bucket
            }
            else
            {
                throw new Exception("No buckets available.");
            }
        }

        // Retrieve a random word with weighted priority (higher weight for earlier buckets)
        /// <summary>
        /// randomizer für werte der list teile
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public CSVlist GetWeightedRandomWord()
        {
            // Ensure bucket count is odd
            if (buckets.Count % 2 == 0)
            {
                MessageBox.Show("Gerade Anzahl von Buckets ist nicht erlaubt! Bitte verwenden Sie eine ungerade Anzahl.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            int totalWeight = 0;
            Dictionary<int, int> bucketWeights = new Dictionary<int, int>();

            for (int i = 0; i < buckets.Count; i++)
            {
                // Higher weight for earlier (harder) buckets
                int weight = (buckets.Count - i) * (buckets[i].Count + 1);
                bucketWeights[i] = weight;
                totalWeight += weight;
            }

            if (totalWeight == 0)
                throw new Exception("No words available.");

            Random random = new Random();
            int randomValue = random.Next(0, totalWeight);
            int cumulativeWeight = 0;

            for (int i = 0; i < buckets.Count; i++)
            {
                cumulativeWeight += bucketWeights[i];

                if (randomValue < cumulativeWeight && buckets[i].Count > 0)
                {
                    return buckets[i][random.Next(buckets[i].Count)];
                }
            }

            return null;
        }
        // Move a word between buckets based on stats
        /// <summary>
        /// bewegt die werte
        /// </summary>
        /// <param name="word">liste</param>
        /// <param name="correctCount">menge</param>
        /// <param name="incorrectCount">menge</param>
        public void MoveWord(CSVlist word, int correctCount, int incorrectCount)
        {
            // Ensure bucket count is odd
            if (buckets.Count % 2 == 0)
            {
                MessageBox.Show("Gerade Anzahl von Buckets ist nicht erlaubt! Bitte verwenden Sie eine ungerade Anzahl.",
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int sourceBucket = GetBucketForWord(word.de_words, word.en_words);

            // If the word has no stats, place it in the middle bucket
            if (sourceBucket == -1)
            {
                sourceBucket = buckets.Count / 2; // Middle index
            }

            // Calculate movement based on mistakes/correct answers
            int movement = incorrectCount - correctCount;

            // Ensure the movement stays within the valid range (0 to max bucket index)
            int targetBucket = Math.Clamp(sourceBucket - movement, 0, buckets.Count - 1);

            Debug.Write($"\nTest: {targetBucket} bewegt: {movement} Berechnung: {sourceBucket}: {word.de_words} und {word.en_words}\n");

            // Remove from the old bucket if it exists there
            if (buckets[sourceBucket].Contains(word))
            {
                buckets[sourceBucket].Remove(word);
            }

            // Add to the new bucket
            if (!buckets[targetBucket].Contains(word))
            {
                buckets[targetBucket].Add(word);
            }
        }
        /// <summary>
        /// Finds the bucket index for a given word.
        /// </summary>
        public int GetBucketForWord(string german, string english)
        {
            for (int i = 0; i < buckets.Count; i++)
            {
                if (buckets[i].Any(w => w.de_words == german && w.en_words == english))
                    return i; // Return the index of the bucket containing the word
            }
            return -1; // Word not found in any bucket
        }
        /// <summary>
        /// Selects a random word from all eligible buckets (excludes highest-level bucket).
        /// </summary>
        public CSVlist GetRandomWord()
        {
            List<int> validBuckets = new List<int>();

            // Collect all buckets except the highest-level one
            for (int i = 0; i < buckets.Count - 1; i++)
            {
                if (buckets[i].Count > 0)
                {
                    validBuckets.Add(i);
                }
            }

            if (validBuckets.Count == 0)
            {
                MessageBox.Show("Kein gültiges Wort zum Üben verfügbar!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return null; // No word found
            }

            // Pick a random bucket
            int selectedBucket = validBuckets[new Random().Next(validBuckets.Count)];

            // Pick a random word from that bucket
            return buckets[selectedBucket][new Random().Next(buckets[selectedBucket].Count)];
        }

        // Add buckets
        public void bucket_add(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Anzahl muss grösser als 0 sein.");

            for (int i = 0; i < count; i++)
            {
                buckets.Add(new List<CSVlist>());
            }
        }

        // Remove buckets
        public void bucket_remove(int count)
        {
            if (count <= 0 || count > bucket_count)
                throw new ArgumentException("Invalid count for bucket removal.");

            for (int i = 0; i < count; i++)
            {
                if (buckets[^1].Count > 0) // Check if last bucket has words
                    throw new Exception("Cannot remove a bucket that contains words.");

                buckets.RemoveAt(buckets.Count - 1);
            }
        }

        // Save the buckets to JSON
        public void SaveBucketsToJson()
        {
            // Define the file path relative to the application's runtime directory
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics.json");

            // Serialize the buckets data to JSON
            var jsonData = JsonConvert.SerializeObject(buckets, Formatting.Indented);

            // Write the JSON data to the file
            File.WriteAllText(filePath, jsonData);

            // Optional: Inform the user that the file was saved
            Debug.WriteLine($"Buckets data saved to: {filePath}");
        }


        // Load buckets from JSON
        public void LoadBucketsFromJson(string json)
        {
            var loadedBuckets = JsonConvert.DeserializeObject<List<List<CSVlist>>>(json);

            if (loadedBuckets == null || loadedBuckets.Count != bucket_count)
                throw new Exception("Invalid bucket data.");

            buckets = loadedBuckets;
        }
    }
}
