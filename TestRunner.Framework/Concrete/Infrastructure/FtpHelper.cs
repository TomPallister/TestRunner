using System.IO;
using System.Net;
using System.Text;

namespace TestRunner.Framework.Concrete.Infrastructure
{
    public static class FtpHelper
    {
        public static void UploadFile(string hostFullPathToSaveLocation, string inputFileLocationAndName, string userName = "Anonymous", string password = "")
        {
            var request = (FtpWebRequest)WebRequest.Create(hostFullPathToSaveLocation);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(userName, password);
            var sourceStream = new StreamReader(inputFileLocationAndName);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();
            var response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }

        public static void UploadString(string hostFullPathToSaveLocation, string inputString, string userName = "Anonymous", string password = "")
        {
            var request = (FtpWebRequest)WebRequest.Create(hostFullPathToSaveLocation);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(userName, password);

            byte[] byteArray = Encoding.UTF8.GetBytes(inputString);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            var sourceStream = new StreamReader(stream);

            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();
            var response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
    }
}
