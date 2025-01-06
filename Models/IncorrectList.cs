using EasyWordWPF_US5.Models;

namespace EasyWordWPF.Model
{
    public class IncorrectList
    {
        private List<CSVlist> incorrectWords; // List to store incorrectly answered words/groups

        public IncorrectList()
        {
            incorrectWords = new List<CSVlist>();
        }

        // Add a word group to the list
        public void AddToIncorrect(CSVlist wordGroup)
        {
            if (!incorrectWords.Contains(wordGroup))
            {
                incorrectWords.Add(wordGroup);
            }
        }

        // Remove a word group from the list
        public void RemoveFromIncorrect(CSVlist wordGroup)
        {
            incorrectWords.Remove(wordGroup);
        }

        // Get the list of incorrectly answered words/groups
        public List<CSVlist> GetIncorrectWords()
        {
            return new List<CSVlist>(incorrectWords); // Return a copy of the list
        }

        // Check if the list contains a specific word group
        public bool Contains(CSVlist wordGroup)
        {
            return incorrectWords.Contains(wordGroup);
        }

        // Get the total count of incorrect words/groups
        public int Count => incorrectWords.Count;

        // Clear all incorrect words
        public void Clear()
        {
            incorrectWords.Clear();
        }
    }
}
