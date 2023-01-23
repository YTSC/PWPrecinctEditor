using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComponentAce.Compression.Libs.zlib;

namespace PWPrecinctEditor
{
    //Part of Kn1fe code: https://github.com/Kn1fe/pPck
    //Modified
    public class fileTableEntry
    {
        public string filePath { get; set; }
        public string fullFilePath { get; set; }
        public uint fileDataOffset { get; set; }
        public int fileDataDecompressedSize { get; set; }
        public int fileDataCompressedSize { get; set; }
    }

    public static class PCKManager
    {
        public static int KEY_1 = -1466731422;
        public static int KEY_2 = -240896429;
        public static int ASIG_1 = -33685778;
        public static int ASIG_2 = -267534609;
        public static int FSIG_1 = 1305093103;
        public static int FSIG_2 = 1453361591;
        public static List<fileTableEntry> table;
        public static string path = Directory.GetCurrentDirectory() + @"\temp\";

        public static void LoadPck(string filepath)
        {
            Merge(ref filepath);
            if (!Directory.Exists(path))            
                Directory.CreateDirectory(path);            
            else            
                Directory.Delete(path, true);            
           
            BinaryReader br = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read));
            br.BaseStream.Seek(-8, SeekOrigin.End);
            int entryCount = br.ReadInt32();           
            br.BaseStream.Seek(-272, SeekOrigin.End);
            long fileTableOffset = (long)((ulong)((uint)(br.ReadUInt32() ^ (ulong)KEY_1)));           
            br.BaseStream.Seek(fileTableOffset, SeekOrigin.Begin);
            table = new List<fileTableEntry>();
            for (int a = 0; a < entryCount; ++a)
            {
                int entrySize = br.ReadInt32() ^ KEY_1;
                entrySize = br.ReadInt32() ^ KEY_2;
                byte[] buffer = new byte[entrySize];
                buffer = br.ReadBytes(entrySize);
                if (entrySize < 276)
                {
                    if(readTableEntry(buffer, true).filePath.Contains(@"surfaces\minimaps\"))
                        table.Add(readTableEntry(buffer, true));
                }
                else
                {
                    if (readTableEntry(buffer, true).filePath.Contains(@"surfaces\minimaps\"))
                        table.Add(readTableEntry(buffer, false));
                }
                //Console.WriteLine(string.Format("\rЧтение файловой таблицы: {0}/{1}", a, entryCount));               
            }
            int count = 0;
            foreach(fileTableEntry fileTable in table)
            {
                if(fileTable.fileDataOffset <= 0)
                {
                    Console.WriteLine("\nПлохой адрес файла: " + fileTable.fileDataOffset);
                }
                else
                {
                    CreateDir(path, fileTable.filePath);
                    byte[] buffer = new byte[fileTable.fileDataCompressedSize];
                    fileTable.fullFilePath = path + fileTable.filePath;
                    BinaryWriter bw = new BinaryWriter(new FileStream(fileTable.fullFilePath, FileMode.Create, FileAccess.Write));
                    br.BaseStream.Seek(fileTable.fileDataOffset, SeekOrigin.Begin);
                    buffer = br.ReadBytes(fileTable.fileDataCompressedSize);
                    if (fileTable.fileDataCompressedSize < fileTable.fileDataDecompressedSize)
                    {
                        MemoryStream ms = new MemoryStream();
                        ZOutputStream zos = new ZOutputStream(ms);
                        CopyStream(new MemoryStream(buffer), zos, fileTable.fileDataCompressedSize);
                        bw.Write(ms.ToArray());
                    }
                    else
                    {
                        bw.Write(buffer);
                    }
                    bw.Close();
                    Console.WriteLine(count++);
                }                
            }
            br.Close();
            if (filepath.EndsWith("x")) File.Delete(filepath);
           
        }
        public static fileTableEntry readTableEntry(byte[] buffer, bool compressed)
        {
            fileTableEntry fte = new fileTableEntry();
            MemoryStream ms = new MemoryStream(buffer);
            if (compressed)
            {
                byte[] buf = new byte[276];
                ZOutputStream zos = new ZOutputStream(new MemoryStream(buf));
                CopyStream(new MemoryStream(buffer), zos, 276);
                buffer = buf;
            }
            BinaryReader br = new BinaryReader(new MemoryStream(buffer));
            fte.filePath = Encoding.GetEncoding("GB2312").GetString(br.ReadBytes(260)).Split(new string[] { "\0" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("/", "\\");
            fte.fullFilePath = string.Empty;
            fte.fileDataOffset = br.ReadUInt32();
            fte.fileDataDecompressedSize = br.ReadInt32();
            fte.fileDataCompressedSize = br.ReadInt32();
            return fte;
        }

        public static void CopyStream(Stream input, Stream output, int Size)
        {
            try
            {
                byte[] buffer = new byte[Size];
                int len;
                while ((len = input.Read(buffer, 0, Size)) > 0)
                {
                    output.Write(buffer, 0, len);
                }
                output.Flush();
            }
            catch
            {
                Console.WriteLine("\nBad zlib data");
            }
        }

        public static void Merge(ref string pck)
        {
            if (new FileInfo(pck).Length < 2147483393)
            {
                string pkx = pck.Replace(".pck", ".pkx");
                if (File.Exists(pkx))
                {
                    Console.WriteLine("\nСоединяем pck и pkx в новый файл");
                    if (File.Exists(pck + "x")) File.Delete(pck + "x");
                    File.Copy(pck, pck + "x");
                    CopyStream(new FileStream(pkx, FileMode.Open, FileAccess.Read), new FileStream(pck + "x", FileMode.Open, FileAccess.Write), 134217728);
                    pck = pck + "x";
                }
            }
        }

        public static void CreateDir(string path, string subpath)
        {
            /*string[] subdirs = subpath.Split(new char[] { '\\' });
            foreach(string dir in subdirs)
            {
                path += "\\" + dir;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }*/

            string[] subdirs = subpath.Split(new char[] { '\\' });
            for (int a = 0; a < subdirs.Length - 1; ++a)
            {
                path += "\\" + subdirs[a];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

    }
}
