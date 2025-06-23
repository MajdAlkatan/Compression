
namespace WinFormsApp1.Compression
{
    public static partial class CompressionManager
    {
        public static Dictionary<string, double> CompressFiles(
            IEnumerable<string> files,
            string algorithm,
            string outputFolder,
            CancellationToken token = default,
            ManualResetEventSlim? pauseEvent = null)
        {
            var ratios = new Dictionary<string, double>();
            var algoType = algorithm == "Shannon-Fano" ? AlgorithmType.SHANNON : AlgorithmType.HUFFMAN;

            foreach (var path in files)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();

                byte[] data = File.ReadAllBytes(path);
                double ratio;
                byte[] comp = algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Compress(data, out ratio, token)
                    : new HuffmanCompressor().Compress(data, out ratio, token);

                var outPath = Path.Combine(outputFolder, Path.GetFileName(path) + ".cmp");
                using var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write);
                using var bw = new BinaryWriter(fs);
                bw.Write((int)StorageType.SINGLE);
                bw.Write((int)algoType);
                bw.Write(comp.Length);
                bw.Write(comp);
                ratios[path] = ratio;
            }
            return ratios;
        }

        public static async Task<Dictionary<string, double>> CompressFilesAsync(
            IEnumerable<string> files,
            string algorithm,
            string outputFolder,
            CancellationToken token = default,
            ManualResetEventSlim? pauseEvent = null)
        {
            var tasks = new List<Task<KeyValuePair<string, double>>>();

            foreach (var path in files)
            {
                tasks.Add(Task.Run(async () => await ProcessFileAsync(path, algorithm, outputFolder, token, pauseEvent)));
            }

            var results = await Task.WhenAll(tasks);
            return results.ToDictionary(r => r.Key, r => r.Value);
        }

        private static async Task<KeyValuePair<string, double>> ProcessFileAsync(
            string path,
            string algorithm,
            string outputFolder,
            CancellationToken token,
            ManualResetEventSlim? pauseEvent)
        {
            token.ThrowIfCancellationRequested();
            pauseEvent?.Wait(token);

            var algoType = algorithm == "Shannon-Fano" ? AlgorithmType.SHANNON : AlgorithmType.HUFFMAN;

            byte[] data = await File.ReadAllBytesAsync(path, token);

            double ratio;
            byte[] comp = algorithm == "Shannon-Fano"
                ? new ShannonFanoCompressor().Compress(data, out ratio, token, pauseEvent)
                : new HuffmanCompressor().Compress(data, out ratio, token, pauseEvent);

            var outPath = Path.Combine(outputFolder, Path.GetFileName(path) + ".cmp");
            using var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);
            token.ThrowIfCancellationRequested();
            bw.Write((int)StorageType.SINGLE);
            token.ThrowIfCancellationRequested();
            bw.Write((int)algoType);
            token.ThrowIfCancellationRequested();
            bw.Write(comp.Length);
            token.ThrowIfCancellationRequested();
            bw.Write(comp);

            return new KeyValuePair<string, double>(path, ratio);
        }

        public static void DecompressFiles(
            IEnumerable<string> cmpPaths,
            CancellationToken token = default,
            ManualResetEventSlim? pevent = null)
        {
            foreach (var path in cmpPaths)
            {
                token.ThrowIfCancellationRequested();
                pevent?.Wait();

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var br = new BinaryReader(fs);

                var type = (StorageType)br.ReadInt32();
                var algoType = (AlgorithmType)br.ReadInt32();
                var byteCount = br.ReadInt32();

                byte[] comp = br.ReadBytes(byteCount);
                byte[] data = algoType == AlgorithmType.SHANNON
                    ? new ShannonFanoCompressor().Decompress(comp, token, pevent)
                    : new HuffmanCompressor().Decompress(comp, token, pevent);

                var outPath = path.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase)
                              ? path[..^4]
                              : path + ".orig";
                File.WriteAllBytes(outPath, data);
            }
        }
        public static async Task DecompressFilesAsync(
            IEnumerable<string> cmpPaths,
            CancellationToken token = default,
            ManualResetEventSlim? pevent = null)
        {
            var tasks = new List<Task>();

            foreach (var path in cmpPaths)
            {
                string localPath = path;

                tasks.Add(Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    pevent?.Wait();

                    using var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                    using var br = new BinaryReader(fs);

                    var type = (StorageType)br.ReadInt32();
                    var algoType = (AlgorithmType)br.ReadInt32();
                    var byteCount = br.ReadInt32();

                    byte[] comp = br.ReadBytes(byteCount);
                    byte[] data = algoType == AlgorithmType.SHANNON
                        ? new ShannonFanoCompressor().Decompress(comp, token, pevent)
                        : new HuffmanCompressor().Decompress(comp, token, pevent);

                    var outPath = localPath.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase)
                                  ? localPath[..^4]
                                  : localPath + ".orig";

                    File.WriteAllBytes(outPath, data);

                }, token));
            }

            await Task.WhenAll(tasks);
        }
    }
}
