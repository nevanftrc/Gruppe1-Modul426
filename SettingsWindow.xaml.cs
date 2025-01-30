using EasyWordWPF_US5.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace EasyWordWPF_US5
{
    /// <summary>
    /// Settings window
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;
        private readonly ExportandImportClass exportClass;

        public bool check_value = false;

        public SettingsWindow(MainWindow mainWindowInstance)
        {
            InitializeComponent();
            mainWindow = mainWindowInstance;

            // Setze den aktuellen Zustand der Checkbox basierend auf dem Status im MainWindow
            CheckGrammar.IsChecked = !mainWindow.isCaseSensitive; // Wenn isCaseSensitive true, wird die Checkbox deaktiviert (beachtet Groß/Kleinschreibung)

            // export settings :)
            exportClass = new ExportandImportClass();
            //path display
            LoadUserDefinedPath();

            // checker
            exportpathcheck.IsChecked = false;

            //checker eimer

            UserDefinedBucket.IsChecked = false;

            DataContext = exportClass;
            //combobox
            LoadComboBoxFromSettings();

        }

        // Event-Handler für den Apply-Button
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Ändere den Zustand von Groß-/Kleinschreibung basierend auf dem Wert der Checkbox
            mainWindow.isCaseSensitive = !(CheckGrammar.IsChecked ?? true); // Wenn angekreuzt, ignoriert es die Groß-/Kleinschreibung
            ComboBoxItem selectedItem = dataextension.SelectedItem as ComboBoxItem;
            string selectedContent = selectedItem?.Content?.ToString() ?? string.Empty;
            int bucketcount = 0;
            if (int.TryParse(mainWindow.numtxtbo.Content?.ToString(), out int parsedValue))
            {
                bucketcount = parsedValue;
            }


            if (exportpathcheck.IsChecked == true)
            {
                // When checked, apply the user-defined path
                if (UserDefinedBucket.IsChecked == false)
                {
                    exportClass.UpdateSettings(false, userdefinedpathbox.Text, selectedContent, bucketcount, false);
                }
                else 
                {
                    exportClass.UpdateSettings(false, userdefinedpathbox.Text, selectedContent, bucketcount, true);
                }
            }
            else
            {
                if (UserDefinedBucket.IsChecked == false)
                {
                    // If unchecked, reset to the default or empty path
                    exportClass.UpdateSettings(true, string.Empty, selectedContent, bucketcount, false);
                }
                else 
                {
                    exportClass.UpdateSettings(true, string.Empty, selectedContent, bucketcount, true);
                }
            }


            MessageBox.Show("Einstellungen angewendet!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        // Event-Handler für den Close-Button
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //export settings
        private void ExportPathCheck_Checked(object sender, RoutedEventArgs e)
        {
            userdefinedpathbox.IsEnabled = true;
        }

        private void ExportPathCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            userdefinedpathbox.IsEnabled = false;
        }

        // checkerbox
        public void checkcheckboxsettings(bool item)
        {
            if (item == true)
            {
                exportpathcheck.IsChecked = false;
            }
            else
            {
                exportpathcheck.IsChecked = true;
            }
        }
        //checkbox eimer
        public void checkcheckboxeimer(bool item)
        {
            if (item == false)
            {
                UserDefinedBucket.IsChecked = false;
            }
            else
            {
                UserDefinedBucket.IsChecked = true;
            }
        }
        //text box
        private void LoadUserDefinedPath()
        {
            exportClass.ReadSettings();

            if (exportClass != null)
            {
                userdefinedpathbox.Text = !string.IsNullOrWhiteSpace(exportClass.UserPath)
                    ? exportClass.UserPath
                    : "Kein Pfad Vorhanden";

                // Reset the checkbox state
                exportpathcheck.IsChecked = false;
            }
        }


        public void LoadComboBoxFromSettings()
        {
            // Read the value from the JSON
            exportClass.ReadSettings();

            // Get the value from the JSON
            string savedExtension = exportClass.dataextension;

            // Iterate over ComboBox items to find the matching one
            foreach (var item in dataextension.Items)
            {
                if (item is ComboBoxItem comboBoxItem)
                {
                    if (comboBoxItem.Content.ToString().Equals(savedExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        // Match found, select it
                        dataextension.SelectedItem = comboBoxItem;
                        return;
                    }
                }
            }

            // If no match is found, reset to the default
            dataextension.SelectedIndex = 0; // Optional: set to first item if no match
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            HelpBox.Visibility = Visibility.Visible;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            HelpBox.Visibility = Visibility.Hidden;
        }
    }
}
