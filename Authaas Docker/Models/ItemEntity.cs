using System.Diagnostics;

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
    public string InstallationDir { get; set; }

    /// <summary>
    ///     Checks if the application is installed in the specified directory.
    /// </summary>
    /// <returns>
    ///     Returns a GenericResult indicating if the application is installed or not.
    /// </returns>
    public async Task<GenericResult> IsInstalled()
    {
        if (Directory.Exists(InstallationDir))
            return GenericResult.Ok();

        return GenericResult.Fail("isInstalled false");
    }

    /// <summary>
    ///     Downloads a file from a given URL and saves it to a given destination path.
    /// </summary>
    /// <param name="progressBar">Progress bar to update the download progress.</param>
    /// <returns>GenericResult indicating the success of the operation.</returns>
    public async Task<GenericResult> DownloadFile(ProgressBar progressBar)
    {
        try
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(new Uri(Url), HttpCompletionOption.ResponseHeadersRead))
                using (var fileStream =
                       new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
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
                                progressBar.Invoke(new Action(() => progressBar.Value = progressPercentage));
                            }
                        }
                    } while (isMoreToRead);
                }

                return GenericResult.Ok();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return GenericResult.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Installs the application using the specified installer path and arguments.
    /// </summary>
    /// <returns>
    /// A <see cref="GenericResult"/> indicating the success or failure of the installation.
    /// </returns>
    public async Task<GenericResult> Install()
    {
        try
        {
            var installerPath = $@"{DestinationPath}"; // Replace with the actual path to your installer executable

            var startInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                WorkingDirectory = Application.StartupPath,
                Arguments = RunArguments,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);

            // Wait for the process to exit
            await process.WaitForExitAsync();

            // Check the exit code of the process to determine if the installation was successful
            if (process.ExitCode == 0)
                // Return success result
                return GenericResult.Ok();
            // Return failure result with an error message
            return GenericResult.Fail("Installation failed with exit code: " + process.ExitCode);
        }
        catch (Exception ex)
        {
            return GenericResult.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a file from the specified destination path.
    /// </summary>
    /// <returns>
    /// Returns a GenericResult indicating the success or failure of the operation.
    /// </returns>
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