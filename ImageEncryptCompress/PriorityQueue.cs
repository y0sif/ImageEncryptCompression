using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageEncryptCompress
{
    public class PriorityQueue<TPriority, TValue> where TPriority : IComparable<TPriority>
    {
        private readonly List<KeyValuePair<TPriority, TValue>> heap;

        public int Count => heap.Count;

        public PriorityQueue()
        {
            heap = new List<KeyValuePair<TPriority, TValue>>();
        }

        public void Enqueue(TPriority priority, TValue value)
        {
            heap.Add(new KeyValuePair<TPriority, TValue>(priority, value));
            int currentIndex = heap.Count - 1;
            while (currentIndex > 0)
            {
                int parentIndex = (currentIndex - 1) / 2;
                if (heap[parentIndex].Key.CompareTo(heap[currentIndex].Key) <= 0)
                    break;
                Swap(currentIndex, parentIndex);
                currentIndex = parentIndex;
            }
        }

        public TValue Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Priority queue is empty");

            TValue result = heap[0].Value;
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            int currentIndex = 0;
            while (currentIndex < heap.Count)
            {
                int leftChildIndex = 2 * currentIndex + 1;
                int rightChildIndex = 2 * currentIndex + 2;
                int smallestChildIndex = -1;

                if (leftChildIndex < heap.Count)
                {
                    smallestChildIndex = leftChildIndex;
                    if (rightChildIndex < heap.Count && heap[rightChildIndex].Key.CompareTo(heap[leftChildIndex].Key) < 0)
                        smallestChildIndex = rightChildIndex;
                }

                if (smallestChildIndex == -1 || heap[currentIndex].Key.CompareTo(heap[smallestChildIndex].Key) <= 0)
                    break;

                Swap(currentIndex, smallestChildIndex);
                currentIndex = smallestChildIndex;
            }

            return result;
        }

        private void Swap(int i, int j)
        {
            KeyValuePair<TPriority, TValue> temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }
    }
}
