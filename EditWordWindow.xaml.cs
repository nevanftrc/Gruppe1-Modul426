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
using System.Windows.Navigation;
using EasyWordWPF_US5.Models;

namespace EasyWordWPF_US5
{
    public partial class EditWordWindow : Window
    {
        public string GermanWord { get; private set; }
        public string EnglishWord { get; private set; }

        public EditWordWindow(string german, string english)
        {
            InitializeComponent();
            GermanWordTextBox.Text = german;
            EnglishWordTextBox.Text = english;
        }

        private void SaveEditedWord_Click(object sender, RoutedEventArgs e)
        {
            GermanWord = GermanWordTextBox.Text.Trim();
            EnglishWord = EnglishWordTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(GermanWord) || string.IsNullOrWhiteSpace(EnglishWord))
            {
                MessageBox.Show("Beide Felder müssen ausgefüllt sein!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
