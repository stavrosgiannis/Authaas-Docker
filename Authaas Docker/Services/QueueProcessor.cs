namespace Authaas_Docker.Services;

public class QueueProcessor<T>
{
    private readonly Queue<QueueItem<T>> _queue = new();

    public IEnumerator<QueueItem<T>> GetEnumerator()
    {
        return _queue.GetEnumerator();
    }


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

    public async Task ProcessAll(Func<T, Task> processFunction, Action<T> individualProcessAction)
    {
        while (!IsEmpty())
        {
            var item = Dequeue();
            await processFunction(item.Data);
            individualProcessAction(item.Data);
            item.IsProcessed = true;
        }
    }


    public List<QueueItem<T>> GetQueueItems()
    {
        return _queue.ToList();
    }
}