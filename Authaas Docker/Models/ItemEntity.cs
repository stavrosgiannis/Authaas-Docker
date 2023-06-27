using System.Management;

namespace Authaas_Docker.Models;

public class QueueItem<T>
{
    public QueueItem(T data)
    {
        Data = data;
        IsProcessed = false;
    }

    public T Data { get; set; }
    public bool IsProcessed { get; set; }
}

public class DownloadableItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string RunArguments { get; set; }
    public string Url { get; set; }
    public string DestinationPath { get; set; }


    public async Task<GenericResult> IsInstalled()
    {
        var result = await GetInstalledSoftware();

        if (result.Success) return new GenericResult<bool>(true, true, "");

        return new GenericResult<bool>(false, false, "");
    }

    /// <summary>
    ///     Downloads a file from a given URL and saves it to a given destination path.
    /// </summary>
    /// <param name="progressBar">Progress bar to update the download progress.</param>
    /// <returns>GenericResult indicating the success of the operation.</returns>
    public async Task<GenericResult> DownloadFile(ProgressBar progressBar)
    {
        using (var client = new HttpClient())
        {
            using (var response = await client.GetAsync(new Uri(Url), HttpCompletionOption.ResponseHeadersRead))
            using (var fileStream = new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var downloadStream = await response.Content.ReadAsStreamAsync())
            {
                long totalBytes;
                if (response.Content.Headers.ContentLength.HasValue)
                    totalBytes = response.Content.Headers.ContentLength.Value;
                else
                    totalBytes = -1; // Indicate that the length is unknown

                var totalReadBytes = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;

                do
                {
                    var readBytes = await downloadStream.ReadAsync(buffer, 0, buffer.Length);
                    if (readBytes == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        await fileStream.WriteAsync(buffer, 0, readBytes);

                        totalReadBytes += readBytes;
                        // Update progress bar
                        if (totalBytes > 0)
                        {
                            var progressPercentage = (int)(totalReadBytes * 100 / totalBytes);
                            progressBar.Value = progressPercentage;
                        }
                    }
                } while (isMoreToRead);
            }
        }

        return GenericResult.Ok();
    }

    private async Task<GenericResult> GetInstalledSoftware()
    {
        var targetApplication = Name; // Replace with the name of your application

        using (var searcher = new ManagementObjectSearcher("SELECT Name, Version FROM Win32_Product"))
        {
            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString();
                var version = obj["Version"]?.ToString();

                if (name != null &&
                    name.Contains(targetApplication,
                        StringComparison.OrdinalIgnoreCase))
                    return new GenericResult<bool>(true, true, ""); // Application is installed
            }
        }

        return new GenericResult<bool>(false, false, "");
    }

    public async Task<GenericResult> Install()
    {
        try
        {
            return GenericResult.Ok(IsInstalled());
        }
        catch (Exception ex)
        {
            return GenericResult.Fail(ex.Message);
        }
    }

    public async Task<GenericResult> CleanTemp()
    {
        try
        {
            if (File.Exists(DestinationPath))
            {
                File.Delete(DestinationPath);
                return GenericResult.Ok($"Deleted File {DestinationPath}");
            }

            return GenericResult.Ok("No file found");
        }
        catch (Exception ex)
        {
            return GenericResult.Fail(ex.Message);
        }
    }
}