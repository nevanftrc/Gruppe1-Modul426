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
    /// Interaction logic for SettingsWindow.xaml
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;
        private readonly ExportClass exportClass;

        public bool check_value = false;

        public SettingsWindow(MainWindow mainWindowInstance)
        {
            InitializeComponent();
            mainWindow = mainWindowInstance;

            // Setze den aktuellen Zustand der Checkbox basierend auf dem Status im MainWindow
            CheckGrammar.IsChecked = !mainWindow.isCaseSensitive; // Wenn isCaseSensitive true, wird die Checkbox deaktiviert (beachtet Groß/Kleinschreibung)

            // export settings :)
            exportClass = new ExportClass();
            //path display
            userdefinedpathbox.Text = !string.IsNullOrWhiteSpace(exportClass.UserPath ?? string.Empty)
            ? exportClass.UserPath
            : "Kein Pfad Vorhanden";

            // checker
            exportpathcheck.IsChecked = false;

        }

        // Event-Handler für den Apply-Button
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Ändere den Zustand von Groß-/Kleinschreibung basierend auf dem Wert der Checkbox
            mainWindow.isCaseSensitive = !(CheckGrammar.IsChecked ?? true); // Wenn angekreuzt, ignoriert es die Groß-/Kleinschreibung
            Label selectedLabel = (Label)dataextension.SelectedItem;
            string labelContent = selectedLabel?.Content?.ToString() ?? string.Empty;


            if (exportpathcheck.IsChecked == true)
            {
                // When checked, apply the user-defined path
                exportClass.UpdateSettings(false, userdefinedpathbox.Text, labelContent);
            }
            else
            {
                // If unchecked, reset to the default or empty path
                exportClass.UpdateSettings(true, string.Empty, labelContent);
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

    }

}
