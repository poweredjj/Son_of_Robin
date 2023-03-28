using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace SonOfRobin
{
    public class FileReaderWriter
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new[] { new StringEnumConverter() },
            Formatting = Formatting.Indented,
        };

        public static void SaveMemoryStream(MemoryStream memoryStream, string path)
        {
            FileStream outStream = File.OpenWrite(path);
            memoryStream.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }

        public static void Save(object savedObj, string path)
        {
            //using (Stream stream = File.Open(path, FileMode.Create))
            //{
            //    BinaryFormatter bformatter = new BinaryFormatter();
            //    bformatter.Serialize(stream, savedObj);
            //}

            string json = JsonConvert.SerializeObject(savedObj, serializerSettings);
            File.WriteAllText(path, json);
        }

        public static object Load(string path)
        {
            try
            {
                //using (Stream stream = File.Open(path, FileMode.Open))
                //{
                //    BinaryFormatter bformatter = new BinaryFormatter();
                //    return bformatter.Deserialize(stream);
                //}

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