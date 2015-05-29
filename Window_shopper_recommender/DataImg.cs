using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;


namespace Window_shopper_recommender
{
   /// <summary>
    /// Classe che permette di raccogliere al suo interno le features delle immagini del database:istogramma RGB, directory di provenienza, tipologia merceologica.
   /// </summary>
   [Serializable()]
    public class DataImg : ISerializable
    {
        
        Emgu.CV.DenseHistogram histBGRA;
        public string TMerc; //corrisponde al tag delle mie immagini
        public string theDirectory;

        public DataImg(DenseHistogram _histBGRA,string _TMerc,string _dir)
        {
            this.histBGRA = _histBGRA;
            this.TMerc = _TMerc;
            this.theDirectory = _dir;
        }

        //parameterless constructor
        public DataImg() { }

        public DenseHistogram getBGRAHist()
        {
            return this.histBGRA;
        }

       //parte aggiunta dopo la Iserializable
        public DataImg(SerializationInfo info, StreamingContext ctxt)
        {
            this.histBGRA = (DenseHistogram)info.GetValue("histBGRA", typeof(DenseHistogram));
            this.TMerc = (string)info.GetValue("TMerc", typeof(string));
            this.theDirectory = (string)info.GetValue("theDirectory", typeof(string));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("histBGRA", this.histBGRA);
            info.AddValue("TMerc", this.TMerc);
            info.AddValue("theDirectory", this.theDirectory);
        }
    }
}