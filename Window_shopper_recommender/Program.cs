using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows;
using com.drew.metadata;
using com.drew.imaging.jpg;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Media.Imaging;
//using System.Windows.Media;
using System.Windows.Input;
//using System.Windows.Navigation;
//using System.Windows.Shapes;


using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Window_shopper_recommender
{
    static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
