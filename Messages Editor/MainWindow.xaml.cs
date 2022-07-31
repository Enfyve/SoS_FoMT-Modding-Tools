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
using System.IO;
using System.Collections.ObjectModel;

namespace Messages_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MessageParser parser;
        string filePath;
        bool fileLoaded = false;
        List<ListViewItem> messages = new List<ListViewItem>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TryOpenFile();
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TrySaveFile();
        }

        private void ParseMessages(string filePath)
        {
            List<string> entries = parser.LoadEntries();
            messages.Clear();
            
            foreach (var entry in entries)
            {
                ListViewItem message = new ListViewItem();
                message.Content = entry;
                messages.Add(message);
            }

            messageStrings.ItemsSource = null;
            messageStrings.Items.Clear();
            messageStrings.ItemsSource = messages;
            messageStrings.Items.Refresh();
            messageStrings.SelectedIndex = 0;

            fileLoaded = true;
            saveFile.IsEnabled = fileLoaded;

            openedFileName.Content = $"{Path.GetFileName(filePath)} - {messageStrings.Items.Count} strings";
        }

        private void TrySaveFile()
        {
            if (fileLoaded)
            {
                if (parser.Export(messages.ConvertAll<String>((x) => x.Content.ToString()), createBackup.IsChecked))
                    modifiedState.Content = "";
                else
                    MessageBox.Show("Could not save file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TryOpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "";
            dialog.DefaultExt = ".bin";
            dialog.Filter = "FoMT Strings (.bin)|*.bin";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                filePath = dialog.FileName;
                parser = new MessageParser(filePath);
                ParseMessages(dialog.FileName);
            }
        }




        private void Window_Drop(object sender, DragEventArgs e)
        {
            filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            parser = new MessageParser(filePath);
            ParseMessages(filePath);
        }

        private void messageStrings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            messageBox.Text = ((sender as ListView).SelectedItem as ListViewItem)?.Content.ToString();
        }

        private void messageBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            messageStrings.SelectedItem = messageBox.Text;
            if (messageStrings.SelectedIndex >= 0)
            {
                messages[messageStrings.SelectedIndex].Content = messageBox.Text;
                messageStrings.Items.Refresh();
                modifiedState.Content = "(modified)";
            }

            if (messagePreview != null)
                messagePreview.Text = messageBox.Text;
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            TryOpenFile();
        }

        private void saveFile_Click(object sender, RoutedEventArgs e)
        {
            TrySaveFile();
        }
    }
}
