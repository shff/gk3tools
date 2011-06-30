using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace Builder
{
   class Downloader
   {
      public static void Download(string url)
      {
         Download(url, Path.GetFileName(url));
      }

      public static void Download(string url, string outputFile)
      {
         using (WebClient client = new WebClient())
         {
            var source = client.OpenRead(url);
            var destination = File.Create(outputFile);

            copyTo(source, destination);

            source.Close();
            destination.Close();
         }
      }

      private static void copyTo(Stream source, Stream destination)
      {
         byte[] buffer = new byte[1024];

         int n;
         do
         {
            n = source.Read(buffer, 0, buffer.Length);
            destination.Write(buffer, 0, n);
         } while (n != 0);
      }
   }
}
