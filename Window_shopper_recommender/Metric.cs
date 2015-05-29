using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Window_shopper_recommender
{
    /// <summary>
    /// Classe che serve per salvare il risultato del confronto fra immagini di test e immagini del database e mantenere traccia delle directory delle immagini di database.
    /// Tali informazioni risulteranno utili nella fase di suggerimento.
    /// </summary>
    public class Metric
    {

        public double resultBGRA;
        public string DirectoryOfImg;

        public Metric()
        {

        }

        public Metric(double _resultBGRA, string _dir)
        {
            this.resultBGRA = _resultBGRA;
            this.DirectoryOfImg = _dir;
        }
    }
}
