Prerequisites

1) SO supported:
   Windows 7 SP1 (x86 e x64) ; Windows 8 (x86 e x64) ; Windows 8.1 (x86 e x64) ; Windows Server 2008 R2 SP1 (x64) ; Windows Server 2012 (x64) 
   ;Windows Server 2012 R2 (x64)
  
2) Libraries  
  -Download and Install Emgu.CV-2.4.10 available on  http://sourceforge.net/projects/emgucv/.
  -To resolve the dependency issue download and install MSVCRT 9.0 SP1 x64  available on https://www.microsoft.com/en-us/download/details.aspx?id=13523 if the OS is x64,
   else  MSVCRT 9.0 SP1 x86 available on https://www.microsoft.com/en-us/download/details.aspx?id=8328 if the OS is x86.
   
3) Open the Emgu folder , in the bin directory select the folder x64 or x86, depending on the OS you use.
   Now select all the following .dll files and copy them in the Project debug folder (Window_shopper_recommender/bin/Debug).
   
4)	Open the Emgu folder ,  in the bin directory select the following .dll : Emgu.CV.dll, Emgu.CV.UI.dll, Emgu.Util.dll, ZedGraph.dll 
    and haarcascade_frontalface_default.xml and  copy them in the Project debug folder (Window_shopper_recommender/bin/Debug).
    
5)	Download  the file System.Windows.Controls.dll and copy it in the Project debug folder (Window_shopper_recommender/bin/Debug).

6)	Copy the DATABASE into the Project Debug folder (Window_shopper_recommender/bin/Debug).

7)	We provide also the .txt file called "outputFileTagghy.txt" containing all the database image features already computed. 
    Copy it into the project debug folder (Window_shopper_recommender/bin/Debug) if you want to save time.
    
8)	To run the application go into Project debug folder (Window_shopper_recommender/bin/Debug) and execute the file “Window_shopper_recommender.exe” .

9) To access the HTML documentation you should download the folder called "html" and start the file called "annotated.html"
