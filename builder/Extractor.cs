using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Builder
{
   class Extractor
   {
      public static void Extract(string filename)
      {
         Extract(filename, Path.GetFileNameWithoutExtension(filename));
      }

      public static void Extract(string filename, string outputFilename)
      {
         using (GZipStream input = new GZipStream(File.OpenRead(filename), CompressionMode.Decompress))
         {
            MemoryStream m = new MemoryStream();
            copyTo(input, m);
            m.Seek(0, SeekOrigin.Begin);

            tar_cs.TarReader reader = new tar_cs.TarReader(m);

            while (reader.MoveNext(false))
            {
               if (reader.FileInfo.FileName.EndsWith("/"))
                  Directory.CreateDirectory(reader.FileInfo.FileName);
               else
               {
                  Stream output = File.Create(reader.FileInfo.FileName);
                  reader.Read(output);
                  output.Close();
               }
            }

            m.Dispose();
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
