using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;

namespace Infrastructure.Services;

public class FileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private string GetWebRootPath() => environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");

    public async Task<string> SaveFileAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл холӣ аст (Файл пуст)");

        var webRootPath = GetWebRootPath();
        var uploadsFolder = Path.Combine(webRootPath, folderName);
        
        // Логируем путь для отладки
        Console.WriteLine($"[DEBUG] Saving file to: {uploadsFolder}");
        
        try
        {
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                
                // На Linux устанавливаем права 775 (rwxrwxr-x) для создаваемой папки
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    try
                    {
                        File.SetUnixFileMode(uploadsFolder, 
                            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                            UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                            UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                    }
                    catch { /* Игнорируем ошибки установки прав, если не удалось */ }
                }
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return Path.Combine(folderName, uniqueFileName).Replace("\\", "/");
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException(
                $"Ошибка доступа к папке: {uploadsFolder}. " +
                $"Убедитесь, что у пользователя веб-сервера (например, www-data) есть права на запись. " +
                $"Попробуйте выполнить команду на сервере: sudo chown -R www-data:www-data {webRootPath} && sudo chmod -R 775 {webRootPath}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при сохранении файла: {ex.Message}", ex);
        }
    }

    public Task DeleteFileAsync(string fileName, string folderName)
    {
        var webRootPath = GetWebRootPath();
        var filePath = Path.Combine(webRootPath, folderName, fileName);
        
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"Ошибка доступа при удалении файла: {filePath}", ex);
        }
        
        return Task.CompletedTask;
    }
}
