using System;
using System.IO;
using System.Linq;

public static class Explorer
{
    private static DriveInfo[] drives;

    static Explorer()
    {
        drives = DriveInfo.GetDrives();
    }

    public static void ShowDrives(int selectedDriveIndex = -1)
    {
        Console.Clear();
        Console.WriteLine("Доступные диски:");

        for (int i = 0; i < drives.Length; i++)
        {
            var driveText = $"{drives[i].Name} - {drives[i].DriveType}";

            if (i == selectedDriveIndex)
            {
                Console.WriteLine($"-> {driveText}");
            }
            else
            {
                Console.WriteLine($"   {driveText}");
            }
        }
    }

    public static int GetDriveCount()
    {
        return drives.Length;
    }

    public static string GetDriveLetter(int index)
    {
        return drives[index].Name.Substring(0, 1);
    }

    public static string GetCurrentPath(string driveLetter)
    {
        return driveLetter + ":\\";
    }

    public static void ShowDriveInfo(string path)
    {
        var drive = drives.FirstOrDefault(d => d.Name.StartsWith(path, StringComparison.OrdinalIgnoreCase));
        if (drive != null)
        {
            Console.WriteLine($"Информация о диске {drive.Name}:");
            Console.WriteLine($"Общий объем: {drive.TotalSize / (1024 * 1024 * 1024)} ГБ");
            Console.WriteLine($"Свободное место: {drive.AvailableFreeSpace / (1024 * 1024 * 1024)} ГБ");
        }
        else
        {
            Console.WriteLine($"Диск {path} не найден.");
        }
    }

    public static void ShowContents(string path, int selectedFolderIndex = -1)
    {
        Console.WriteLine($"Содержимое {path}:");

        try
        {
            var folders = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            for (int i = 0; i < folders.Length; i++)
            {
                var folderName = Path.GetFileName(folders[i]);
                var prefix = (i == selectedFolderIndex) ? "->" : "  ";
                Console.WriteLine($"{prefix} [Папка] {folderName}");
            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var fileExtension = Path.GetExtension(file);
                Console.WriteLine($"   [Файл] {fileName} (Формат: {fileExtension})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении содержимого: {ex.Message}");
        }
    }


    public static void Navigate(string path)
    {
        Console.Clear();
        ShowContents(path);

        int selectedFolderIndex = 0;

        do
        {
            ConsoleKeyInfo keyInfo = ArrowKeys.WaitForKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                string selectedName = GetSelectedName(path, selectedFolderIndex);
                string newPath = Path.Combine(path, selectedName);

                if (IsDirectory(newPath))
                {
                    ShowContents(newPath);
                    selectedFolderIndex = 0; // Сбросить выбранный индекс при переходе в новую папку
                }
                else
                {
                    OpenFile(newPath);
                }
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                // Перемещение влево по папкам
                selectedFolderIndex = ArrowKeys.GetNewIndex(selectedFolderIndex, GetFolderCount(path) - 1, -1);
                ShowContents(path, selectedFolderIndex);
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                // Перемещение вправо по папкам
                selectedFolderIndex = ArrowKeys.GetNewIndex(selectedFolderIndex, GetFolderCount(path) - 1, 1);
                ShowContents(path, selectedFolderIndex);
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                // Возврат в предыдущую папку или на уровень выше
                if (path.Length > 3)
                {
                    path = Directory.GetParent(path).FullName;
                    ShowContents(path);
                }
                else
                {
                    break;
                }
            }
            // Другие обработки клавиш...

        } while (true);
    }

    private static string GetSelectedName(string path, int index)
    {
        try
        {
            var folders = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            var allItems = folders.Concat(files).ToArray();

            if (index >= 0 && index < allItems.Length)
            {
                return Path.GetFileName(allItems[index]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка файлов и папок: {ex.Message}");
        }

        return string.Empty;
    }


    public static bool IsDirectory(string path)
    {
        FileAttributes attr = File.GetAttributes(path);
        return (attr & FileAttributes.Directory) == FileAttributes.Directory;
    }

    public static string GetFolder(string path, int index)
    {
        try
        {
            var folders = Directory.GetDirectories(path);
            if (index >= 0 && index < folders.Length)
            {
                return Path.GetFileName(folders[index]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка папок: {ex.Message}");
        }

        return string.Empty;
    }
    public static int GetFolderCount(string path)
    {
        try
        {
            return Directory.GetDirectories(path).Length;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка папок: {ex.Message}");
            return 0;
        }
    }



    public static void OpenFile(string filePath)
    {
        try
        {
            Console.WriteLine($"Открытие файла: {filePath}");

            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".txt")
            {
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            else
            {
                System.Diagnostics.Process.Start(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при открытии файла: {ex.Message}");
        }
    }

}


public static class ArrowKeys
{
    public static ConsoleKeyInfo WaitForKey()
    {
        return Console.ReadKey(true);
    }

    public static int GetNewIndex(int currentIndex, int maxIndex, int step)
    {
        int newIndex = currentIndex + step;
        if (newIndex < 0)
        {
            return maxIndex;
        }
        else if (newIndex > maxIndex)
        {
            return 0;
        }
        return newIndex;
    }

}
class Program
{
    static void Main()
    {
        Console.CursorVisible = false;

        Explorer.ShowDrives();

        ConsoleKeyInfo keyInfo;
        int selectedDriveIndex = 0; // Индекс выбранного диска

        do
        {
            keyInfo = ArrowKeys.WaitForKey();

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.Write("Выберите диск и нажмите Enter: ");
                string selectedDrive = Explorer.GetDriveLetter(selectedDriveIndex);
                string currentPath = Explorer.GetCurrentPath(selectedDrive);
                Explorer.ShowDriveInfo(currentPath);
                Explorer.ShowContents(currentPath);

                int selectedFolderIndex = 0;

                do
                {
                    keyInfo = ArrowKeys.WaitForKey();

                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        string selectedFolder = Explorer.GetFolder(currentPath, selectedFolderIndex);
                        string newPath = Path.Combine(currentPath, selectedFolder);

                        if (Explorer.IsDirectory(newPath))
                        {
                            currentPath = newPath;
                            Explorer.Navigate(currentPath);
                        }
                        else
                        {
                            Explorer.OpenFile(newPath);
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        // Перемещение влево по папкам
                        selectedFolderIndex = ArrowKeys.GetNewIndex(selectedFolderIndex, Explorer.GetFolderCount(currentPath) - 1, -1);
                        Explorer.ShowContents(currentPath, selectedFolderIndex);
                    }
                    else if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        // Перемещение вправо по папкам
                        selectedFolderIndex = ArrowKeys.GetNewIndex(selectedFolderIndex, Explorer.GetFolderCount(currentPath) - 1, 1);
                        Explorer.ShowContents(currentPath, selectedFolderIndex);
                    }
                    else if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        // Возврат в предыдущую папку или на уровень выше
                        if (currentPath.Length > 3)
                        {
                            currentPath = Directory.GetParent(currentPath).FullName;
                            Explorer.Navigate(currentPath);
                        }
                        else
                        {
                            break;
                        }
                    }
                    // Другие обработки клавиш...

                } while (true);
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                // Перемещение влево по дискам
                selectedDriveIndex = ArrowKeys.GetNewIndex(selectedDriveIndex, Explorer.GetDriveCount() - 1, -1);
                Explorer.ShowDrives(selectedDriveIndex);
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                // Перемещение вправо по дискам
                selectedDriveIndex = ArrowKeys.GetNewIndex(selectedDriveIndex, Explorer.GetDriveCount() - 1, 1);
                Explorer.ShowDrives(selectedDriveIndex);
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                // Пользователь нажал Escape, выходим из программы
                break;
            }
            // Другие обработки клавиш...

        } while (true);

        Console.CursorVisible = true;
    }
}






