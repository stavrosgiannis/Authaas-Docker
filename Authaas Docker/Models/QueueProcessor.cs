using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authaas_Docker.Models
{
    public class QueueProcessor<T> : IEnumerable<QueueItem<T>>
    {
        private readonly Queue<QueueItem<T>> _queue = new Queue<QueueItem<T>>();

        public void Enqueue(T item)
        {
            _queue.Enqueue(new QueueItem<T>(item));
        }

        public QueueItem<T> Dequeue()
        {
            return _queue.Dequeue();
        }

        public bool IsEmpty()
        {
            return !_queue.Any();
        }

        public async Task ProcessAll(Func<T, Task> processFunction)
        {
            while (!IsEmpty())
            {
                var item = Dequeue();
                await processFunction(item.Data);
                item.IsProcessed = true;
            }
        }

        public IEnumerator<QueueItem<T>> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


}
