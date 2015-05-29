using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Window_shopper_recommender
{
    /// <summary>
    /// Classe che permette di serializzare una lista di elementi della classe DataImg
    /// </summary>
    [Serializable()]
    public class ObjectToSerialize : ISerializable
    {
        public List<DataImg> OggettoDaSerializzare;

        public ObjectToSerialize(List<DataImg> _OggettoDaSerializzare)
        {
            this.OggettoDaSerializzare = _OggettoDaSerializzare;
        }

        public ObjectToSerialize()
        {
        }

        //parte aggiunta dopo aver deffinito la Iseriaalizable
        public ObjectToSerialize(SerializationInfo info, StreamingContext ctxt)
        {
            this.OggettoDaSerializzare = (List<DataImg>)info.GetValue("OggettoDaSerializzare", typeof(List<DataImg>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("OggettoDaSerializzare", this.OggettoDaSerializzare);
        }
    }
}
