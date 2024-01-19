using System;
using System.Collections;
using System.Collections.Generic;

namespace DialogueLogger
{
    // Wrapper class for queue - the difference is the queue has a max amount of elements
    public class DialogueQueue<T> : Queue<T>, IEnumerable<T>
    {
        private readonly int maxSize; // Configurable in ModConfig.js
        private readonly Queue<T> queue;

        public DialogueQueue(int maxSize)
        {
            if (maxSize <= 0) throw new ArgumentOutOfRangeException("maxSize", "The maximum size must be greater than zero.");
            this.maxSize = maxSize;
            this.queue = new Queue<T>();
        }

        public void enqueue(T item)
        {
            if (queue.Count >= maxSize) queue.Dequeue(); // Queue has a max size - if exceeded, remove first element from queue
            queue.Enqueue(item);
        }
        // Makes this class iterable
        public new IEnumerator<T> GetEnumerator() { return queue.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}