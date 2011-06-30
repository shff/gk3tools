using System;
using System.Collections.Generic;
using System.Text;

namespace Builder
{
   class Program
   {
      private const string LzoFile = "lzo-2.05";
      private const string ZLibFile = "zlib-1.2.5";
      private const string FullLzoFile = LzoFile + ".tar.gz";
      private const string FullZLibFile = ZLibFile + ".tar.gz";
      private const string LzoUrl = "http://www.oberhumer.com/opensource/lzo/download/" + FullLzoFile;
      private const string ZLibUrl = "http://zlib.net/" + FullZLibFile;

      static void Main(string[] args)
      {
         if (args.Length > 1)
         {
            if (args[0] == "-d")
            {
               Downloader.Download(args[1]);
            }
            else if (args[0] == "-x")
            {
               Extractor.Extract(args[1]);
            }
            else
            {
               Console.WriteLine("I don't understand the command line arguments.");
            }
         }
         else
         {
            Console.WriteLine("Need either -d URL or -x FILE.");
         }

         return;

         // download LZO
         Console.WriteLine("Downloading LZO...");
         Downloader.Download(LzoUrl, FullLzoFile);

         // download ZLib
         Console.WriteLine("Downloading ZLib...");
         Downloader.Download(ZLibUrl, FullZLibFile);

         // extract LZO
         Console.WriteLine("Extracting LZO...");
         Extractor.Extract(FullLzoFile, LzoFile);

         // extract ZLib
         Console.WriteLine("Extracting ZLib...");
         Extractor.Extract(FullZLibFile, ZLibFile);

         // building LZO...
         Console.WriteLine("Building LZO...");
         System.Diagnostics.ProcessStartInfo i = new System.Diagnostics.ProcessStartInfo("cmd", "/c b\\win32\\vc_dll.bat");
         i.WorkingDirectory = LzoFile;
         var lzoProc = System.Diagnostics.Process.Start(i);

         // building ZLib...
         Console.WriteLine("Building ZLib...");
         System.Diagnostics.ProcessStartInfo i2 = new System.Diagnostics.ProcessStartInfo("cmd", "/c bld_ml32.bat");
         i2.WorkingDirectory = ZLibFile + "/contrib/masmx86";
         var zlibProc1 = System.Diagnostics.Process.Start(i2);

         // wait for the first part of the ZLib building to finish...
         zlibProc1.WaitForExit();

         // now step 2 of the ZLib building...
         var zlibProc2 = RunMSBuild("zlibvc.vcxproj", ZLibFile + "/contrib/vstudio/vc10", false);

         Console.WriteLine("Waiting for ZLib and LZO to finish...");
         zlibProc2.WaitForExit();
         lzoProc.WaitForExit();

         // all done!
         Console.WriteLine("LZO and ZLib are finished!");

         Console.WriteLine("Building LibBarn...");
         BuildLibBarn();
      }

      static void BuildLibBarn()
      {
         RunMSBuild("libbarn2010.vcxproj", "libbarn", false);
      }

      static System.Diagnostics.Process RunMSBuild(string projectFile, string workingDirectory, bool debug)
      {
         System.Diagnostics.ProcessStartInfo p = new System.Diagnostics.ProcessStartInfo("msbuild", projectFile + 
            (debug ? " p:/Configuration=Debug" : " /p:Configuration=Release"));
         p.WorkingDirectory = workingDirectory;
         return System.Diagnostics.Process.Start(p);
      }
   }
}
