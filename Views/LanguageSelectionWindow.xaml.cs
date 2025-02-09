using System.Windows;
using System.Windows.Controls;

namespace EasyWordWPF_US5.Views
{
    public partial class LanguageSelectionWindow : Window
    {
        public LanguageSelectionWindow()
        {
            InitializeComponent();
        }

        private void ApplyLanguage_Click(object sender, RoutedEventArgs e)
        {
            string selectedLanguage = (LanguageComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedLanguage))
            {
                MessageBox.Show($"Sprache geändert zu: {selectedLanguage}", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Bitte eine Sprache auswählen!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
