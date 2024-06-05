using FileSearch.Core;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace FileSearch.UI
{
    public partial class MainWindow : Window
    {
        private readonly string indexPath = "file_index.json";
        private List<string> files;
        private readonly FileIndexer fileIndexer;
        private readonly FileSearcher fileSearcher;

        public MainWindow()
        {
            InitializeComponent();
            fileIndexer = new FileIndexer();
            fileSearcher = new FileSearcher();
            IndexFiles();
        }

        private void IndexFiles()
        {
            try
            {
                var stopWatch = Stopwatch.StartNew();
                files = fileIndexer.LoadIndex(indexPath) ?? IndexAndSaveFiles();
                stopWatch.Stop();
                MessageBox.Show($"Indexing completed in {stopWatch.Elapsed.TotalSeconds} seconds.\nTotal files indexed: {files.Count}", "Indexing Complete");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during indexing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<string> IndexAndSaveFiles()
        {
            var files = fileIndexer.IndexFiles();
            fileIndexer.SaveIndex(files, indexPath);
            return files;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            var searchTerm = SearchTextBox.Text;
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Please enter a search term.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var searchResults = fileSearcher.SearchFiles(files, searchTerm);
            DisplaySearchResults(searchResults, searchTerm);
        }

        private void DisplaySearchResults(List<string> searchResults, string searchTerm)
        {
            ResultsListBox.Items.Clear();
            if (searchResults.Count > 0)
            {
                foreach (var file in searchResults)
                {
                    ResultsListBox.Items.Add(CreateSearchResultModel(file, searchTerm));
                }
            }
            else
            {
                ResultsListBox.Items.Add("No files found.");
            }
        }

        private SearchResultModel CreateSearchResultModel(string filePath, string searchTerm)
        {
            var regex = new Regex($"({Regex.Escape(searchTerm)})", RegexOptions.IgnoreCase);
            var match = regex.Match(filePath);
            if (match.Success)
            {
                return new SearchResultModel
                {
                    Prefix = filePath.Substring(0, match.Index),
                    Match = match.Value,
                    Suffix = filePath.Substring(match.Index + match.Length),
                    FullPath = filePath
                };
            }
            return new SearchResultModel { FullPath = filePath };
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem is SearchResultModel selectedItem)
            {
                OpenFile(selectedItem.FullPath);
            }
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetContextMenuSelectedItem(sender) is SearchResultModel selectedItem)
            {
                OpenFile(selectedItem.FullPath);
            }
        }

        private void ShowInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetContextMenuSelectedItem(sender) is SearchResultModel selectedItem)
            {
                ShowInExplorer(selectedItem.FullPath);
            }
        }

        private void CopyPathMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetContextMenuSelectedItem(sender) is SearchResultModel selectedItem)
            {
                Clipboard.SetText(selectedItem.FullPath);
                MessageBox.Show("File path copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenFile(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowInExplorer(string filePath)
        {
            try
            {
                string argument = "/select, \"" + filePath + "\"";
                Process.Start("explorer.exe", argument);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing file in Explorer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private SearchResultModel GetContextMenuSelectedItem(object sender)
        {
            if (sender is MenuItem menuItem)
            {
                return menuItem.DataContext as SearchResultModel;
            }
            return null;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    public class SearchResultModel
    {
        public string Prefix { get; set; }
        public string Match { get; set; }
        public string Suffix { get; set; }
        public string FullPath { get; set; }
    }
}