using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SonOfRobin
{
    class FileReaderWriter
    {
        public static void Save(object savedObj, string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, savedObj);
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
                    BinaryFormatter bformatter = new BinaryFormatter();
                    return bformatter.Deserialize(stream);
                }
            }
            catch (System.Runtime.Serialization.SerializationException)
            { return null; } // file corrupted

            catch (FileNotFoundException)
            { return null; }

            catch (DirectoryNotFoundException)
            { return null; }
        }

    }
}
