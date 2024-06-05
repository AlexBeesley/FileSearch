using System.Collections.Concurrent;
using System.Text.Json;

namespace FileSearch.Core
{
    public class FileIndexer
    {
        public List<string> LoadIndex(string indexPath)
        {
            if (File.Exists(indexPath))
            {
                try
                {
                    var json = File.ReadAllText(indexPath);
                    return JsonSerializer.Deserialize<List<string>>(json);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error loading index: {ex.Message}", ex);
                }
            }
            return null;
        }

        public void SaveIndex(List<string> files, string indexPath)
        {
            try
            {
                var json = JsonSerializer.Serialize(files);
                File.WriteAllText(indexPath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving index: {ex.Message}", ex);
            }
        }

        public List<string> IndexFiles()
        {
            var files = new List<string>();
            var driveInfos = DriveInfo.GetDrives().Where(d => d.IsReady);

            foreach (var drive in driveInfos)
            {
                try
                {
                    files.AddRange(IndexDirectory(drive.RootDirectory.FullName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing drive {drive.Name}: {ex.Message}");
                }
            }

            return files;
        }

        public IEnumerable<string> IndexDirectory(string path)
        {
            var files = new ConcurrentBag<string>();

            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    files.Add(file);
                }

                var directories = Directory.EnumerateDirectories(path);
                Parallel.ForEach(directories, (directory) =>
                {
                    try
                    {
                        foreach (var file in IndexDirectory(directory))
                        {
                            files.Add(file);
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Access denied to directory {directory}: {ex.Message}");
                    }
                    catch (PathTooLongException ex)
                    {
                        Console.WriteLine($"Path too long: {directory}: {ex.Message}");
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        Console.WriteLine($"Directory not found: {directory}: {ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"IO error in directory {directory}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error in directory {directory}: {ex.Message}");
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to path {path}: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                Console.WriteLine($"Path too long: {path}: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory not found: {path}: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO error in path {path}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in path {path}: {ex.Message}");
            }

            return files;
        }
    }
}
