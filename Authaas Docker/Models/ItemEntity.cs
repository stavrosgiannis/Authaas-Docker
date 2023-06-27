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


    public bool IsInstalled()
    {
        // TODO: Implement logic to check if the item is installed
        throw new NotImplementedException();
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


    public async Task<GenericResult> Install()
    {
        // TODO: Implement logic to install the item
        throw new NotImplementedException();
    }
}