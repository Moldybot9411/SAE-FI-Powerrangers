
using System.Windows;

using Microsoft.Win32;


namespace SAE_FI.Services
{
    public class Filepicker
    {
        private void openFilepicker(object sender, RoutedEventArgs e)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Alle Dateien (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    MessageBox.Show(openFileDialog.FileName);
                }
            }
    }
}
