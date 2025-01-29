using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyWordWPF_US5.Models
{
    /// <summary>
    /// Diese Klasse gibt den standart von wörter und die liste
    /// </summary>
    public class CSVlist
    {
        public string en_words { get; set; }
        public string de_words { get; set; }
        public int CorrectCount { get; set; } = 0;
        public int IncorrectCount { get; set; } = 0;

        //add here more
        private string filestring { get; set; }

        public CSVlist()
        {
            en_words = string.Empty;
            de_words = string.Empty;
            filestring = string.Empty;
        }
        public CSVlist(string deutsche_woerter, string englische_woerter)
        {
            en_words = englische_woerter;
            de_words = deutsche_woerter;
        }
        public static List<CSVlist> insert_data(string filePath)
        {
            List<CSVlist> data = new List<CSVlist>();

            try
            {
                // Read all lines from the CSV file
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    // Skip empty or invalid lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Split the line by ';' delimiter
                    var parts = line.Split(';');

                    if (parts.Length == 2)
                    {
                        // Add to the list if the line is valid
                        data.Add(new CSVlist(parts[0].Trim(), parts[1].Trim()));
                    }
                    else
                    {
                        Console.WriteLine($"Skipped invalid line: {line}");
                    }
                }

                Console.WriteLine("CSV data successfully inserted!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            return data;
        }

    }
}
