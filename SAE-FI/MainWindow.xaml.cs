using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SAE_FI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HelloButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hello there!");
        }
        private void BtnAuswerten_Click(object sender, RoutedEventArgs e)
        {
            //Zugriff auf Datei erstellen.

            //Anfangswert setzen, um sinnvoll vergleichen zu können.


            //In einer Schleife die Werte holen und auswerten. Den größten Wert "merken".


            //Datei wieder freigeben.


            //Höchstwert auf Oberfläche ausgeben.

            MessageBox.Show("Gleich kachelt das Programm...");
            //kommentieren Sie die Exception aus.
            //throw new Exception("peng");
        }
    }
}