using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using EasyWordWPF_US5.Models;
using EasyWordWPF;

namespace EasyWordWPF_US5
{
    /// <summary>
    /// Das importieren der daten
    /// </summary>
    public partial class ImportWindow : Window
    {
        private StatisticsService statisticsService;
        public ImportWindow(Buckets bucket)
        {
            InitializeComponent();
            statisticsService = new StatisticsService(bucket);
        }
        private void ClickDragAndDropArea_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV, TXT, JSON files|*.csv;*.txt;*.json",
                Title = "Datei auswählen"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePathLabel.Content = $"Pfad: {openFileDialog.FileName}";
            }
        }

        // 📌 Handle file drag-over event
        private void ClickDragAndDropArea_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsValidFileType(files[0]))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            e.Handled = true;
        }

        // 📌 Handle file drop event
        private void ClickDragAndDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsValidFileType(files[0]))
                {
                    SelectedFilePathLabel.Content = $"Pfad: {files[0]}";
                }
            }
        }

        // 📌 Check if the file has a valid extension
        private bool IsValidFileType(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            return extension == ".csv" || extension == ".txt" || extension == ".json";
        }
        private void OnImportCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnOkay_Click(object sender, RoutedEventArgs e)
        {
            string text = SelectedFilePathLabel.Content.ToString();
            if (text.StartsWith("Pfad: "))
            {
                string path = text.Substring(6); // Removes the first 6 characters ("Pfad: ")
                Debug.WriteLine(path); // Debugging output

                // Check if the import file already exists
                string importFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imported_data.json");

                if (File.Exists(importFilePath))
                {
                    File.Delete(importFilePath);
                    Debug.WriteLine("Alte 'imported_data.json' gelöscht.");
                }

                statisticsService.ImportData(path);
            }
        }

    }
}
