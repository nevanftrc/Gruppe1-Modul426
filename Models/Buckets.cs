using EasyWordWPF_US5;
using EasyWordWPF_US5.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace EasyWordWPF
{
    /// <summary>
    /// Diese Klasse Behält die menge und den zufalls generator von wörter in der wort liste
    /// </summary>
    public class Buckets
    {
        public MainWindow Menu { get; set; }
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
            if (bucketCount <= 0) bucketCount = 3; // Final fallback

            for (int i = 0; i < bucketCount; i++)
            {
                buckets.Add(new List<CSVlist>());
            }
        }
        // Fill the first bucket with words from a file
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
        public CSVlist GetWeightedRandomWord()
        {
            int totalWeight = 0;
            Dictionary<int, int> bucketWeights = new Dictionary<int, int>();

            for (int i = 0; i < buckets.Count; i++)
            {
                int weight = (buckets.Count - i) * (buckets[i].Count + 1); // Higher weight for earlier buckets
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

            return null; // Should never reach here
        }

        // Move a word between buckets based on mistakes
        public void MoveWord(CSVlist word, int sourceBucket, int correctCount, int incorrectCount)
        {
            if (sourceBucket < 0 || sourceBucket >= bucket_count)
                throw new ArgumentOutOfRangeException("Invalid bucket index.");

            if (!buckets[sourceBucket].Remove(word))
                return; // Word not found, exit

            // Calculate movement using mistakes/correct answers
            int movement = incorrectCount - correctCount;

            // Negative movement moves the word forward, positive moves it back
            int targetBucket = Math.Max(0, Math.Min(bucket_count - 1, sourceBucket - movement));

            buckets[targetBucket].Add(word);
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
