using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows;
using com.drew.metadata;
using com.drew.imaging.jpg;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;



using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;


using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;



namespace Window_shopper_recommender
{
    /// <summary>
    /// Classe che contiene al proprio interno le funzioni che servono a generare un suggerimento completo e le routine dei pulsanti.
    /// </summary>
    public partial class Form1 : Form
    {
        public List<DataImg> lista1 = new List<DataImg>();
        public string OurIMGDirectory;
        public string DatabasePath = "DATABASE";
        public int NImgsToDisplay = 20;
        public HaarCascade haar; //viola jones classifier

        public Form1()
        {
            Panel my_panel = new Panel();
            VScrollBar vScroller = new VScrollBar();
            vScroller.Dock = DockStyle.Right;
            vScroller.Width = 30;
            vScroller.Height = 200;
            vScroller.Name = "VScrollBar1";
            my_panel.Controls.Add(vScroller);
            InitializeComponent();
            this.tabControl1.SizeMode = TabSizeMode.Fixed;

            ((Control)this.tabPage6).Enabled = false;
            ((Control)this.tabPage1).Enabled = false;
            ((Control)this.tabPage2).Enabled = false;
            ((Control)this.tabPage3).Enabled = false;
        }

/*******************************************************************************************************************************************************************/
/**************************************************** FUNZIONI *******************************************************************************************************/
/******************************************************************************************************************************************************************/

        /*Questa funzione calcola l'istogramma nei soli canali B,G,R di un immagine BGRA passata in ingresso.
         *Ogni immagine viene scansionata per vedere quali sono i pixel trasparenti, per i quali non sarà necessario calcolare l'istogramma.
         *L'istogramma quindi è calcolato solamente sulla parte di immagine in cui è presente il singolo indumento.*/
        //------- tale commento non si trova nella /doc in quanto Doxygen non riconosceva il tipo ritornato Emgu.CV.DenseHistogram -------------------------//
        DenseHistogram ComputeHistoBGRA(Image<Bgra, byte> _img1)
        {
            DenseHistogram Total = new DenseHistogram(new int[] { 20, 20, 20 }, new RangeF[] { new RangeF(0, 255), new RangeF(0, 255), new RangeF(0, 255) });
            Image<Gray, byte>[] ArrayOfImages = new Image<Gray, byte>[3];
            ArrayOfImages = _img1.Split();
            int height = _img1.Height;
            int width = _img1.Width;
            Image<Gray, byte> mask = new Image<Gray, byte>(width, height);
            for (int h = 0; h <= _img1.Rows - 1; h++)
            {
                for (int l = 0; l <= _img1.Cols - 1; l++)
                {
                    if ((_img1[h, l].Alpha == 0))
                    {
                        mask[h, l] = new Gray(0);
                    }
                    else
                    {
                        mask[h, l] = new Gray(255);
                    }
                }
            }
            Total.Calculate(ArrayOfImages, false, mask);
            Total.Normalize(1);
            return Total;
        }


        /**************************************************************************************************************************************************/
        /// <summary>
        /// Esegue la comparazione degli istogrammi nei canali B,G,R. Il metodo di comparazione utilizzato è il CV_COMP_BHATTACHARYYA già implementato nelle EMGUCv.
        /// La funzione prende in ingresso l'istogramma relativo ad un immagine del database e un istogramma relativo ad una sezione di corpo dell'utente,
        /// restituisce una metrica di somiglianza in formato double.
        /// </summary>
        /// <param name="_TotalHistDatabaseImg"></param>
        /// <param name="_TotalHistOurImg"></param>
        /// <returns>result</returns>
        public double CompareHistBGRA(DenseHistogram _TotalHistDatabaseImg, DenseHistogram _TotalHistOurImg)
        {
            //Comparing histograms
            double result = CvInvoke.cvCompareHist(_TotalHistOurImg.Ptr, _TotalHistDatabaseImg.Ptr, Emgu.CV.CvEnum.HISTOGRAM_COMP_METHOD.CV_COMP_BHATTACHARYYA);

            return result;
        }

        /*******************************************************************************************************************************************/
       
        /// <summary>
        /// Prendendo in input il nome della sottocartella appartenente al DATABASE-donna, genera una stringa che attribuisce un tag ad ogni immagine 
        /// basato sulla cartella di appartenenza. In questo modo sarà possibile capire a quale tipologia merceologica appartiene ciascuna immagine.
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns>tags</returns>
        public string GeneraTagsCartellaDonna(string strFileName)
        {
            string tags = "null";
            if (strFileName == "Borse") { tags = "Donna;Borse"; };
            if (strFileName == "Camicie") { tags = "Donna;Camicie"; };
            if (strFileName == "Cappelli") { tags = "Donna;Cappelli"; };
            if (strFileName == "Cinture") { tags = "Donna;Cinture"; };
            if (strFileName == "Foulard e Sciarpe") { tags = "Donna;Foulard e sciarpe"; };
            if (strFileName == "Gioielli") { tags = "Donna;Gioielli"; };
            if (strFileName == "Gonne") { tags = "Donna;Gonne"; };
            if (strFileName == "Guanti") { tags = "Donna;Guanti"; };
            if (strFileName == "Jeans") { tags = "Donna;Jeans"; };
            if (strFileName == "Maglieria e Felpe") { tags = "Donna;Maglieria e felpe"; };
            if (strFileName == "Pantaloni") { tags = "Donna;Pantaloni"; };
            if (strFileName == "Scarpe") { tags = "Donna;Scarpe"; };
            if (strFileName == "T-shirt e Top") { tags = "Donna;T-shirt e top"; };
            if (strFileName == "Vestiti") { tags = "Donna;Vestiti"; };
            if (strFileName == "Giacche") { tags = "Donna;Giacche"; };

            return tags;
        }
        /*******************************************************************************************************************************************/
        /// <summary>
        /// Prendendo in input il nome della sottocartella appartenente al DATABASE-uomo, genera una stringa che attribuisce un tag ad ogni immagine 
        /// basato sulla cartella di appartenenza. In questo modo sarà possibile capire a quale tipologia merceologica appartiene ciascuna immagine.
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns>tags</returns>
        public string GeneraTagsCartellaUomo(string strFileName)
        {
            string tags = "null";
            if (strFileName == "Cappelli e Occhiali") { tags = "Uomo;Cappelli e Occhiali"; };
            if (strFileName == "Sciarpe e Accessori") { tags = "Uomo;Sciarpe e Accessori"; };
            if (strFileName == "Camicie") { tags = "Uomo;Camicie"; };
            if (strFileName == "Completi e Cravatte") { tags = "Uomo;Completi e cravatte"; };
            if (strFileName == "Giacche") { tags = "Uomo;Giacche"; };
            if (strFileName == "Jeans") { tags = "Uomo;Jeans"; };
            if (strFileName == "Maglieria e Felpe") { tags = "Uomo;Maglierie e felpe"; };
            if (strFileName == "Pantaloni") { tags = "Uomo;Pantaloni"; };
            if (strFileName == "Scarpe") { tags = "Uomo;Scarpe"; };
            if (strFileName == "T-shirt e Polo") { tags = "Uomo;T-Shirt e polo"; };

            return tags;
        }

        /*******************************************************************************************************************************************/
        /// <summary>
        /// Prendendo in input il nome della sottocartella appartenente al DATABASE-bimbi, genera una stringa che attribuisce un tag ad ogni immagine 
        /// basato sulla cartella di appartenenza. In questo modo sarà possibile capire a quale tipologia merceologica appartiene ciascuna immagine.
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns>tags</returns>
        public string GeneraTagsCartellaBambini(string strFileName)
        {
            string tags = "null";
            if (strFileName == "Bimbo") { tags = "Bimbo"; };
            if (strFileName == "Bimba") { tags = "Bimba"; };
            return tags;
        }

        /******************************************************************************************************************************************************/
        /// <summary>
        /// Riempie una lista di DataImg si tratta di immagini del DATABASE-uomo potenzialmente suggeribili in base alla parte del corpo che si sta analizzando Es: 0= parte alta, 1=collo, 2=busto, 3=gambe, 4=piedi
        /// Per ogni parte del corpo dell'utente la funzione estrarrà, dalla lista totale di immagini del database, solamente le immagini che corrispondono al vestiario suggeribile per quella determinata zona del corpo.
        /// ES: parte 0=testa -> estrarrà solo Sciarpe ed accessori
        /// </summary>
        /// <param name="k"></param>
        /// <returns>ListaSesso</returns>
        public List<DataImg> RiempiListaSessoUomo(int k)
        {

            List<DataImg> ListaSesso = new List<DataImg>();

            if (k == 0)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Cappelli e Occhiali"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if (k == 1)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Sciarpe e Accessori"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if (k == 2)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Camicie") || (lista1[i].TMerc == "Uomo;Giacche") || (lista1[i].TMerc == "Uomo;Maglierie e felpe") || (lista1[i].TMerc == "Uomo;T-Shirt e polo"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if (k == 3)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Pantaloni") || (lista1[i].TMerc == "Uomo;Jeans") || (lista1[i].TMerc == "Uomo;Completi e cravatte"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if (k == 4)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if (lista1[i].TMerc == "Uomo;Scarpe")
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if ((k != 0) && (k != 1) && (k != 2) && (k != 3) && (k != 4))
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Scarpe") || (lista1[i].TMerc == "Uomo;Pantaloni") || (lista1[i].TMerc == "Uomo;T-Shirt e polo"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            return ListaSesso;
        }

        /******************************************************************************************************************************************************/
        /*Riempie una lista di DataImg di immagini del database potenzialmente suggeribili in base alla parte del corpo che si sta analizzando
         es: 0= parte alta, 1=collo, 2=busto, 3=gambe, 4=piedi*/
        /******************************************************************************************************************************************************/
        /// <summary>
        /// Riempie una lista di DataImg, si tratta di immagini del DATABASE-donna potenzialmente suggeribili in base alla parte del corpo che si sta analizzando Es: 0= parte alta, 1=collo, 2=busto, 3=gambe, 4=piedi
        /// Per ogni parte del corpo dell'utente la funzione estrarrà, dalla lista totale di immagini del database, solamente le immagini che corrispondono al vestiario suggeribile per quella determinata zona del corpo.
        /// ES: parte 0=testa -> estrarrà solo Sciarpe,Foulard e Gioielli.
        /// </summary>
        /// <param name="k"></param>
        /// <returns>ListaSesso</returns>
        public List<DataImg> RiempiListaSessoDonna(int k)
        {

            List<DataImg> ListaSesso = new List<DataImg>();

            if (k == 0)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Cappelli") || (lista1[i].TMerc == "Donna;Guanti"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };

            if (k == 1)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Foulard e sciarpe") || (lista1[i].TMerc == "Donna;Gioielli"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if (k == 2)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Camicie") || (lista1[i].TMerc == "Donna;Giacche") || (lista1[i].TMerc == "Donna;Maglieria e felpe") || (lista1[i].TMerc == "Donna;T-shirt e top"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if (k == 3)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Pantaloni") || (lista1[i].TMerc == "Donna;Jeans") || (lista1[i].TMerc == "Donna;Vestiti") || (lista1[i].TMerc == "Donna;Gonne"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if (k == 4)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Scarpe") || (lista1[i].TMerc == "Donna;Borse") || (lista1[i].TMerc == "Donna;Cinture"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            if ((k != 0) && (k != 1) && (k != 2) && (k != 3) && (k != 4))
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Scarpe") || (lista1[i].TMerc == "Donna;Pantaloni") || (lista1[i].TMerc == "Donna;T-shirt e top"))
                    {
                        ListaSesso.Add(lista1[i]);
                    };
                }
            };
            return ListaSesso;
        }

        /*******************************************************************************************************************************************/
        /*In base a cosa sta guardando la persona riempie una lista di articoli simili presi dal database ****************************************************/
        /// <summary>
        /// In base a quali articoli sta guardando la persona davanti alla vetrina, la funzione riempie una lista di vestiti che appartengono alla stessa gamma di prodotti
        /// e li inserisce in una lista. Viene previsto anche il caso di nulla guardato -> verranno inseriti nella lista solamente capi relativi all'abbigliamento da
        /// donna in quanto si suppone che sia più probabile che una donna si fermi a guardare una vetrina.
        /// </summary>
        /// <returns>ListaArticolo</returns>
        public List<DataImg> RiempiListaArticoli()
        {
            List<DataImg> ListaArticolo = new List<DataImg>();
            //COMPLETI DA UOMO E CAMICIE guardati
            if (CBUomoElegante.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Completi e cravatte") || (lista1[i].TMerc == "Uomo;Camicie"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //PARTE MEDIA UOMO guardati
            if (CBParteMediaUomo.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Maglierie e felpe") || (lista1[i].TMerc == "Uomo;T-Shirt e polo"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //PARTE BASSA UOMO guardati
            if (CBParteBassaUomo.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Jeans") || (lista1[i].TMerc == "Uomo;Pantaloni"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //SCARPE UOMO guardati
            if (CBScarpeUomo.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Scarpe"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //ACCESSORI UOMO guardati
            if (CBAccessoriUomo.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Cappelli e Occhiali") || (lista1[i].TMerc == "Uomo;Sciarpe e Accessori"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //GIACCHE UOMO guardati
            if (CBGiaccheUomo.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Uomo;Giacche"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //BORSE E CINTURE DONNA  guardati
            if (CBBorseCintureDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Borse") || (lista1[i].TMerc == "Donna;Cinture"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //GIOIELLI E FOULARD DONNA  guardati
            if (CBGioielliFoulardDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Foulard e sciarpe") || (lista1[i].TMerc == "Donna;Gioielli"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //CAPPELLI E GUANTI DONNA  guardati
            if (CBCappelliGuantiDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Cappelli") || (lista1[i].TMerc == "Donna;Guanti"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //VESTITI DONNA  guardati
            if (CBVestitiDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Vestiti"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //PARTE MEDIA DONNA  guardati
            if (CBParteMediaDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Maglieria e felpe") || (lista1[i].TMerc == "Donna;T-shirt e top"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //PARTE BASSA DONNA  guardati
            if (CBParteBassaDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Gonne") || (lista1[i].TMerc == "Donna;Pantaloni") || (lista1[i].TMerc == "Donna;Jeans"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //GIACCHE DONNA  guardati
            if (CBGiaccheDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Giacche"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //SCARPE DONNA  guardati
            if (CBScarpeDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Scarpe"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //CAMICIE DONNA  guardati
            if (CBCamicieDonna.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Camicie"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //CAPI BIMBO guardati
            if (CBBimbo.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Bimbo"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //CAPI BIMBA guardati
            if (CBBimba.Checked)
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Bimba"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            //NULLA GUARDATO =nessun checkbox schiacciato= non rileva dove sta guardando
            if ((CBUomoElegante.Checked == false) && (CBParteMediaUomo.Checked == false) && (CBParteBassaUomo.Checked == false) && (CBScarpeUomo.Checked == false) && (CBAccessoriUomo.Checked == false) && (CBGiaccheUomo.Checked == false) &&
                (CBBorseCintureDonna.Checked == false) && (CBGioielliFoulardDonna.Checked == false) && (CBCappelliGuantiDonna.Checked == false) && (CBVestitiDonna.Checked == false) && (CBParteMediaDonna.Checked == false) && (CBParteBassaDonna.Checked == false) && (CBGiaccheDonna.Checked == false) && (CBScarpeDonna.Checked == false) && (CBCamicieDonna.Checked == false) &&
                (CBBimbo.Checked == false) && (CBBimba.Checked == false))
            {
                for (int i = 0; i < lista1.Count - 1; i++)
                {
                    if ((lista1[i].TMerc == "Donna;Gonne") || (lista1[i].TMerc == "Donna;Scarpe") || (lista1[i].TMerc == "Donna;T-shirt e top"))
                    {
                        ListaArticolo.Add(lista1[i]);
                    };
                }
            };
            return ListaArticolo;
        }

        /***************************************************************************************************************************************************/
        /// <summary>
        /// Ordina una lista di immagini che hanno già subito la comparazione degli istogrammi in base ai colori indossati dall'utente. 
        /// L' ordinamento avviene in ordine crescente in quanto le immagini che hanno metrica di somiglianza più piccola saranno le più simili rispetto al colore indossato dell'utente.
        /// </summary>
        /// <param name="lista"></param>
        /// <returns>SortedListCol</returns>
        public List<Metric> OrdinaPerColoreBGR(List<Metric> lista)
        {
            List<Metric> SortedListCol = new List<Metric>();
            SortedListCol = lista.OrderBy(o => o.resultBGRA).ToList();
            return SortedListCol;
        }


        /**************************************************************************************************************************************************************/
        /// <summary>
        /// Genera il display delle immagini da suggerire. Le immagini possono comparire una sola volta all'interno di un singolo suggerimento.
        /// Viene data maggiore importanze nella visualizzazione ai capi riguardanti la zona del busto e delle gambe. 
        /// </summary>
        /// <param name="_ArraydiMetricheSex"></param>
        /// <param name="_ListaArticolo"></param>
        /// <param name="_ArrayMetricheSordedSex"></param>
        /// <param name="_ArrayMetricheArticoloSorted"></param>
        /// <param name="NImgForGenderCol"></param>
        /// <param name="NImgForGenderNONCol"></param>
        /// <param name="NImgForArticleCol"></param>
        /// <param name="NImgForArticleNONCol"></param>
        public void DecidiCosaVisualizzare(List<Metric>[] _ArraydiMetricheSex, List<DataImg> _ListaArticolo, List<Metric>[] _ArrayMetricheSordedSex, List<Metric>[] _ArrayMetricheArticoloSorted, int NImgForGenderCol, int NImgForGenderNONCol, int NImgForArticleCol, int NImgForArticleNONCol)
        {

            flowLayoutPanel1.Controls.Clear();
            this.flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel2.Controls.Clear();
            this.flowLayoutPanel2.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel3.Controls.Clear();
            this.flowLayoutPanel3.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel4.Controls.Clear();
            this.flowLayoutPanel4.FlowDirection = FlowDirection.TopDown;
            PictureBox[] Suggerimenti = new PictureBox[NImgsToDisplay];
            List<Metric> AlreadyShow = new List<Metric>();
            int v = 0, w = 0, x = 0, y = 0, z = 0;// sesso colorate
            int k = 0;//sesso random
            int l = 0, m = 0, n = 0, o = 0, p = 0, h = 0;//articolo colorate
            for (int i = 0; i <= NImgsToDisplay - 1; i++)
            {
                Suggerimenti[i] = new PictureBox();
                Suggerimenti[i].Name = "ItemNum_" + i.ToString();
                Suggerimenti[i].BackColor = System.Drawing.Color.Transparent;
                Suggerimenti[i].Size = new Size(100, 100);
                Suggerimenti[i].SizeMode = PictureBoxSizeMode.Zoom;
                Suggerimenti[i].Visible = true;

                Suggerimenti[i].MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
               
                //display immagini relative al genere, che hanno somiglianza in base ai colori indossati in ogni parte del corpo
                if (i <= NImgForGenderCol - 1)
                {
                    if ((i == 0) || (i % 5 == 0))
                    {
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[2][v].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[2][v]);
                        v = v + 1;
                    };

                    if ((i == 1) || ((i - 1) % 5 == 0))
                    {
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[3][w].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[3][w]);
                        w = w + 1;
                    };
                    if ((i == 2) || ((i - 2) % 5 == 0))
                    {
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[0][x].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[0][x]);
                        x = x + 1;
                    };
                    if ((i == 3) || ((i - 3) % 5 == 0))
                    {
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[1][y].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[1][y]);
                        y = y + 1;
                    };
                    if ((i == 4) || ((i - 4) % 5 == 0))
                    {
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[4][z].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[4][z]);
                        z = z + 1;
                    };
                    flowLayoutPanel1.Controls.Add(Suggerimenti[i]);
                };

                //display immagini in base al genere che non hanno corrispondenza in base ai colori indossati dall'utente
                if ((i <= NImgForGenderCol + NImgForGenderNONCol - 1) && (i > NImgForGenderCol - 1))
                {
                    Random rnd = new Random();
                    if ((k == 0) || (k % 5 == 0))
                    {
                        int qu;
                        bool trovato = false;
                        do
                        {
                            qu = rnd.Next(0, (_ArrayMetricheSordedSex[2].Count - 1));
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheSordedSex[2][qu].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[2][qu].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[2][qu]);
                    };
                    if ((k == 1) || ((k - 1) % 5 == 0))
                    {
                        int qu;
                        bool trovato = false;
                        do
                        {
                            qu = rnd.Next(0, (_ArrayMetricheSordedSex[3].Count - 1));
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheSordedSex[3][qu].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[3][qu].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[3][qu]);
                    };
                    if ((k == 2) || ((k - 2) % 5 == 0))
                    {
                        int qu;
                        bool trovato = false;
                        do
                        {
                            qu = rnd.Next(0, (_ArrayMetricheSordedSex[0].Count - 1));
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheSordedSex[0][qu].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[0][qu].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[0][qu]);
                    };
                    if ((k == 3) || ((k - 3) % 5 == 0))
                    {
                        int qu;
                        bool trovato = false;
                        do
                        {
                            qu = rnd.Next(0, (_ArrayMetricheSordedSex[1].Count - 1));
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheSordedSex[1][qu].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[1][qu].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[1][qu]);
                    };
                    if ((k == 4) || ((k - 4) % 5 == 0))
                    {
                        int qu;
                        bool trovato = false;
                        do
                        {
                            qu = rnd.Next(0, (_ArrayMetricheSordedSex[4].Count - 1));
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheSordedSex[4][qu].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheSordedSex[4][qu].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheSordedSex[4][qu]);
                    };
                    k = k + 1;
                    flowLayoutPanel3.Controls.Add(Suggerimenti[i]);
                };

                //display immagini relative alla gamma di prodotti osservati, che hanno corrispondenza in base al colore indossato nelle varie zone del corpo dall'utente
                if ((i <= NImgForGenderCol + NImgForGenderNONCol + NImgForArticleCol - 1) && (i > NImgForGenderCol + NImgForGenderNONCol - 1))
                {
                    //ti consiglio un articolo che stai guardando simile alla parte media che indossi
                    if ((h == 0) || (h % 5 == 0))
                    {
                        bool trovato = false;
                        do
                        {
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheArticoloSorted[2][l].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    l = l + 1;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheArticoloSorted[2][l].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheArticoloSorted[2][l]);
                        l = l + 1;
                    };
                    //ti consiglio un articolo che stai guardando simile alla parte delle gambe che indossi
                    if ((h == 1) || ((h - 1) % 5 == 0))
                    {
                        bool trovato = false;
                        do
                        {
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheArticoloSorted[3][m].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    m = m + 1;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheArticoloSorted[3][m].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheArticoloSorted[3][m]);
                        m = m + 1;

                    };
                    //ti consiglio un articolo che stai guardando simile alla parte  alta della testa che indossi
                    if ((h == 2) || ((h - 2) % 5 == 0))
                    {
                        bool trovato = false;
                        do
                        {
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheArticoloSorted[0][n].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    n = n + 1;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheArticoloSorted[0][n].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheArticoloSorted[0][n]);
                        n = n + 1;
                    };
                    //ti consiglio un articolo che stai guardando simile alla parte del collo che indossi
                    if ((h == 3) || ((h - 3) % 5 == 0))
                    {
                        bool trovato = false;
                        do
                        {
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheArticoloSorted[1][o].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    o = o + 1;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheArticoloSorted[1][o].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheArticoloSorted[1][o]);
                        o = o + 1;
                    };
                    //ti consiglio un articolo che stai guardando simile alla parte dei piedi che indossi
                    if ((h == 4) || ((h - 4) % 5 == 0))
                    {
                        bool trovato = false;
                        do
                        {
                            for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                            {
                                if (_ArrayMetricheArticoloSorted[4][p].DirectoryOfImg == AlreadyShow[gg].DirectoryOfImg)
                                {
                                    trovato = true;
                                    p = p + 1;
                                    break;
                                }
                                else
                                {
                                    trovato = false;
                                };
                            }
                        } while (trovato == true);
                        Suggerimenti[i].Load(_ArrayMetricheArticoloSorted[4][p].DirectoryOfImg);
                        AlreadyShow.Add(_ArrayMetricheArticoloSorted[4][p]);
                        p = p + 1;
                    };
                    flowLayoutPanel2.Controls.Add(Suggerimenti[i]);
                    h = h + 1;
                };

                //display immagini relative alla gamma di prodotti osservata, non relazionati ai colori indossati dall'utente
                if ((i <= NImgForGenderCol + NImgForGenderNONCol + NImgForArticleCol + NImgForArticleNONCol - 1) && (i > NImgForGenderCol + NImgForGenderNONCol + NImgForArticleCol - 1))
                {
                    Random rnd = new Random();
                    int qu;
                    if (AlreadyShow.Count == 0)
                    {
                        for (int b = 0; b <= NImgForArticleNONCol; b++)
                        {
                            qu = rnd.Next(0, (_ListaArticolo.Count - 1));
                            Suggerimenti[i].Load(_ListaArticolo[qu].theDirectory);
                        }
                    };
                    bool trovato = false;
                    do
                    {
                        qu = rnd.Next(0, (_ListaArticolo.Count - 1));
                        for (int gg = 0; gg <= AlreadyShow.Count - 1; gg++)
                        {
                            if (_ListaArticolo[qu].theDirectory == AlreadyShow[gg].DirectoryOfImg)
                            {
                                trovato = true;
                                break;
                            }
                            else
                            {
                                trovato = false;
                                Suggerimenti[i].Load(_ListaArticolo[qu].theDirectory);
                            };
                        }
                    } while (trovato == true);
                    flowLayoutPanel4.Controls.Add(Suggerimenti[i]);
                };
            }
        }

        /********************************************************************************************************************************************/
        /// <summary>
        /// Segmenta l'utente donna, senza pelle, in 5 aree prefissate: 0=sopra la testa; 1=collo; 2=busto; 3=gambe; 4=piedi.
        /// Per ogniuno di questi segmenti traccia un rettangolo colorato che li indica nell'immagine originale.
        /// Le immagini di ogni segmento e l'immagine con i rettangoli vengono salvate nella cartella del debug dell'applicazione.
        /// </summary>
        /// <param name="ImgDaSegmentare"></param>
        /// <param name="ImgRettangoli"></param>
        /// <returns>OurImgBGRAArray</returns>
        public Image<Bgra, byte>[] SegmentaDonna(Image<Bgra, byte> ImgDaSegmentare, Image<Bgra, byte> ImgRettangoli)
        {

            Image<Bgra, byte>[] OurImgBGRAArray = new Image<Bgra, byte>[5];
            Image<Bgra, byte> SubOurImgBGRA0;
            Image<Bgra, byte> SubOurImgBGRA1;
            Image<Bgra, byte> SubOurImgBGRA2;
            Image<Bgra, byte> SubOurImgBGRA3;
            Image<Bgra, byte> SubOurImgBGRA4;

            Rectangle rett0 = new Rectangle(147, 45, 87, 45);
            Rectangle rett1 = new Rectangle(157, 127, 77, 34);
            Rectangle rett2 = new Rectangle(127, 180, 140, 90);
            Rectangle rett3 = new Rectangle(138, 295, 136, 169);
            Rectangle rett4 = new Rectangle(148, 467, 128, 51);
            /////////calcola i retangoli da tagliare
            SubOurImgBGRA0 = ImgDaSegmentare.GetSubRect(rett0);
            SubOurImgBGRA0.Save("SubRectangleTesta_SenzaPelle.png");
            ImgRettangoli.Draw(rett0, new Bgra(0, 255, 255,255), 3);

            SubOurImgBGRA1 = ImgDaSegmentare.GetSubRect(rett1);
            SubOurImgBGRA1.Save("SubRectangleCollo_SenzaPelle.png");
            ImgRettangoli.Draw(rett1, new Bgra(255, 255, 0, 255), 3);

            SubOurImgBGRA2 = ImgDaSegmentare.GetSubRect(rett2);
            SubOurImgBGRA2.Save("SubRectanglePetto_SenzaPelle.png");
            ImgRettangoli.Draw(rett2, new Bgra(0, 255, 0, 255), 3);

            SubOurImgBGRA3 = ImgDaSegmentare.GetSubRect(rett3);
            SubOurImgBGRA3.Save("SubRectangleGambe_SenzaPelle.png");
            ImgRettangoli.Draw(rett3, new Bgra(0, 0, 255, 255), 3);

            SubOurImgBGRA4 = ImgDaSegmentare.GetSubRect(rett4);
            SubOurImgBGRA4.Save("SubRectanglePiedi_SenzaPelle.png");
            ImgRettangoli.Draw(rett4, new Bgra(102, 0, 51, 255), 3);

            ImgRettangoli.Save("immagine_rettangolata.png");
            imageBox1.Image = ImgRettangoli;
            ////////crea l'array
            OurImgBGRAArray[0] = SubOurImgBGRA0;
            OurImgBGRAArray[1] = SubOurImgBGRA1;
            OurImgBGRAArray[2] = SubOurImgBGRA2;
            OurImgBGRAArray[3] = SubOurImgBGRA3;
            OurImgBGRAArray[4] = SubOurImgBGRA4;

            return OurImgBGRAArray;
        }

        /********************************************************************************************************************************************/
       /// <summary>
        /// Segmenta l'utente uomo, senza pelle, in 5 aree prefissate: 0=sopra la testa; 1=collo; 2=busto; 3=gambe; 4=piedi.
        /// Per ogniuno di questi segmenti traccia un rettangolo colorato che li indica nell'immagine originale.
        /// Le immagini di ogni segmento e l'immagine con i rettangoli vengono salvate nella cartella del debug dell'applicazione.
       /// </summary>
       /// <param name="ImgDaSegmentare"></param>
       /// <param name="ImgRettangoli"></param>
        /// <returns>OurImgBGRAArray</returns>
        public Image<Bgra, byte>[] SegmentaUomo(Image<Bgra, byte> ImgDaSegmentare, Image<Bgra, byte> ImgRettangoli)
        {

            Image<Bgra, byte>[] OurImgBGRAArray = new Image<Bgra, byte>[5];
            Image<Bgra, byte> SubOurImgBGRA0;
            Image<Bgra, byte> SubOurImgBGRA1;
            Image<Bgra, byte> SubOurImgBGRA2;
            Image<Bgra, byte> SubOurImgBGRA3;
            Image<Bgra, byte> SubOurImgBGRA4;

            Rectangle rett0 = new Rectangle(147, 7, 87, 48);
            Rectangle rett1 = new Rectangle(150, 110, 89, 40);
            Rectangle rett2 = new Rectangle(103, 137, 201, 157);
            Rectangle rett3 = new Rectangle(133, 299, 140, 198);
            Rectangle rett4 = new Rectangle(133, 495, 137, 51);
            /////////calcola i retangoli da tagliuzzare
            SubOurImgBGRA0 = ImgDaSegmentare.GetSubRect(rett0);
            SubOurImgBGRA0.Save("SubRectangleTesta_SenzaPelle.png");
            ImgRettangoli.Draw(rett0, new Bgra(0, 255, 255, 255), 3);

            SubOurImgBGRA1 = ImgDaSegmentare.GetSubRect(rett1);
            SubOurImgBGRA1.Save("SubRectangleCollo_SenzaPelle.png");
            ImgRettangoli.Draw(rett1, new Bgra(255, 255, 0, 255), 3);

            SubOurImgBGRA2 = ImgDaSegmentare.GetSubRect(rett2);
            SubOurImgBGRA2.Save("SubRectanglePetto_SenzaPelle.png");
            ImgRettangoli.Draw(rett2, new Bgra(0, 255, 0, 255), 3);

            SubOurImgBGRA3 = ImgDaSegmentare.GetSubRect(rett3);
            SubOurImgBGRA3.Save("SubRectangleGambe_SenzaPelle.png");
            ImgRettangoli.Draw(rett3, new Bgra(0, 0, 255, 255), 3);

            SubOurImgBGRA4 = ImgDaSegmentare.GetSubRect(rett4);
            SubOurImgBGRA4.Save("SubRectanglePiedi_SenzaPelle.png");
            ImgRettangoli.Draw(rett4, new Bgra(102, 0, 51, 255), 3);

            ImgRettangoli.Save("immagine_rettangolata.png");
            imageBox1.Image = ImgRettangoli;
            ////////crea l'array
            OurImgBGRAArray[0] = SubOurImgBGRA0;
            OurImgBGRAArray[1] = SubOurImgBGRA1;
            OurImgBGRAArray[2] = SubOurImgBGRA2;
            OurImgBGRAArray[3] = SubOurImgBGRA3;
            OurImgBGRAArray[4] = SubOurImgBGRA4;

            return OurImgBGRAArray;
        }

        /***************************************************************************************************************************************************/
        /// <summary>
        /// Elimina tutti i pixel relativi alla pelle partendo dall'originale immagine di test. Per decidere quali pixel saranno cancellati o meno, viene avviato il 
        /// riconoscimento facciale. All'interno dell'area rettangolare corrispondente al viso, viene estratta una sottoparte nell'area relativa a naso e guance.
        /// Nel caso in cui nessuna faccia venga riconosciuta l'immagine non verrà elaborata.
        /// Nel caso in cui vengano riconosciute più di una faccia, si considererà solo la prima.
        /// </summary>
        /// <param name="ImgScelta"></param>
        /// <param name="ImgSceltaGray"></param>
        /// <returns>ImgScelta</returns>
        public Image<Bgra, byte> EliminaPelle(Image<Bgra, byte> ImgScelta, Image<Gray, byte> ImgSceltaGray)
        {


            var faces = ImgSceltaGray.DetectHaarCascade(haar, 1.4, 4, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(25, 25))[0];
            Image<Bgra, byte> faccia;
            if(faces.Count() !=0)
            {
                faccia = ImgScelta.GetSubRect(faces[0].rect);
                Image<Bgra, byte> facciaResized = faccia.Resize(100, 100, INTER.CV_INTER_CUBIC, false);
                facciaResized.Save("faccia_estratta.png");
                Image<Bgra, byte> guancia;
                guancia = facciaResized.GetSubRect(new Rectangle(27, 55, 51, 12));
                guancia.Save("Guance_Naso.png");
                Bgra avg;
                MCvScalar StandardDeviation;
                guancia.AvgSdv(out avg, out StandardDeviation);
                float epsilon = 70;
                for (int h = 0; h <= ImgScelta.Rows - 1; h++)
                {
                    for (int l = 0; l <= ImgScelta.Cols - 1; l++)
                    {
                        if (((ImgScelta[h, l].Blue >= (avg.Blue - epsilon)) && (ImgScelta[h, l].Blue <= (avg.Blue + epsilon))) && ((ImgScelta[h, l].Green >= (avg.Green - epsilon)) && (ImgScelta[h, l].Green <= (avg.Green + epsilon))) && ((ImgScelta[h, l].Red >= (avg.Red - epsilon)) && (ImgScelta[h, l].Red <= (avg.Red + epsilon))))
                        {
                            ImgScelta[h, l] = new Bgra(0, 0, 0, 0);
                        };
                    }
                }
            }
            
            return ImgScelta;
        }

        /***************************************************************************************************************************************/
        /// <summary>
        /// Descrive quali sono i nomi delle sottocartelle del database bambini
        /// </summary>
        /// <returns>ListaCapiBimbi</returns>
        public List<string> RiempiCapiBimbi()
        {
            List<string> ListaCapiBimbi = new List<string>();
            ListaCapiBimbi.Add("Bimbo");
            ListaCapiBimbi.Add("Bimba");

            return ListaCapiBimbi;
        }

        /***************************************************************************************************************************************/
        /// <summary>
        /// Descrive quali sono i nomi delle sottocartelle del database uomo
        /// </summary>
        /// <returns>ListaCapiUomo</returns>
        public List<string> RiempiCapiUomo()
        {
            List<string> ListaCapiUomo = new List<string>();
            ListaCapiUomo.Add("Camicie");
            ListaCapiUomo.Add("Cappelli e Occhiali");
            ListaCapiUomo.Add("Completi e Cravatte");
            ListaCapiUomo.Add("Giacche");
            ListaCapiUomo.Add("Jeans");
            ListaCapiUomo.Add("Maglieria e Felpe");
            ListaCapiUomo.Add("Pantaloni");
            ListaCapiUomo.Add("Scarpe");
            ListaCapiUomo.Add("Sciarpe e Accessori");
            ListaCapiUomo.Add("T-shirt e Polo");    
            return ListaCapiUomo;
        }

        /***************************************************************************************************************************************/
        /// <summary>
        /// Descirve quali sono i nomi delle sottocartelle del database donna
        /// </summary>
        /// <returns>ListaCapiDonna</returns>
        public List<string> RiempiCapiDonna()
        {
            List<string> ListaCapiDonna = new List<string>();
            ListaCapiDonna.Add("Borse");
            ListaCapiDonna.Add("Camicie");
            ListaCapiDonna.Add("Cappelli");
            ListaCapiDonna.Add("Cinture");
            ListaCapiDonna.Add("Foulard e Sciarpe");
            ListaCapiDonna.Add("Giacche");
            ListaCapiDonna.Add("Gioielli");
            ListaCapiDonna.Add("Gonne");
            ListaCapiDonna.Add("Guanti");
            ListaCapiDonna.Add("Jeans");
            ListaCapiDonna.Add("Maglieria e Felpe");
            ListaCapiDonna.Add("Pantaloni");
            ListaCapiDonna.Add("Scarpe");
            ListaCapiDonna.Add("T-shirt e Top");
            ListaCapiDonna.Add("Vestiti");

            return ListaCapiDonna;
        }   


        /*---------------------------------------------------------------------------------------------------------------------------------------------------------*/
        /*--------------------------------------BOTTONI------------------------------------------------------------------------------------------------------*/
        /*-----------------------------------------------------------------------------------------------------------------------------------------------------*/


        /***********************************************************************************************************************************************/
        //---------------------------CALCOLA GLI ISTOGRAMMI DELLE IMMAGINI DEL DATABASE E LI SERIALIZZA-------------------------------------------------
        /***************************************************************************************************************************************************/
        /// <summary>
        /// Premendo questo pulsante comincia il calcolo di ogni istogramma delle immagini del database. Successivamente un file .txt verrà generato per salvare 
        /// tutte le informazioni. Se il file è già presente verrà sovrascritto.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calcola_Click_1(object sender, EventArgs e)
        {
            /*-------  CALCOLA ISTOGRAMMI BGR PER OGNI IMMAGINE DEL DATABASE E SERIALIZZA LE INFO ----------*/
            ProgressBar progressBar1 = new ProgressBar();
            progressBar1.Step = 1;
            progressBar1.Size = new Size(252, 38);
            progressBar1.Location = new Point(329,656); //da modificare
            tabPage6.Controls.Add(progressBar1);
            progressBar1.Show();

            DenseHistogram Hist = new DenseHistogram(new int[] { 20, 20, 20 }, new RangeF[] { new RangeF(0, 255), new RangeF(0, 255), new RangeF(0, 255) });
            List<DataImg> lista = new List<DataImg>();
            List<string> lista_nomi_capi_donna = new List<string>();
            List<string> lista_nomi_capi_uomo = new List<string>();
            List<string> lista_nomi_capi_bambini = new List<string>();
            lista_nomi_capi_donna = RiempiCapiDonna();
            lista_nomi_capi_uomo = RiempiCapiUomo();
            lista_nomi_capi_bambini = RiempiCapiBimbi();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 0;
            //ciclo per riempire il massimo valore della rogress bar
            for (int j = 0; j <= lista_nomi_capi_donna.Count - 1; j++)
            {
                string path = DatabasePath + "\\donna\\" + lista_nomi_capi_donna[j];
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] arrayImg = dir.GetFiles("*.png");
                progressBar1.Maximum = progressBar1.Maximum + arrayImg.Length;
            }
            for (int j = 0; j <= lista_nomi_capi_uomo.Count - 1; j++)
            {
                string path = DatabasePath + "\\uomo\\" + lista_nomi_capi_uomo[j];
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] arrayImg = dir.GetFiles("*.png");
                progressBar1.Maximum = progressBar1.Maximum + arrayImg.Length;
            }
            for (int j = 0; j <= lista_nomi_capi_bambini.Count - 1; j++)
            {
                string path = DatabasePath + "\\bimbi\\" + lista_nomi_capi_bambini[j];
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] arrayImg = dir.GetFiles("*.png");
                progressBar1.Maximum = progressBar1.Maximum + arrayImg.Length;
            }

            //scorro tutte le cartelle del database donna calcolo istogramma rgba e metto i tag e addiungo alla lista
            for (int h = 0; h <= lista_nomi_capi_donna.Count - 1; h++)
            {
                string path = DatabasePath + "\\donna\\" + lista_nomi_capi_donna[h];
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] arrayImg = dir.GetFiles("*.png");
                //ciclo for per riempire la lista con gli istogrammi e tag delle immagini del database
                for (int j = 0; j <= arrayImg.Length - 1; j++)
                {
                    //calcolo degli istogrammi  e tag con le immagini png ritagliate dallo sfondo
                    string nome = arrayImg[j].Name;
                    string directoryIMG = path + "\\" + nome;
                    Image<Bgra, byte> DatabaseImg = new Image<Bgra, byte>(path + "\\" + nome);
                    Hist = ComputeHistoBGRA(DatabaseImg);
                    string TypeMerce = GeneraTagsCartellaDonna(lista_nomi_capi_donna[h]);
                    lista.Add(new DataImg(Hist, TypeMerce, directoryIMG));
                    progressBar1.PerformStep();
                }
            }

            //scorro tutte le cartelle del database uomo calcolo istogramma rgba e metto i tag e addiungo alla lista
            for (int h = 0; h <= lista_nomi_capi_uomo.Count - 1; h++)
            {
                string path = DatabasePath + "\\uomo\\" + lista_nomi_capi_uomo[h];
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] arrayImg = dir.GetFiles("*.png");

                //ciclo for per riempire la lista con gli istogrammi e tag delle immagini del database
                for (int j = 0; j <= arrayImg.Length - 1; j++)
                {
                    //calcolo degli istogrammi  e tag con le immagini png ritagliate dallo sfondo
                    string nome = arrayImg[j].Name;
                    string directoryIMG = path + "\\" + nome;
                    Image<Bgra, byte> DatabaseImg = new Image<Bgra, byte>(path + "\\" + nome);
                    Hist = ComputeHistoBGRA(DatabaseImg);
                    string TypeMerce = GeneraTagsCartellaUomo(lista_nomi_capi_uomo[h]);
                    lista.Add(new DataImg(Hist, TypeMerce, directoryIMG));
                    progressBar1.PerformStep();
                }
            }

            //scorro tutte le cartelle del database BAMBINI calcolo istogramma rgba e metto i tag e addiungo alla lista
            for (int h = 0; h <= lista_nomi_capi_bambini.Count - 1; h++)
            {
                string path = DatabasePath + "\\bimbi\\" + lista_nomi_capi_bambini[h];
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] arrayImg = dir.GetFiles("*.png");

                //ciclo for per riempire la lista con gli istogrammi e tag delle immagini del database
                for (int j = 0; j <= arrayImg.Length - 1; j++)
                {
                    //calcolo degli istogrammi  e tag con le immagini png ritagliate dallo sfondo
                    string nome = arrayImg[j].Name;
                    string directoryIMG = path + "\\" + nome;
                    Image<Bgra, byte> DatabaseImg = new Image<Bgra, byte>(path + "\\" + nome);
                    Hist = ComputeHistoBGRA(DatabaseImg);
                    string TypeMerce = GeneraTagsCartellaBambini(lista_nomi_capi_bambini[h]);
                    lista.Add(new DataImg(Hist, TypeMerce, directoryIMG));
                    progressBar1.PerformStep();
                }
            }

            MessageBox.Show("Histogram computation finished. Now starts the saving operation in a file.");
            //parte di serializzazione
            ObjectToSerialize objectToSerialize = new ObjectToSerialize(lista);
            Serializer serializer = new Serializer();
            serializer.SerializeObject("outputFileTagghy.txt", objectToSerialize);
            MessageBox.Show("import complited succesfully");
            MessageBox.Show("ATTENTION: IF SOMETHING CHANGES IN THE DATABASE DIRECTORY FOLDER, PLEASE, RE-COMPUTE THE IMPORT OPERATION");
            //essendo che ho fatto l'operazione passo alla pagina successiva e l'altra la disabilito
            ((Control)this.tabPage1).Enabled = true;
            tabControl1.SelectedIndex = 2;
            ((Control)this.tabPage6).Enabled = false;
            Label mess = new Label();
            mess.Text = "PAGE NOT ANYMORE ACCESSIBLE.YOU HAVE ALREADY COMPUTE THE COMPUTE OPERATION. \n CONTINUE TO USE THE APPLICATION OR SHUT DOWN THE APPLIACTION";
            mess.AutoSize = true;
            tabPage6.Controls.Add(mess);
        }
        /**********************************************************************************************************************************************/
        //------------------------DESERIALIZZA IL FILE DI TESTO CONTENENTE TUTTE LE CARATTERISTICHE DELLE IMMAGINI DEL DATABASE
        /**********************************************************************************************************************************************/
        /// <summary>
        /// Questo pulsante permette di leggere il file .txt che contiene tutte le informazioni riguardanti gli istogrammi. Tutte le informazioni saranno importate
        /// in una lista che verra utilizzata durante il corso d'esecuzione dell'applicazione.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importa_Click(object sender, EventArgs e)
        {
            //PARTE DI DESERIALIZZAZIONE
            ObjectToSerialize objectToDeSerialize = new ObjectToSerialize();
            Serializer serializer1 = new Serializer();
            objectToDeSerialize = serializer1.DeSerializeObject("outputFileTagghy.txt");
            lista1 = objectToDeSerialize.OggettoDaSerializzare;
            MessageBox.Show("READING complited succesfully");
            MessageBox.Show("ATTENTION: IF SOMETHING CHANGES IN THE DATABASE DIRECTORY FOLDER, RE-COMPUTE FIRST THE CALCOLA OPERATION, THAN THE IMPORT OPERATION");
            tabControl1.SelectedIndex = 3;
            ((Control)this.tabPage2).Enabled = true;
            ((Control)this.tabPage1).Enabled = false;
            Label messaggio= new Label();
            messaggio.Text = "PAGE NOT ANYMORE ACCESSIBLE.YOU HAVE ALREADY COMPUTE THE IMPORT OPERATION. \n CONTINUE TO USE THE APPLICATION OR SHUT DOWN THE APPLIACTION";
            messaggio.AutoSize=true;
            tabPage1.Controls.Add(messaggio);
        }

        /*****************************************************************************************************************************************/
        //----------------------PERMETTE DI SCEGLIERE UN IMMAGINE DI TEST E SI SALVA LA DIRECTORY
        /*******************************************************************************************************************************************/
      /// <summary>
      /// Questo pulsante permette di scegliere un'immagine di tipo ".png" da utilizzare come immagine di test.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
        private void BrowseTestImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                OurIMGDirectory = Openfile.FileName;
                string estensione = Path.GetExtension(OurIMGDirectory);
                if ((estensione == ".png"))
                {
                    pictureBox_ourImg.Load(Openfile.FileName);
                }
                else 
                {
                    MessageBox.Show("NOT VALID FILE EXTENSION. retry");
                }
                
            }
        }
        

        /***********************************************************************************************************************************************/
        //-----------------------ESEGUE IL SUGGERIMENTO SULLA BASE DELL'IMMAGINE DI TEST PRESCELTA
        /***********************************************************************************************************************************************/
        /// <summary>
        /// Questo pulsante permette di eseguire il suggerimento in base sia al genere dell'utente che alla sua orientazione dello sguardo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void suggest_Click(object sender, EventArgs e)
        {
          if (OurIMGDirectory == null)
          {
              MessageBox.Show("BEFORE CLICKING THE BUTTON FOR THE SUGGESTION, PLEASE CHOOSE A TEST IMAGE");
          }
          else 
          {
            
            ///////////////////////////////////////////////////////////////////////////
            //////PARTE DI ESTRAZIONE DELLE FEATURES DELLA PERSONA/////////////////////
            //////////////////////////////////////////////////////////////////////////
            haar = new HaarCascade(@"haarcascade_frontalface_default.xml");

            //contiene gli istogrammi dell'omino
            DenseHistogram[] ArrayOurHist = new DenseHistogram[5];
            for (int i = 0; i <= 4; i++)
            {
                ArrayOurHist[i] = new DenseHistogram(new int[] { 20, 20, 20 }, new RangeF[] { new RangeF(0, 255), new RangeF(0, 255), new RangeF(0, 255) });
            }

            Image<Bgra, byte> OurImgBGRA = new Image<Bgra, byte>(OurIMGDirectory); //immagine di test che scelgo
            Image<Gray, byte> OurImgGRAY = new Image<Gray, byte>(OurIMGDirectory);

            //faccio il resize dell'immagine in modo che siano tutte grandi uguali e che i rettangolini che estraggono le parti del corpo abbiano senso
            Image<Bgra, byte> ResizedOurImg;
            Image<Gray, byte> ResizedOurImgGRAY;
            Image<Bgra, byte> ResizedOurImgCopy;
            bool preservescale = false;
            ResizedOurImg = OurImgBGRA.Resize(380, 550, INTER.CV_INTER_CUBIC, preservescale); //sarà da passare alla funzione di segmentazione e a quella che toglie la pelle
            ResizedOurImg.Save("ResizedBGRA.png");
            ResizedOurImgGRAY = OurImgGRAY.Resize(380, 550, INTER.CV_INTER_CUBIC, preservescale);
            ResizedOurImgGRAY.Save("ResizedGRAY.png");
            ResizedOurImgCopy = OurImgBGRA.Resize(380, 550, INTER.CV_INTER_CUBIC, preservescale); //sarà da passare alla funzione di segmentazione e a quella che toglie la pelle
            ResizedOurImgCopy.Save("ResizedBGRACopy.png");
            ResizedOurImg = EliminaPelle(ResizedOurImg, ResizedOurImgGRAY);
            ResizedOurImg.Save("SenzaPelle.png");
            Image<Bgra, byte>[] array_da_analizzare = new Image<Bgra, byte>[5];//contiene tutti i rettangolini delle parti del corpo ritagliate

            /*definisco e inizializzo un array di DataImg per contenere tutti i capi potenzialmente suggeribili in base alla parte del corpo che sto anlizzando
             ES: Donna, ArraySesso[1]-> 1=sto analizzando la parte del collo-> esso conterrà tutte le sciarpe e le collane*/
            List<DataImg>[] ArraySesso = new List<DataImg>[5];
            for (int i = 0; i <= array_da_analizzare.Count() - 1; i++)
            { ArraySesso[i] = new List<DataImg>(); }

            if (checkBox1.Checked)
            {
                array_da_analizzare = SegmentaUomo(ResizedOurImg, ResizedOurImgCopy);
                for (int i = 0; i <= array_da_analizzare.Count() - 1; i++)
                { ArraySesso[i] = RiempiListaSessoUomo(i); }
            }
            else
            {
                if (checkBox2.Checked)
                {
                    array_da_analizzare = SegmentaDonna(ResizedOurImg,ResizedOurImgCopy);
                    for (int i = 0; i <= array_da_analizzare.Count() - 1; i++)
                    { ArraySesso[i] = RiempiListaSessoDonna(i); }
                }
                else
                {
                    //scegli la segmenta donna nel caso in cui non schiacci alcun bottone, è più probabile che sia una donna a guardare le vetrine :)
                    array_da_analizzare = SegmentaDonna(ResizedOurImg, ResizedOurImgCopy);
                    for (int i = 0; i <= array_da_analizzare.Count() - 1; i++)
                    { ArraySesso[i] = RiempiListaSessoDonna(i); }
                }
            };
            //calcola istogrammi per ogni rettangolino di pezzetTino di persona estratto
            for (int i = 0; i <= array_da_analizzare.Count() - 1; i++)
            { ArrayOurHist[i] = ComputeHistoBGRA(array_da_analizzare[i]); }
          

            //definisco l'array di liste che contiente i vari confronti e lo inizializzo.
            /*es: ArraydiMetriche[0] conterrà tutti i risultati dei confronti fra la lista di immagini selezionate dal database
             di cappelli+guanti che si confronteranno con il rettangolino che rappresenta la parte alta dell'omino*/
            double result;
            List<Metric>[] ArraydiMetriche = new List<Metric>[5];
            for (int i = 0; i <= 4; i++)
            { ArraydiMetriche[i] = new List<Metric>(); }

            for (int i = 0; i <= ArraySesso.Count() - 1; i++)
            {
                for (int j = 0; j <= ArraySesso[i].Count() - 1; j++)
                {
                    result = CompareHistBGRA(ArraySesso[i][j].getBGRAHist(), ArrayOurHist[i]);
                    ArraydiMetriche[i].Add(new Metric(result, ArraySesso[i][j].theDirectory));
                }
            }

            //ordinia le liste che contengono le comparazioni degli istogrammi  che ha appena riempito sopra, in base alla somiglianza.
            List<Metric>[] ArraydiMetricheSorted = new List<Metric>[5];
            for (int i = 0; i <= ArraydiMetriche.Count() - 1; i++)
            { ArraydiMetricheSorted[i] = new List<Metric>(); }


            for (int i = 0; i <= ArraydiMetriche.Count() - 1; i++)
            { ArraydiMetricheSorted[i] = OrdinaPerColoreBGR(ArraydiMetriche[i]); }


            //estraggo una lista di vestiti coerente in base a cosa sta guardando il cliente
            List<DataImg> ListaArticolo = new List<DataImg>();
            ListaArticolo = RiempiListaArticoli();

            /*Confronto in base al colore delle tue singole parti del corpo quale dei vestiti potrebbe piacerti di più
             es: donna con cappellino rosso sta guardando a giacche da uomo: vado a controllare fra le giacche da uomo quale assomiglia di più
             al colore del berrettino. E così anche per tutte le altre parti del corpo. che giacca assomigli di più al colore della mia maglietta, pantaloni ecc..*/
            double resulta;
            List<Metric>[] ArraydiMetricheArticolo = new List<Metric>[5];
            for (int i = 0; i <= ArraydiMetricheArticolo.Count() - 1; i++)
            { ArraydiMetricheArticolo[i] = new List<Metric>(); }

            for (int i = 0; i <= ArraydiMetricheArticolo.Count() - 1; i++)
            {
                for (int j = 0; j <= ListaArticolo.Count() - 1; j++)
                {
                    resulta = result = CompareHistBGRA(ListaArticolo[j].getBGRAHist(), ArrayOurHist[i]);
                    ArraydiMetricheArticolo[i].Add(new Metric(resulta, ListaArticolo[j].theDirectory));
                }
            }

            List<Metric>[] ArraydiMetricheArticoloSorted = new List<Metric>[5];
            for (int i = 0; i <= ArraydiMetricheArticolo.Count() - 1; i++)
            {
                ArraydiMetricheArticoloSorted[i] = OrdinaPerColoreBGR(ArraydiMetricheArticolo[i]);
            }

            //PARTE DI DECISIONE DI COSA FAR VEDERE IN BASE AI PESI CHE SONO STATI POSTI CON LE BARRE
            int PesoSex, PesoTags, PesoColour;

            int NImgForGender, NImgForArticle, NImgForGenderCol, NImgForArticleCol, NImgForGenderNONCol, NImgForArticleNONCol;

            PesoSex = PesoSesso.Value;
            PesoTags = PesoTag.Value;
            PesoColour = PesoColore.Value;
            /*----------------------------------------------------CASO1--------------------------------------------------------*/
            /*Da più importanza,in termini di immagini visualizzate, al sesso della persona che guarda,piuttosto che alla
             tipologia di merce che sta puntando con gli occhi. ES: Uomo che guarda ad accessori donna. Peso sesso 8/10, 
             pesoTag 4/10--> di 10 immagini di cui fare il display 7 riguardano capi dell'uomo. 3 riguardano accessori da donna*/
            /*------------------------------------------------------------------------------------------------------------------*/
            if (PesoSex > PesoTags)
            {
                NImgForGender = (int)((PesoSex * NImgsToDisplay) / (PesoSex + PesoTags));
                NImgForArticle = NImgsToDisplay - NImgForGender;

                //delle immgini che ti devo suggerire, quante devono essere correlate al colore che tu porti??
                NImgForGenderCol = (int)((NImgForGender * PesoColour * 10) / (100));
                NImgForGenderNONCol = NImgForGender - NImgForGenderCol;
                NImgForArticleCol = (int)((NImgForArticle * PesoColour * 10) / (100));
                NImgForArticleNONCol = NImgForArticle - NImgForArticleCol;
                MessageBox.Show("N Im Genere:" + NImgForGender.ToString() + "-> Col:" + NImgForGenderCol.ToString() + "-> NON Col" + NImgForGenderNONCol.ToString());
                MessageBox.Show("N Im Articolo:" + NImgForArticle.ToString() + "-> Col:" + NImgForArticleCol.ToString() + "-> NON Col" + NImgForArticleNONCol.ToString());

                DecidiCosaVisualizzare(ArraydiMetriche, ListaArticolo, ArraydiMetricheSorted, ArraydiMetricheArticoloSorted, NImgForGenderCol, NImgForGenderNONCol, NImgForArticleCol, NImgForArticleNONCol);

            };

         
            /*-----------------------------------------------------CASO2-----------------------------------------------------------*/
            /*Da più importanza,in termini di immagini visualizzate,alla tipologi di merce che la persona sta guardando piuttosto che 
             al sesso della persona stessa.*/
            /*--------------------------------------------------------------------------------------------------------------------*/
            if (PesoSex < PesoTags)
            {
                NImgForArticle = (int)(PesoTags * NImgsToDisplay / (PesoSex + PesoTags));
                NImgForGender = NImgsToDisplay - NImgForArticle;

                //delle immgini che ti devo suggerire, quante devono essere correlate al colore che tu porti??
                NImgForGenderCol = (int)((NImgForGender * PesoColour * 10) / (100));
                NImgForGenderNONCol = NImgForGender - NImgForGenderCol;
                NImgForArticleCol = (int)((NImgForArticle * PesoColour * 10) / (100));
                NImgForArticleNONCol = NImgForArticle - NImgForArticleCol;

                DecidiCosaVisualizzare(ArraydiMetriche, ListaArticolo, ArraydiMetricheSorted, ArraydiMetricheArticoloSorted, NImgForGenderCol, NImgForGenderNONCol, NImgForArticleCol, NImgForArticleNONCol);
            };

            /*-----------------------------------------------------CASO3---------------------------------------------------*/
            /*Da stessa importanza,in termini di immagini visualizzate, sia al sesso della persona che sta guardando,
             sia alla tipologia di merce che sta puntando con gli occhi*/
            /*--------------------------------------------------------------------------------------------------------------*/
            if (PesoSex == PesoTags)
            {
                NImgForGender = (int)(NImgsToDisplay / 2);
                NImgForArticle = NImgsToDisplay - NImgForGender;

                //delle immgini che ti devo suggerire, quante devono essere correlate al colore che tu porti??
                NImgForGenderCol = (int)((NImgForGender * PesoColour * 10) / (100));
                NImgForGenderNONCol = NImgForGender - NImgForGenderCol;
                NImgForArticleCol = (int)((NImgForArticle * PesoColour * 10) / (100));
                NImgForArticleNONCol = NImgForArticle - NImgForArticleCol;

                DecidiCosaVisualizzare(ArraydiMetriche, ListaArticolo, ArraydiMetricheSorted, ArraydiMetricheArticoloSorted, NImgForGenderCol, NImgForGenderNONCol, NImgForArticleCol, NImgForArticleNONCol);

            };
            ((Control)this.tabPage3).Enabled = true;
            tabControl1.SelectedIndex = 4;
          };
        }

        //TUTTO IL RESTO 
        private void pictureBox_ourImg_Click(object sender, EventArgs e)
        {

        }

       
        private void pictureBox_DBImg1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_DBImg2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_DBImg3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_DBImg4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_DBImg5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_DBImg6_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void AccessoriUomo_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ParteMediaUomo_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ParteBassaUomo_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void progressBarSerialization_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
           
            var f2 = new Form();
            PictureBox pb = (PictureBox)sender;
            PictureBox newPb = new PictureBox();
            Bitmap img = new Bitmap(pb.Image);

            Bitmap resizedimg = new Bitmap(img, new Size(img.Width / 2, img.Height / 2));
            var newwidth = resizedimg.Width;
            var newheight = resizedimg.Height;
            f2.Size = new Size(newwidth+25,newheight+100);
            newPb.Image = resizedimg;
            newPb.Size = new Size(newwidth, newheight);
    
            f2.StartPosition = FormStartPosition.CenterScreen;
            f2.Controls.Add(newPb);
            f2.ShowDialog();
        }
        private void Form1_MouseLeave(object sender, EventArgs e)
        {

            PictureBox pb = (PictureBox)sender;
            pb.Size = new Size(100, 100);
        }
        
        

        private void calcola_MouseEnter(object sender, EventArgs e)
        {
              
        }   
        private void calcola_MouseLeave(object sender, EventArgs e)
        {
         
        }   

        private void f2(object sender, EventArgs e)
        {

        }

        

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox1_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
           
        }

         private void button1_Click(object sender, EventArgs e)
        {
           
        }

         private void label10_Click(object sender, EventArgs e)
         {

         }

         private void tabPage1_Click(object sender, EventArgs e)
         {
         
         }

         private void pictureBox2_Click(object sender, EventArgs e)
         {

         }

         private void label10_Click_1(object sender, EventArgs e)
         {

         }

         private void ENJOY_Click(object sender, EventArgs e)
         {
             ((Control)this.tabPage6).Enabled = true;
             tabControl1.SelectedIndex = 1;
             ((Control)this.tabPage5).Enabled = false;
         }

         private void SKIP_STEP1_Click(object sender, EventArgs e)
         {
             string dir_serialized_file = "outputFileTagghy.txt";
             bool fileExist = File.Exists(dir_serialized_file);
             if (fileExist == true)
             {
                 ((Control)this.tabPage1).Enabled = true;
                 tabControl1.SelectedIndex = 2;
                 ((Control)this.tabPage6).Enabled = false;
                 Label mess = new Label();
                 mess.Text = "PAGE NOT ANYMORE ACCESSIBLE.YOU HAVE ALREADY COMPUTE THE COMPUTE OPERATION. \n CONTINUE TO USE THE APPLICATION OR SHUT DOWN THE APPLIACTION";
                 mess.AutoSize = true;
                 mess.BringToFront();
                 tabPage6.Controls.Add(mess);
             }
             else 
             {
                 MessageBox.Show("Cannot skip this step if you have not extract the features of the database images. PLEASE COMPUTE IT and than go through the other steps.");
             };
         }

         private void tabPage6_Click(object sender, EventArgs e)
         {
            
         }



         private void calcola_Click(object sender, EventArgs e)
         {


         }

         private void label13_Click(object sender, EventArgs e)
         {

         }

         private void button1_Click_1(object sender, EventArgs e)
         {
             MessageBox.Show("The main goal of the project is to create a system able to suggests for sale clothes to the window shoppers based on their worn clothes and their gaze orientation, focusing on a color analysis. Specifically the system have to recognize how the window shopper likes wear, in a sense that it analyses the color for each cloth worn by the shopper and it have to suggest some compliant articles stored in the shop database, basing on the colors it has recognized. The suggestion takes also care about the gaze orientation of the window shopper, that means the application have to observe where the window shopper is looking at and taking into account the range of products he’s interested in, suggests other similar products in the same range, based on the color analysis of the clothes worn by the user. In order to perform the correct suggestion, the application have to distinguish between the gender of the window shopper (Male/Female) and  distinguish among the region of the window shop where the user is looking at, because each range of product is exposed in a different area of the window shop. The system contains also the possibility for each kind of store to set some parameters to make the suggestion suitable with the products sold.");
         }

         private void textBox1_TextChanged_1(object sender, EventArgs e)
         {

         }
        
    }
}


