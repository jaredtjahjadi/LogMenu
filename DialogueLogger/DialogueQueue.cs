using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogueLogger
{
    public class DialogueQueue<T> : IEnumerable<T>
    {
        private readonly int maxSize;
        private readonly Queue<T> queue;

        public DialogueQueue(int maxSize)
        {
            if (maxSize <= 0) throw new ArgumentOutOfRangeException("maxSize", "The maximum size must be greater than zero.");
            this.maxSize = maxSize;
            this.queue = new Queue<T>();
        }

        public void enqueue(T item)
        {
            if (queue.Count >= maxSize) queue.Dequeue();
            queue.Enqueue(item);
        }

        public T dequeue()
        {
            return queue.Dequeue();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return queue.Count; } }
    }
}
