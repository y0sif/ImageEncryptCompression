using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageEncryptCompress
{
    public class Node<T>
    {
        public T value;
        public int freq;
        public Node<T> left, right;
        public Node(T value, int freq)
        {
            this.value = value;
            this.freq = freq;
            left = right = null;
        }
    }
}
