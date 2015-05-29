using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using System.Runtime.Remoting.Messaging;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Window_shopper_recommender
{
    /// <summary>
    /// Classe che si occupa della generazione e riempimento del file di testo contenente tutte le informazioni sulle features delle immagini del database.
    /// </summary>
    public class Serializer
    {
        public Serializer()
        {
            
        }

        public void SerializeObject(string filename, ObjectToSerialize objectToSerialize)
        {
            Stream stream = File.Open(filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, objectToSerialize);
            stream.Close();

        }

        //parte di deserializzazione aggiunta in questo progetto . perchè qui mi serve
        public ObjectToSerialize DeSerializeObject(string filename)
        {

            ObjectToSerialize objectToSerialize;
            Stream stream = File.Open(filename, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            objectToSerialize = (ObjectToSerialize)bFormatter.Deserialize(stream);
            stream.Flush();
            stream.Close();
            return objectToSerialize;
        }

    }
}
