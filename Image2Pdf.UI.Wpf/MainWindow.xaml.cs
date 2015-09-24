using Image2Pdf.Core;
using Image2Pdf.Core.InputFileHanding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Syncfusion.Windows.Tools.Controls;

namespace Image2Pdf.UI.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ListBoxItem> ImageFileCollection { get; set; } = new ObservableCollection<ListBoxItem>();
        private IInputFileHandlingStrategy _inputFileHandlingStrategy = null;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void wizard_Next(object sender, RoutedEventArgs e)
        {
            ValidateInputPage();
        }

        private void wizard_Finish(object sender, RoutedEventArgs e)
        {

        }

        private void wizard_Cancel(object sender, RoutedEventArgs e)
        {

        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio.Tag == null)
            {
                return;
            }

            int selectedValue = (int)radio.Tag;
            switch (selectedValue)
            {
                case 0: _inputFileHandlingStrategy = null; break;
                case 1: _inputFileHandlingStrategy = new MoveInputFilesToRecyclebinStrategy(); break;
                case 2: _inputFileHandlingStrategy = new DeleteInputFilesStrategy(); break;
                case 3: _inputFileHandlingStrategy = null; break;
                case 4: _inputFileHandlingStrategy = null; break;
            }
        }

        private void selectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
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

                    ImageFileCollection.Add(newItem);
                });

                CheckDuplicates();

                ValidateInputPage();
            }
        }

        private void CheckDuplicates()
        {
            if (removeDuplicatesCheckbox.IsChecked.Value)
            {
                var uniquePaths = ImageFileCollection.GroupBy(i => i.Tag)
                    .Select(g => new ListBoxItem() { Tag = g.Key, Content = System.IO.Path.GetFileName(g.Key.ToString()) })
                    .Distinct()
                    .Cast<ListBoxItem>()
                    .ToList();

                if (uniquePaths.Count < ImageFileCollection.Count)
                {
                    ImageFileCollection.Clear();
                    uniquePaths.ForEach(i => ImageFileCollection.Add(i));
                }
            }
        }

        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = fileListBox.SelectedItems.Cast<ListBoxItem>().ToList();
            if (CanMoveDown(selectedItems))
            {
                MoveDown(selectedItems);
            }
        }

        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = fileListBox.SelectedItems.Cast<ListBoxItem>().ToList();
            if (CanMoveUp(selectedItems))
            {
                MoveUp(selectedItems);
            }
        }

        private void MoveUp(List<ListBoxItem> selectedItems)
        {
            if (selectedItems.Count == 0) { return; }

            var indexList = selectedItems
                .Select(s => ImageFileCollection.IndexOf(s))
                .OrderBy(n => n)
                .ToList();

            foreach (var index in indexList)
            {
                ImageFileCollection.Move(index, index - 1);
            }

            UpdateButtonStatus();
        }

        private void MoveDown(List<ListBoxItem> selectedItems)
        {
            if (selectedItems.Count == 0) { return; }

            var indexList = selectedItems
                .Select(s => ImageFileCollection.IndexOf(s))
                .OrderByDescending(n => n)
                .ToList();

            foreach (var index in indexList)
            {
                ImageFileCollection.Move(index, index + 1);
            }

            UpdateButtonStatus();
        }

        private bool CanMoveUp(List<ListBoxItem> selectedItems)
        {
            if (selectedItems.Count == 0) { return false; }

            return selectedItems.Select(s => ImageFileCollection.IndexOf(s)).Min() != 0;
        }

        private bool CanMoveDown(List<ListBoxItem> selectedItems)
        {
            if (selectedItems.Count == 0) { return false; }

            return selectedItems.Select(s => ImageFileCollection.IndexOf(s)).Max() != ImageFileCollection.Count - 1;
        }

        private void UpdateButtonStatus()
        {
            var selectedItems = fileListBox.SelectedItems.Cast<ListBoxItem>().ToList();
            if (selectedItems.Count == 0)
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

            moveUpButton.IsEnabled = CanMoveUp(selectedItems);
            moveDownButton.IsEnabled = CanMoveDown(selectedItems);
        }

        private void ValidateInputPage()
        {
            if (fileListBox.Items.Count == 0)
            {
                wizard.NextEnabled = false;
            }
            else
            {
                wizard.NextEnabled = true;
            }
        }
        
        private void fileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStatus();
        }

        private void deleteFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (fileListBox.SelectedItems == null) { return; }

            var selectedItems = fileListBox.SelectedItems.Cast<ListBoxItem>().ToList();
            foreach (var selectedItem in selectedItems)
            {
                ImageFileCollection.Remove(selectedItem);
            }

            ValidateInputPage();
        }

        private void selectOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string currentFileName = System.IO.Path.GetFileName(outputFileNameTextBox.Text);
                outputFileNameTextBox.Text = $@"{dialog.SelectedPath}\{currentFileName}";
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

            var outputFilePath = outputFileNameTextBox.Text;
            var converter = new ImageToPdfConverter(sourceFileList, outputFilePath, _inputFileHandlingStrategy);
            await Task.Run(() => converter.ConvertImagesToPdf(progress));
        }

        private void openPdfButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(outputFileNameTextBox.Text);
        }

        private void removeDuplicatesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckDuplicates();
        }

        private void wizard_SelectedPageChanging(object sender, WizardPageSelectionChangeEventArgs e)
        {
            if (e.NewPage.Name == "output")
            {
                outputFileNameTextBox.Text = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ImageFileCollection.First().Tag.ToString()), "output.pdf");
            }
        }
    }
}
