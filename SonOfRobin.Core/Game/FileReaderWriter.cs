using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;

namespace SonOfRobin
{
    public class FileReaderWriter
    {
        private static readonly JsonSerializerSettings serializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
        };

        public static void SaveJson(object savedObj, string path, bool compress)
        {
            string json = JsonConvert.SerializeObject(savedObj, serializerSettings);

            if (compress)
            {
                byte[] compressedBytes = Compress(json);
                try
                {
                    File.WriteAllBytes($"{path}.gzip", compressedBytes);
                }
                catch (IOException) { return; }
            }
            else
            {
                try
                {
                    File.WriteAllText(path, json);
                }
                catch (DirectoryNotFoundException)
                { }
            }
        }

        public static bool SaveMemoryStream(MemoryStream memoryStream, string path)
        {
            try
            {
                FileStream outStream = File.OpenWrite(path);
                memoryStream.WriteTo(outStream);
                outStream.Flush();
                outStream.Close();
                return true;
            }
            catch (IOException)
            {
                MessageLog.Add(debugMessage: true, text: $"IOException while trying to write {Path.GetFileName(path)}.");
                return false;
            }
        }

        public static byte[] LoadBytes(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }

        public static string LoadFile(string path)
        {
            using var stream = TitleContainer.OpenStream(path);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static object LoadJson(string path, bool useStreamReader = false)
        {
            string compressedPath = $"{path}.gzip";

            try
            {
                if (File.Exists(compressedPath))
                {
                    byte[] compressedBytes = File.ReadAllBytes(compressedPath);
                    string json = Decompress(compressedBytes);
                    if (json == "") return null;
                    return JsonConvert.DeserializeObject(json, serializerSettings);
                }
                else
                {
                    string json = useStreamReader ? LoadFile(path) : File.ReadAllText(path);
                    return JsonConvert.DeserializeObject(json, serializerSettings);
                }
            }
            catch (JsonReaderException) { return null; } // file corrupted
            catch (JsonSerializationException) { return null; } // file corrupted
            catch (System.Runtime.Serialization.SerializationException) { return null; } // file corrupted
            catch (System.Text.DecoderFallbackException) { return null; } // file corrupted
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }

        private static byte[] Compress(string str)
        {
            using (MemoryStream ms = new())
            {
                using (GZipStream gzip = new(ms, CompressionMode.Compress, true))
                using (StreamWriter writer = new(gzip))
                {
                    writer.Write(str);
                }
                return ms.ToArray();
            }
        }

        private static string Decompress(byte[] compressedBytes)
        {
            using (MemoryStream ms = new(compressedBytes))
            {
                using (GZipStream gzip = new(ms, CompressionMode.Decompress))
                using (StreamReader reader = new(gzip))
                {
                    try
                    {
                        return reader.ReadToEnd();
                    }
                    catch (InvalidDataException) { return ""; } // file not compressed or damaged
                }
            }
        }

        public static bool PathExists(string path)
        {
            // takes compression (filename change) into account

            if (File.Exists($"{path}.gzip")) return true;
            return File.Exists(path);
        }
    }
}