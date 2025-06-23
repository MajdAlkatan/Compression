using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinFormsApp1.Compression
{
    public class HuffmanCompressor
    {
        private class Node
        {
            public byte? Symbol { get; set; }
            public int Frequency { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public bool IsLeaf => Left == null && Right == null;
        }

        private Node BuildTree(Dictionary<byte, int> freq)
        {
            var pq = new PriorityQueue<Node, int>();
            foreach (var kv in freq)
                pq.Enqueue(new Node { Symbol = kv.Key, Frequency = kv.Value }, kv.Value);
            while (pq.Count > 1)
            {
                var left = pq.Dequeue();
                var right = pq.Dequeue();
                var parent = new Node { Left = left, Right = right, Frequency = left.Frequency + right.Frequency };
                pq.Enqueue(parent, parent.Frequency);
            }
            return pq.Dequeue();
        }

        private void BuildCodes(Node n, string code, Dictionary<byte, string> dict)
        {
            if (n.IsLeaf)
            {
                dict[n.Symbol!.Value] = code.Length == 0 ? "0" : code;
                return;
            }
            BuildCodes(n.Left, code + "0", dict);
            BuildCodes(n.Right, code + "1", dict);
        }

        private static byte[] BitsToBytes(string bits)
        {
            int len = (bits.Length + 7) / 8; byte[] arr = new byte[len];
            for (int i = 0; i < bits.Length; i++)
                if (bits[i] == '1')
                    arr[i / 8] |= (byte)(1 << (7 - (i % 8)));
            return arr;
        }
        private static string BytesToBits(byte[] arr, int bitCount)
        {
            var sb = new StringBuilder(bitCount);
            for (int i = 0; i < bitCount; i++)
                sb.Append(((arr[i / 8] >> (7 - (i % 8))) & 1) == 1 ? '1' : '0');
            return sb.ToString();
        }

        public byte[] Compress(byte[] input, out double ratio, CancellationToken token = default, ManualResetEventSlim? pauseEvent = null)
        {
            token.ThrowIfCancellationRequested();

            if (input.Length == 0)
            {
                ratio = 0; // assign something before returning
                return Array.Empty<byte>();
            }

            var freq = input.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
            var root = BuildTree(freq);
            var codes = new Dictionary<byte, string>();
            BuildCodes(root, "", codes);

            var bits = new StringBuilder();
            foreach (var b in input)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                bits.Append(codes[b]);
            }

            byte[] bitBytes = BitsToBytes(bits.ToString());

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(freq.Count);
            foreach (var kv in freq)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                bw.Write(kv.Key);
                bw.Write(kv.Value);
            }
            bw.Write(bits.Length);
            bw.Write(bitBytes);

            ratio = 100.0 * (input.Length - ms.Length) / input.Length;

            Console.WriteLine($"[Huffman] Compression Ratio: {ratio:F2}%");

            return ms.ToArray();
        }


        public byte[] Decompress(byte[] data, CancellationToken token = default, ManualResetEventSlim? pauseEvent = null)
        {
            token.ThrowIfCancellationRequested();
            if (data.Length == 0)
                return Array.Empty<byte>();

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            int cnt = br.ReadInt32();
            var freq = new Dictionary<byte, int>();

            for (int i = 0; i < cnt; i++)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                freq[br.ReadByte()] = br.ReadInt32();
            }
            var root = BuildTree(freq);
            int bitLen = br.ReadInt32();
            byte[] enc = br.ReadBytes((bitLen + 7) / 8);
            string bits = BytesToBits(enc, bitLen);

            var res = new List<byte>();
            Node cur = root;
            foreach (char bit in bits)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                cur = bit == '0' ? cur.Left : cur.Right;
                if (cur.IsLeaf)
                {
                    res.Add(cur.Symbol!.Value); cur = root;
                }
            }
            return res.ToArray();
        }
    }
}
