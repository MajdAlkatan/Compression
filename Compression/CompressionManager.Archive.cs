using System.Collections.Concurrent;
using WinFormsApp1.io;

namespace WinFormsApp1.Compression
{
    public static partial class CompressionManager
    {
        private readonly record struct ArchiveFileEntry(
            int originalIndex,
            string fileName,
            int originalLength,
            int compressedLength,
            byte[] compressedData
            );
        public static void CreateArchive(IEnumerable<string> filePaths,
                                         string archivePath,
                                         string algorithm = "Huffman",
                                         CancellationToken token = default,
                                         ManualResetEventSlim? pauseEvent = null)
        {
            var files = filePaths.ToList();
            if (!files.Any())
                throw new ArgumentException("File list is empty.", nameof(filePaths));

            AlgorithmType algoType = algorithm == "Shannon-Fano" ? AlgorithmType.SHANNON : AlgorithmType.HUFFMAN;

            using var fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write((int)StorageType.ARCHIVE);
            bw.Write((int)algoType);
            bw.Write(files.Count);

            foreach (var path in files)
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();

                byte[] data = File.ReadAllBytes(path);
                byte[] comp = algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Compress(data, out _)
                    : new HuffmanCompressor().Compress(data, out _);
                bw.Write(Path.GetFileName(path));
                bw.Write(data.Length);
                bw.Write(comp.Length);
                bw.Write(comp);
            }
        }
        public async static Task CreateArchiveAsync(IEnumerable<string> filePaths,
                                 string archivePath,
                                 string algorithm = "Huffman",
                                 CancellationToken token = default,
                                 ManualResetEventSlim? pauseEvent = null)
        {
            var files = filePaths.ToList();
            if (!files.Any())
                throw new ArgumentException("File list is empty.", nameof(filePaths));

            AlgorithmType algoType = algorithm == "Shannon-Fano" ? AlgorithmType.SHANNON : AlgorithmType.HUFFMAN;

            var compressedFiles = new ConcurrentDictionary<int, (string Name, int OriginalLength, byte[] CompressedData)>();

            Parallel.ForEach(Enumerable.Range(0, files.Count), new ParallelOptions { CancellationToken = token }, i =>
            {
                pauseEvent?.Wait();
                token.ThrowIfCancellationRequested();

                string path = files[i];
                byte[] data = File.ReadAllBytes(path);

                byte[] comp = algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Compress(data, out _)
                    : new HuffmanCompressor().Compress(data, out _);

                compressedFiles[i] = (Path.GetFileName(path), data.Length, comp);
            });

            // Step 2: Write the archive sequentially
            using var fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            await bw.WriteAsync((int)StorageType.ARCHIVE);
            await bw.WriteAsync((int)algoType);
            await bw.WriteAsync(files.Count);

            for (int i = 0; i < files.Count; i++)
            {
                var (name, originalLen, compData) = compressedFiles[i];
                await bw.WriteAsync(name);
                await bw.WriteAsync(originalLen);
                await bw.WriteAsync(compData.Length);
                await bw.WriteAsync(compData);
            }
        }

        

        private static async Task<ArchiveFileEntry> ProcessFileForArchiveAsync(
            string path,
            string algorithm,
            CancellationToken token,
            ManualResetEventSlim? pauseEvent,
            int index = 0
            )
        {
            pauseEvent?.Wait(token);
            token.ThrowIfCancellationRequested();

            byte[] data = await File.ReadAllBytesAsync(path, token);

            byte[] comp = await Task.Run(() =>
            {
                return algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Compress(data, out _)
                    : new HuffmanCompressor().Compress(data, out _);
            }, token);

            return new ArchiveFileEntry(
                originalIndex: index,
                fileName: Path.GetFileName(path),
                originalLength: data.Length,
                compressedLength: comp.Length,
                compressedData: comp
            );
        }



        public static List<string> ListArchive(string archivePath)
        {
            using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            StorageType type = (StorageType)br.ReadInt32();

            if (type != StorageType.ARCHIVE)
            {
                throw new Exception();
            }

            br.ReadInt32();

            int count = br.ReadInt32();
            var names = new List<string>(capacity: count);

            for (int i = 0; i < count; i++)
            {
                string name = br.ReadString();
                int _orig = br.ReadInt32();
                int comp = br.ReadInt32();
                fs.Seek(comp, SeekOrigin.Current);
                names.Add(name);
            }
            return names;
        }

        public static void ExtractSingleFile(string archivePath,
                                             string targetName,
                                             string outputDir = "",
                                             CancellationToken token = default,
                                             ManualResetEventSlim? pEvent = null)
        {
            using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            StorageType type = (StorageType)br.ReadInt32();
            AlgorithmType algoType = (AlgorithmType)br.ReadInt32();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();
                pEvent?.Wait();

                string name = br.ReadString();
                int _orig = br.ReadInt32();
                int comp = br.ReadInt32();
                byte[] compData = br.ReadBytes(comp);

                if (!name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                byte[] data = algoType == AlgorithmType.SHANNON
                    ? new ShannonFanoCompressor().Decompress(compData, token, pEvent)
                    : new HuffmanCompressor().Decompress(compData, token, pEvent);

                string outDir = string.IsNullOrWhiteSpace(outputDir)
                                 ? Path.GetDirectoryName(archivePath)!
                                 : outputDir;
                string outPath = Path.Combine(outDir, name);

                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                File.WriteAllBytes(outPath, data);
                return;
            }

            throw new FileNotFoundException($"File '{targetName}' not found in archive.");
        }
        public static void ExtractAllFiles(
                                    string archivePath,
                                   string outputDir = "",
                                   CancellationToken token = default,
                                   ManualResetEventSlim? pEvent = null)
        {
            using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            StorageType type = (StorageType)br.ReadInt32();
            AlgorithmType algorithm = (AlgorithmType)br.ReadInt32();
            int count = br.ReadInt32();

            string baseOutputDir = string.IsNullOrWhiteSpace(outputDir)
                ? Path.GetDirectoryName(archivePath)!
                : outputDir;

            for (int i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();
                pEvent?.Wait();

                string name = br.ReadString();
                int _orig = br.ReadInt32();
                int comp = br.ReadInt32();
                byte[] compData = br.ReadBytes(comp);

                byte[] data = algorithm == AlgorithmType.SHANNON
                    ? new ShannonFanoCompressor().Decompress(compData, token, pEvent)
                    : new HuffmanCompressor().Decompress(compData, token, pEvent);

                string outPath = Path.Combine(baseOutputDir, name);
                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                File.WriteAllBytes(outPath, data);
            }
        }

        public static async Task ExtractAllFilesAsync(
            string archivePath,
            string outputDir = "",
            CancellationToken token = default,
            ManualResetEventSlim? pEvent = null)
        {
            List<(string Name, byte[] CompData)> files = new();

            AlgorithmType algorithm;

            using (var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                StorageType type = (StorageType)br.ReadInt32();
                algorithm = (AlgorithmType)br.ReadInt32();
                int count = br.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    pEvent?.Wait();

                    string name = br.ReadString();
                    int _orig = br.ReadInt32();
                    int comp = br.ReadInt32();
                    byte[] compData = br.ReadBytes(comp);

                    files.Add((name, compData));
                }
            }

            string baseOutputDir = string.IsNullOrWhiteSpace(outputDir)
                ? Path.GetDirectoryName(archivePath)!
                : outputDir;

            var tasks = files.Select(file => Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                pEvent?.Wait();

                byte[] data = algorithm == AlgorithmType.SHANNON
                    ? new ShannonFanoCompressor().Decompress(file.CompData, token, pEvent)
                    : new HuffmanCompressor().Decompress(file.CompData, token, pEvent);

                string outPath = Path.Combine(baseOutputDir, file.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                File.WriteAllBytes(outPath, data);

            }, token)).ToArray();

            await Task.WhenAll(tasks);
        }



    }
}
