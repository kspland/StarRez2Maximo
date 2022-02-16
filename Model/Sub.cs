using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Xml;
using System.IO;


namespace StarRezMaintenanceV6.Model
{
    class Sub
    {
        public static string DataRequest(string PostData, string uname, string pword, string method, string url)
        {

            string results;
            string authInfo = uname + ":" + pword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

            //Create a request using a URL that can receive a post.
            WebRequest request = WebRequest.Create(url);

            //Set the Method property of the request to POST.
            request.Method = method;

            //Set the basic credentials and authorization
            request.Headers["Authorization"] = "Basic " + authInfo;

            //Create POST data and convert it to a byte array.
            byte[] byteArray = Encoding.UTF8.GetBytes(PostData);

            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream
            Stream dataStream = request.GetRequestStream();

            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);

            //Close the Stream object.
            dataStream.Close();

            //Get the response.
            WebResponse response = request.GetResponse();

            //Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            //Get the stream containing content returned by the server.
            //The using block ensures the stream is automatically closed.
            using (dataStream = response.GetResponseStream())
            {
                //Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                //Read the content.
                string responseFromServer = reader.ReadToEnd();

                //Saver results from post into string to return out of program.
                results = responseFromServer;

                //Display the content.
                Console.WriteLine(responseFromServer);
            }

            //Close the response.
            response.Close();

            return results;
        }
    }


    public class mwo
    {
        public string orgid { get; set; }   // Maximo Orginazation ID; Must be included on
        public string siteid { get; set; }  // Maximo SiteID 
        public string name { get; set; }    // Resident name currently InRoom
        public string email { get; set; }   // Resident email address in StarRez
        public string phone { get; set; }   // Resident may enter a phone number in StarRez Work Order; field is optional
        public string starrezid { get; set; } // Primary Key used to identifiy each Work Order in StarRez Maintenance Table.
        public string failurecode { get; set; } // Corresponding Maximo code to type of problem;No longer used
        public string problemcode { get; set; } // Corresponding Maixmo code to work type; No longer used
        public string location { get; set; }    // Resident Room Location 
        
        public string description { get; set; }  // Description of problem or issue by Resident/Staff
    }
}
   