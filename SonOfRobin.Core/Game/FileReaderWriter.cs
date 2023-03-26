using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SonOfRobin
{
    internal class FileReaderWriter
    {
        public static void Save(object savedObj, string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                if (SonOfRobinGame.os == OS.Android)
                {
                    //Encoding.UTF8.GetBytes(JsonSerializer.Serialize(savedObj, GetJsonSerializerOptions())); // TODO make it work
                }
                else
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, savedObj);
                }
            }
        }

        public static void SaveMemoryStream(MemoryStream memoryStream, string path)
        {
            FileStream outStream = File.OpenWrite(path);
            memoryStream.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }

        public static object Load(string path)
        {
            try
            {
                using (Stream stream = File.Open(path, FileMode.Open))
                {

                    if (SonOfRobinGame.os == OS.Android)
                    {
                        // return JsonSerializer.Deserialize<object>(stream, GetJsonSerializerOptions()); // TODO make it work
                        return null;
                    }
                    else
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        return bformatter.Deserialize(stream);
                    }
                }
            }
            catch (System.Runtime.Serialization.SerializationException)
            { return null; } // file corrupted
            catch (System.Text.DecoderFallbackException)
            { return null; } // file corrupted
            catch (FileNotFoundException)
            { return null; }
            catch (DirectoryNotFoundException)
            { return null; }
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions()
            {
                PropertyNamingPolicy = null,
                WriteIndented = true,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
        }
    }
}