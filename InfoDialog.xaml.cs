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

using System.Windows;

namespace EasyWordWPF_US5
{
    public partial class InfoDialog : Window
    {
        // Diese Eigenschaft bindet das aktuelle Datum
        public string CurrentDate { get; set; }

        public InfoDialog()
        {
            InitializeComponent();
            CurrentDate = DateTime.Now.ToString("dd.MM.yyyy");  // Setze das heutige Datum
            DataContext = this;  // Setze den DataContext für das Binding
        }
    }
}
