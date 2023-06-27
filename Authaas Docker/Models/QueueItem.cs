using System;
using System.Collections.Generic;
using System.Linq;
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

}
