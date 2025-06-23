using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinFormsApp1.Compression
{
    public class ShannonFanoCompressor
    {
        private class Sym { public byte B; public int F; public string C = ""; }
        private void Build(List<Sym> s, int a, int b)
        {
            if (a >= b) return;

            int total = s.Skip(a).Take(b - a + 1).Sum(t => t.F), half = total / 2, sum = 0, idx = a;

            for (; idx < b; idx++)
            {
                sum += s[idx].F;
                if (sum >= half)
                    break;
            }
            for (int i = a; i <= idx; i++)
                s[i].C += "0";
            for (int i = idx + 1; i <= b; i++)
                s[i].C += "1";
            Build(s, a, idx);
            Build(s, idx + 1, b);
        }
        private byte[] BitsToBytes(string bits) 
        {
            int n = (bits.Length + 7) / 8;
            byte[] a = new byte[n];
            for (int i = 0; i < bits.Length; i++)
                if (bits[i] == '1')
                    a[i / 8] |= (byte)(1 << (7 - (i % 8)));
            return a;
        }
        private string BytesToBits(byte[] a, int len)
        { 
            var sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
                sb.Append(((a[i / 8] >> (7 - (i % 8))) & 1) == 1 ? '1' : '0');
            return sb.ToString(); 
        }
        public byte[] Compress(byte[] input, out double ratio, CancellationToken token = default, ManualResetEventSlim? pauseEvent = null)
        {
            token.ThrowIfCancellationRequested();
            var list = input.GroupBy(b => b).Select(g => new Sym { B = g.Key, F = g.Count() }).OrderByDescending(s => s.F).ToList();
            
            Build(list, 0, list.Count - 1);
            
            var dict = list.ToDictionary(s => s.B, s => s.C);
            var bits = new StringBuilder();

            foreach (var b in input)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                bits.Append(dict[b]);
            }
            
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            
            w.Write(list.Count);
            
            foreach (var s in list)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                w.Write(s.B);
                w.Write(s.F);
                w.Write((byte)s.C.Length);
                w.Write(BitsToBytes(s.C));
            }

            w.Write(bits.Length);
            w.Write(BitsToBytes(bits.ToString()));

            int originalSize = input.Length;
            int compressedSize = (int)ms.Length;
            ratio = 100.0 * (originalSize - compressedSize) / originalSize;
            Console.WriteLine($"Compression Ratio: {ratio:F2}%");

            return ms.ToArray();
        }
        public byte[] Decompress(byte[] input, CancellationToken token = default, ManualResetEventSlim? pauseEvent = null)
        {
            using var ms = new MemoryStream(input);
            using var r = new BinaryReader(ms);

            int cnt = r.ReadInt32();
            var map = new Dictionary<string, byte>();

            for (int i = 0; i < cnt; i++)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                byte b = r.ReadByte();
                int f = r.ReadInt32();
                byte len = r.ReadByte();
                byte[] cb = r.ReadBytes((len + 7) / 8);
                string code = BytesToBits(cb, len); map[code] = b;
            }

            int bitLen = r.ReadInt32();
            byte[] enc = r.ReadBytes((bitLen + 7) / 8);
            string bits = BytesToBits(enc, bitLen);
            var sbCode = new StringBuilder();
            var outList = new List<byte>();

            foreach (char bit in bits)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();
                sbCode.Append(bit);
                if (map.TryGetValue(sbCode.ToString(), out byte v))
                {
                    outList.Add(v); sbCode.Clear();
                }
            }
            return outList.ToArray();
        }
    }
}
