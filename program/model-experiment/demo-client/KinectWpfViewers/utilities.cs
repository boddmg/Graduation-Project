using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.WpfViewers
{
    class Utilities
    {
        public static byte[] CompressBytes(byte[] data)
        {
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
                zip.Write(data, 0, data.Length);
                zip.Close();
                byte[] buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
                ms.Close();
                return buffer;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static String CompressString(String str)  
        {     
            string compressString = "";  
            byte[] compressBeforeByte = Encoding.GetEncoding("UTF-8").GetBytes(str);  
            byte[] compressAfterByte=CompressBytes(compressBeforeByte);  
            compressString = Convert.ToBase64String(compressAfterByte);  
            return compressString;  
        }  


    }
}
