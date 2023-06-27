using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Authaas_Docker.Models
{
    public class QueueItem<T>
    {
        public T Data { get; set; }
        public bool IsProcessed { get; set; }

        public QueueItem(T data)
        {
            Data = data;
            IsProcessed = false;
        }
    }

    public class DownloadableItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string RunArguments { get; set; }
        public string Url { get; set; }
        public string DestinationPath { get; set; }

        public DownloadableItem(string url, string destinationPath)
        {
            Url = url;
            DestinationPath = destinationPath;
        }

        public bool IsInstalled()
        {
            // TODO: Implement logic to check if the item is installed
            throw new NotImplementedException();
        }

        public async Task Download()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(Url);

                using (var fileStream = new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        public void Install()
        {
            // TODO: Implement logic to install the item
            throw new NotImplementedException();
        }
    }

}
