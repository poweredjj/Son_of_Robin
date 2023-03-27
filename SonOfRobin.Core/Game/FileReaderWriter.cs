using DouglasDwyer.PowerSerializer;
using System.IO;

namespace SonOfRobin
{
    public class FileReaderWriter
    {
        private static readonly PowerSerializer powerSerializer = new PowerSerializer();

        public static void SaveMemoryStream(MemoryStream memoryStream, string path)
        {
            FileStream outStream = File.OpenWrite(path);
            memoryStream.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }

        public static void Save(object savedObj, string path)
        {
            byte[] serializedData = powerSerializer.Serialize(savedObj);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                fileStream.Write(serializedData, 0, serializedData.Length);
            }
        }

        public static object Load(string path)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] serializedData = new byte[fileStream.Length];
                    fileStream.Read(serializedData, 0, (int)fileStream.Length);
                    return powerSerializer.Deserialize(serializedData);
                }
            }
            catch (System.Text.DecoderFallbackException)
            { return null; } // file corrupted
            catch (FileNotFoundException)
            { return null; }
            catch (DirectoryNotFoundException)
            { return null; }
        }
    }
}