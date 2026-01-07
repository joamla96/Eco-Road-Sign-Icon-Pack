using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eco.Client.Utils
{
    public class PriorityQueue<T>
    {
        private readonly SortedSet<(int priority, T item)> _queue;

        public PriorityQueue()
        {
            // Use a custom comparer that compares only by priority
            _queue = new SortedSet<(int priority, T item)>(Comparer<(int, T)>.Create((x, y) =>
            {
                int result = x.Item1.CompareTo(y.Item1);
                return result == 0 ? 1 : result; // Force uniqueness by returning non-zero if priorities are equal
            }));
        }

        public void Enqueue(T item, int priority)
        {
            _queue.Add((priority, item));
        }

        public T Dequeue()
        {
            if (_queue.Count == 0) throw new InvalidOperationException("Queue is empty");

            var item = _queue.Min; // Get the item with the smallest priority
            _queue.Remove(item);   // Remove it from the queue

            return item.item;
        }

        public bool IsEmpty() => _queue.Count == 0;
    }
}
