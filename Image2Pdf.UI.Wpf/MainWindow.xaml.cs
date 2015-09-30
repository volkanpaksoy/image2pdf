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
        private bool _userUpdatedOutput = false;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            SetSelectedInputFileHandlingStrategy();
            
            _userUpdatedOutput = false;
        }


        #region Page 1: Input

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

        private void fileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStatus();
        }

        private void removeDuplicatesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckDuplicates();
        }

        #endregion

        #region Page 2: Output
        
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

        #endregion

        #region Page 3: Generate PDF

        private async void convertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PreparePageForPreProcess();

                await StartProcess();

                PreparePageForPostProcess();
            }
            catch (Exception ex)
            {
                progressMessageTextBox.Foreground = new SolidColorBrush(Colors.Red);
                progressMessageTextBox.Text = $"Error: {ex.Message}";
            }
            finally
            {
                
            }
        }

        private void PreparePageForPostProcess()
        {
            openPdfButton.IsEnabled = true;
            wizard.FinishEnabled = true;
        }

        private void PreparePageForPreProcess()
        {
            convertButton.IsEnabled = false;
            wizard.BackEnabled = false;
            wizard.CancelEnabled = false;
            inputFileActionContainer.IsEnabled = false;
            progressMessageTextBox.Visibility = Visibility.Visible;
        }

        private async Task StartProcess()
        {
            var sourceFileList = fileListBox.Items.Cast<ListBoxItem>()
                .Select(lbi => lbi.Tag.ToString())
                .ToList();

            progressBar.Visibility = Visibility.Visible;
            progressBar.Maximum = sourceFileList.Count;

            var progress = new Progress<TaskProgress>(prog =>
            {
                progressMessageTextBox.Text = prog.StatusMessage;
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

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio.Tag == null)
            {
                return;
            }

            int selectedValue = int.Parse(radio.Tag.ToString());
            switch (selectedValue)
            {
                case 1: _inputFileHandlingStrategy = null; break;
                case 2: _inputFileHandlingStrategy = new MoveInputFilesToRecyclebinStrategy(); break;
                case 3: _inputFileHandlingStrategy = new DeleteInputFilesStrategy(); break;
                case 4: _inputFileHandlingStrategy = new BackupInputFilesStrategy(); break;
                case 5: _inputFileHandlingStrategy = new RenameInputFilesStrategy(); break;
            }
        }

        private int GetSelectedInputFileHandlingStrategy()
        {
            RadioButton selectedOption = inputFileStrategyRadio1;

            if      (inputFileStrategyRadio1.IsChecked.Value) { selectedOption = inputFileStrategyRadio1; }
            else if (inputFileStrategyRadio2.IsChecked.Value) { selectedOption = inputFileStrategyRadio2; }
            else if (inputFileStrategyRadio3.IsChecked.Value) { selectedOption = inputFileStrategyRadio3; }
            else if (inputFileStrategyRadio4.IsChecked.Value) { selectedOption = inputFileStrategyRadio4; }
            else if (inputFileStrategyRadio5.IsChecked.Value) { selectedOption = inputFileStrategyRadio5; }

            return int.Parse(selectedOption.Tag.ToString());
        }

        private void SetSelectedInputFileHandlingStrategy()
        {
            switch (UserPreferences.Default.InputFileStrategy)
            {
                case 1: inputFileStrategyRadio1.IsChecked = true; break;
                case 2: inputFileStrategyRadio2.IsChecked = true; break;
                case 3: inputFileStrategyRadio3.IsChecked = true; break;
                case 4: inputFileStrategyRadio4.IsChecked = true; break;
                case 5: inputFileStrategyRadio5.IsChecked = true; break;
                default: inputFileStrategyRadio1.IsChecked = true; break;
            }
        }

        #endregion

        private void wizard_Next(object sender, RoutedEventArgs e)
        {
            ValidateInputPage();
        }

        private void wizard_Finish(object sender, RoutedEventArgs e)
        {
            SaveConfigChanges();

            CleanUp();
        }

        private void CleanUp()
        {
            ImageFileCollection.Clear();
            
            inputFileActionContainer.IsEnabled = true;
            progressMessageTextBox.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Hidden;
            openPdfButton.IsEnabled = false;
            convertButton.IsEnabled = true;
            // LoadConfig();

            wizard.SelectedWizardPage = (WizardPage)wizard.Items.GetItemAt(0);
        }

        private void wizard_Cancel(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit the application?", "Exit", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void SaveConfigChanges()
        {
            UserPreferences.Default.InputFileStrategy = GetSelectedInputFileHandlingStrategy();
            UserPreferences.Default.Save();
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

        private void wizard_SelectedPageChanging(object sender, WizardPageSelectionChangeEventArgs e)
        {
            if (e.NewPage.Name == "input")
            {
                wizard.BackEnabled = false;
                wizard.FinishEnabled = false;
                ValidateInputPage();
            }
            else if(e.NewPage.Name == "output")
            {
                if (!_userUpdatedOutput)
                {
                    outputFileNameTextBox.Text = Path.Combine(
                        Path.GetDirectoryName(ImageFileCollection.First().Tag.ToString()), 
                        $"image2df-output-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.pdf");
                    _userUpdatedOutput = false;
                }

                wizard.BackEnabled = true;
                wizard.NextEnabled = true;
                wizard.FinishEnabled = false;
            }
            else if (e.NewPage.Name == "run")
            {
                wizard.BackEnabled = true;
                wizard.NextEnabled = false;
                wizard.FinishEnabled = false;
            }
        }

        private void outputFileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _userUpdatedOutput = true;
        }

        private void wizard_About(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
    }
}
