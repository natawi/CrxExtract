using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CrxExtract
{
    internal class ExtensionFile
    {
        public readonly int Version;

        public readonly byte[] AuthorPublicKey;

        public readonly ZipArchive ZipData;

        public ExtensionFile(string filename)
        {
            using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));

            if (reader.BaseStream.Length < 12)
                throw new Exception("Invalid CRX file (invalid size)");

            char[] magic = { 'C', 'r', '2', '4' };
            if (!magic.SequenceEqual(reader.ReadChars(4)))
                throw new Exception("Invalid CRX file (missing magic)");

            Version = reader.ReadInt32();
            if (Version != 3)
                throw new Exception(string.Format("Unknown file version ({0})", Version));

            int keySize = reader.ReadInt32();
            if (keySize > reader.BaseStream.Length - reader.BaseStream.Position)
                throw new Exception("Invalid author public key length");

            AuthorPublicKey = reader.ReadBytes(keySize);

            var zipStream = new MemoryStream(reader.ReadBytes((int)reader.BaseStream.Length - (int)reader.BaseStream.Position));
            ZipData = new ZipArchive(zipStream, ZipArchiveMode.Read);
        }
    }
}
