namespace FileSearch.Core
{
    public class FileSearcher
    {
        public List<string> SearchFiles(List<string> files, string searchTerm)
        {
            return files.Where(f => f.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
