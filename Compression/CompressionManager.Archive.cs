using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace WinFormsApp1.Compression
{
    public static partial class CompressionManager
    {
        // 1) إنشاء أرشيف واحد يضم عدّة ملفات
        public static void CreateArchive(IEnumerable<string> filePaths,
                                         string archivePath,
                                         string algorithm = "Huffman",
                                         CancellationToken token   = default,
                                         ManualResetEventSlim? pauseEvent = null)
        {
            var files = filePaths.ToList();
            if (!files.Any())
                throw new ArgumentException("File list is empty.", nameof(filePaths));

            using var fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write(files.Count);

            foreach (var path in files)
            {
                pauseEvent?.Wait();            // إيقاف مؤقّت إن لزم
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

        // 2) جلب أسماء الملفات داخل الأرشيف
        public static List<string> ListArchive(string archivePath)
        {
            using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            int count = br.ReadInt32();
            var names = new List<string>(capacity: count);

            for (int i = 0; i < count; i++)
            {
                string name   = br.ReadString();
                int    _orig  = br.ReadInt32();
                int    comp   = br.ReadInt32();
                fs.Seek(comp, SeekOrigin.Current);
                names.Add(name);
            }
            return names;
        }

        // 3) استخراج ملف واحد من الأرشيف
        public static void ExtractSingleFile(string archivePath,
                                             string targetName,
                                             string algorithm  = "Huffman",
                                             string outputDir  = "",
                                             CancellationToken token = default)
        {
            using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();

                string name   = br.ReadString();
                int    _orig  = br.ReadInt32();
                int    comp   = br.ReadInt32();
                byte[] compData = br.ReadBytes(comp);

                if (!name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                byte[] data = algorithm == "Shannon-Fano"
                    ? new ShannonFanoCompressor().Decompress(compData)
                    : new HuffmanCompressor().Decompress(compData);

                string outDir  = string.IsNullOrWhiteSpace(outputDir)
                                 ? Path.GetDirectoryName(archivePath)!
                                 : outputDir;
                string outPath = Path.Combine(outDir, name);

                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                File.WriteAllBytes(outPath, data);
                return;
            }

            throw new FileNotFoundException($"File '{targetName}' not found in archive.");
        }
    }
}
