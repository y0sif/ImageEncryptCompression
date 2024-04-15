using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageEncryptCompress
{
    public class Node
    {
        public char character;
        public int freq;
        public Node left, right;
        public Node(char character, int freq)
        {
            this.character = character;
            this.freq = freq;
            left = right = null;
        }
    }
}
