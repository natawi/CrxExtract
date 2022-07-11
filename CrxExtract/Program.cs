using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;

namespace CrxExtract
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                Console.WriteLine("Extracting \"{0}\"...", arg);

                ExtensionFile file;
                try
                {
                    file = new ExtensionFile(arg);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occurred while reading the file: {0}", exception.Message);
                    continue;
                }

                string outDirectory = arg + "_out";

                ZipArchiveEntry manifest = file.ZipData.GetEntry("manifest.json");
                if (manifest != null)
                {
                    var serializer = JsonSerializer.Create();
                    using var reader = new JsonTextReader(new StreamReader(manifest.Open()));

                    var json = serializer.Deserialize<JToken>(reader);
                    if (json != null)
                    {
                        if (json["name"] != null)
                            outDirectory = Path.Combine(Path.GetDirectoryName(arg), ((string)json["name"]).Trim(Path.GetInvalidFileNameChars()));
                        else
                            Console.WriteLine("Manifest JSON is missing a 'name' field!");
                    }
                    else
                        Console.WriteLine("Manifest contains invalid JSON data!");
                }
                else
                    Console.WriteLine("Odd... there appears to be no manifest!", arg);

                Directory.CreateDirectory(outDirectory);

                foreach (ZipArchiveEntry entry in file.ZipData.Entries)
                {
                    string path = Path.Combine(outDirectory, entry.FullName);
                    if (path.EndsWith('\\') || path.EndsWith('/'))
                        Directory.CreateDirectory(path);
                    else
                        entry.ExtractToFile(path, true);
                } 
            }
        }
    }
}
