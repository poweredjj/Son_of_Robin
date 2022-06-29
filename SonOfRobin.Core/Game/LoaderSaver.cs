using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SonOfRobin
{
    class LoaderSaver
    {
        public static void Save(object savedObj, string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, savedObj);
            }
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
            catch(System.Runtime.Serialization.SerializationException)
            { return null; } // file corrupted

            catch (FileNotFoundException)
            { return null; }

            catch (DirectoryNotFoundException)
            { return null; }
        }

    }
}
