using Newtonsoft.Json;
using System.IO;

namespace SonOfRobin
{
    public class FileReaderWriter
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
        };

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
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"IOException while trying to write {Path.GetFileName(path)}.");
                return false;
            }
        }

        public static void Save(object savedObj, string path)
        {
            string json = JsonConvert.SerializeObject(savedObj, serializerSettings);
            File.WriteAllText(path, json);
        }

        public static object Load(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject(json, serializerSettings);
            }
            catch (JsonReaderException) { return null; } // file corrupted
            catch (JsonSerializationException) { return null; } // file corrupted
            catch (System.Runtime.Serialization.SerializationException) { return null; } // file corrupted
            catch (System.Text.DecoderFallbackException) { return null; } // file corrupted
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }
    }
}