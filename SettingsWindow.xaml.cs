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

namespace EasyWordWPF_US5
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;

        public SettingsWindow(MainWindow mainWindowInstance)
        {
            InitializeComponent();
            mainWindow = mainWindowInstance;

            // Setze den aktuellen Zustand der Checkbox basierend auf dem Status im MainWindow
            CheckGrammar.IsChecked = !mainWindow.isCaseSensitive; // Wenn isCaseSensitive true, wird die Checkbox deaktiviert (beachtet Groß/Kleinschreibung)
        }

        // Event-Handler für den Apply-Button
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Ändere den Zustand von Groß-/Kleinschreibung basierend auf dem Wert der Checkbox
            mainWindow.isCaseSensitive = !(CheckGrammar.IsChecked ?? true); // Wenn angekreuzt, ignoriert es die Groß-/Kleinschreibung

            MessageBox.Show("Einstellungen angewendet!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Event-Handler für den Close-Button
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
