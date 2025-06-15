using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace WinFormsApp1.Compression
{
    public static partial class CompressionManager
    {
        // ضغط كل ملف على حِدة ⇒ *.cmp
        public static Dictionary<string,double> CompressFiles(
            IEnumerable<string> files,
            string algorithm,
            string outputFolder,
            CancellationToken token = default,
            ManualResetEventSlim? pauseEvent = null)
        {
            var ratios = new Dictionary<string, double>();

            foreach (var path in files)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();

                byte[] data   = File.ReadAllBytes(path);
                double ratio;
                byte[] comp   = algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Compress(data, out ratio)
                    : new HuffmanCompressor().Compress(data, out ratio);

                var outPath = Path.Combine(outputFolder, Path.GetFileName(path) + ".cmp");
                File.WriteAllBytes(outPath, comp);
                ratios[path] = ratio;
            }
            return ratios;
        }

        // فك ضغط ملفات *.cmp
        public static void DecompressFiles(
            IEnumerable<string> cmpPaths,
            string algorithm,
            CancellationToken token = default)
        {
            foreach (var path in cmpPaths)
            {
                token.ThrowIfCancellationRequested();

                byte[] comp = File.ReadAllBytes(path);
                byte[] data = algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Decompress(comp)
                    : new HuffmanCompressor().Decompress(comp);

                var outPath = path.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase)
                              ? path[..^4]
                              : path + ".orig";
                File.WriteAllBytes(outPath, data);
            }
        }
    }
}
