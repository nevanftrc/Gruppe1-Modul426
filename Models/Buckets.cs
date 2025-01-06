using System;
using System.Collections.Generic;

namespace EasyWordWPF
{
    public class Buckets
    {
        public int bucket_count => buckets.Count; // Number of buckets
        public List<List<CSVlist>> buckets { get; private set; } // Buckets storing words

        public Buckets(int initialBucketCount = 3) // Default 3 buckets
        {
            buckets = new List<List<CSVlist>>();
            for (int i = 0; i < initialBucketCount; i++)
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
            for (int i = 0; i < buckets.Count; i++)
            {
                totalWeight += buckets[i].Count * (buckets.Count - i); // Higher weight for leftmost buckets
            }

            if (totalWeight == 0)
                throw new Exception("No words available.");

            Random random = new Random();
            int randomValue = random.Next(0, totalWeight);
            int cumulativeWeight = 0;

            for (int i = 0; i < buckets.Count; i++)
            {
                cumulativeWeight += buckets[i].Count * (buckets.Count - i);
                if (randomValue < cumulativeWeight && buckets[i].Count > 0)
                {
                    int index = random.Next(0, buckets[i].Count);
                    return buckets[i][index];
                }
            }

            return null; // Should never reach here
        }

        // Move a word between buckets
        public void MoveWord(CSVlist word, int sourceBucket, int targetBucket)
        {
            if (sourceBucket < 0 || sourceBucket >= bucket_count || targetBucket < 0 || targetBucket >= bucket_count)
                throw new ArgumentOutOfRangeException("Invalid bucket index.");

            if (buckets[sourceBucket].Remove(word))
            {
                buckets[targetBucket].Add(word);
            }
        }

        // Add buckets
        public void bucket_add(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0.");

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

    }
}
