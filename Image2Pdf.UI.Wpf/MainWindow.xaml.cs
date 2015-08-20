using Image2Pdf.Core;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Image2Pdf.UI.Wpf
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

        private void selectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                dlg.FileNames.ToList().ForEach(filename =>
                {
                    var newItem = new ListBoxItem()
                    {
                        Content = System.IO.Path.GetFileName(filename),
                        Tag = filename
                    };

                    fileListBox.Items.Add(newItem);
                });
            }
        }

        private async void convertButton_Click(object sender, RoutedEventArgs e)
        {
            var sourceFileList = fileListBox.Items.Cast<ListBoxItem>()
                .Select(lbi => lbi.Tag.ToString())
                .ToList();

            progressBar.Visibility = Visibility.Visible;
            progressBar.Maximum = sourceFileList.Count;

            var progress = new Progress<TaskProgress>(prog =>
            {
                textBox.Text = prog.StatusMessage;
                progressBar.Value = prog.ProcessedInputCount;
            });

            var converter = new ImageToPdfConverter();
            var outputFilePath = outputFileNameTextBox.Text;
            await Task.Run(() => converter.ConvertImagesToPdf(sourceFileList, outputFilePath, progress));
        }

        private void selectOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string currentFileName = System.IO.Path.GetFileName(outputFileNameTextBox.Text);
                outputFileNameTextBox.Text = $@"{dialog.SelectedPath}\{currentFileName}";
            }
        }

        private void deleteFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (fileListBox.SelectedItem == null)
            {
                return;
            }

            fileListBox.Items.Remove(fileListBox.SelectedItem);
        }

        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(sender);
        }

        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(sender);
        }

        private void MoveItem(object sender)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button == null)
            {
                return;
            }

            var selectedItem = fileListBox.SelectedItem;
            var selectedIndex = fileListBox.SelectedIndex;
            fileListBox.Items.RemoveAt(selectedIndex);

            if (button.Content.ToString().ToLower() == "up")
            {
                fileListBox.Items.Insert(selectedIndex - 1, selectedItem);
            }
            else if (button.Content.ToString().ToLower() == "down")
            {
                fileListBox.Items.Insert(selectedIndex + 1, selectedItem);
            }

            fileListBox.SelectedItem = selectedItem;
        }

        private void fileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fileListBox.SelectedItems.Count == 0)
            {
                moveUpButton.IsEnabled = false;
                moveDownButton.IsEnabled = false;
                deleteFilesButton.IsEnabled = false;
            }
            else
            {
                moveUpButton.IsEnabled = true;
                moveDownButton.IsEnabled = true;
                deleteFilesButton.IsEnabled = true;
            }

            moveUpButton.IsEnabled = !(fileListBox.SelectedIndex == 0);
            moveDownButton.IsEnabled = !(fileListBox.SelectedIndex == fileListBox.Items.Count - 1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitList();
        }

        private void InitList()
        {
            fileListBox.Items.Clear();

            foreach (var filename in Directory.GetFiles(@"C:\Temp\Test").Take(5))
            {
                fileListBox.Items.Add(new ListBoxItem()
                {
                    Content = System.IO.Path.GetFileName(filename),
                    Tag = filename
                });
            }
        }

        private void openPdfButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(outputFileNameTextBox.Text);
        }
    }
}
