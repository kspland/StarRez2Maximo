using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using System.Net;
using System.Web;
using StarRezMaintenanceV6.Model;


namespace StarRezMaintenanceV6
{
    class Program
    {
        static void Main(string[] args)
        {
         
            /*  This program reads a data file of maintenance workorders from StarRez and creates workorders in 
             *  Maximo via Maximo Rest Services. The program then updates the StarRez Maintneance table to Job Sent
             *  via  StarRez Rest Services. Maximo has alimit of 600 charcters for the description. Location names
             *  have been placed on the roomspace customield for each roomspace in StarRez. If a student is inroom
             *  their name,location and email address will be export. They have an option of entering a contact phone
             *  number. If the room is empty and StarRez Maintenance  work order is created by Housing app the
             *  name will be null with a location and description of the is given. 
             * 
             * 
             * 
             * */

                string lead; // Declare lead variable for Maximo

                string supervisor; // Declare lead variable for Maxmo


            //File path to location StarRez Maintenance file is exported
           //File is name is Maximo concatenated with date and time as a text file
             string file_name = @"\\(servername)\$SRuea\Maintenance\Maximo-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            // File path when testing or running jobs directly from PC
           // string file_name = @"C:\Users\\Desktop\Maximo-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            //Create a list with StarRez Work order data for each line of input
            // fiels are seperated by a |
            List<mwo> _wo = new List<mwo>();
                using (var reader = new StreamReader(file_name))
                using (var csvReader = new CsvReader(reader,System.Globalization.CultureInfo.CurrentCulture))
                {
                    csvReader.Configuration.Delimiter = "|";
                    csvReader.Configuration.MissingFieldFound = null;
                    csvReader.Configuration.ReadingExceptionOccurred = null;
                    csvReader.Configuration.HasHeaderRecord = false;
                    csvReader.Configuration.BadDataFound = null;



                    while (csvReader.Read())
                    {
                        mwo Record = csvReader.GetRecord<mwo>();
                        _wo.Add(Record);
                    }
                }

                foreach (mwo m in _wo)
                {


                    // Create a request using a URL that can receive a post.   
                    // WebRequest request = WebRequest.Create( Global.MaximoTestBaseURL + "/?_action=AddChange&");
                    WebRequest request = WebRequest.Create(Global.MaximoProdBaseURL + "/?_action=AddChanges&");

                    //Set the authorization security level and id 
                    // Credentials are in Base64 format with username:password decoded
                    request.Headers.Add("authorization", "Basic username:password");


                    // Set the Method property of the request to POST.  
                    request.Method = "POST";


                    //If NGW location set Rise as the lead and Supervisor
                    // NGW locations are fully serviced by Rise, IF location of Workorder
                    // is among those set the Lead and Supervisor to Rise. They 
                    // are filtered out by Rise once created in Maximo
                    if (string.Compare(m.location.Substring(0, 4), "NGDL") == 0
                        || string.Compare(m.location.Substring(0, 4), "NGCN") == 0
                        || string.Compare(m.location.Substring(0, 4), "NGBY") == 0
                        || string.Compare(m.location.Substring(0, 4), "NGGF") == 0
                        || string.Compare(m.location.Substring(0, 4), "NGOX") == 0
                        || string.Compare(m.location.Substring(0, 4), "NGMR") == 0
                        || string.Compare(m.location.Substring(0, 4), "NGRV") == 0)
                    {
                        lead = "RISE";
                        supervisor = "RISE";
                    }
                    else // Set lead and Supervisor to blank if non NGW buildings.
                    {
                        lead = "";
                        supervisor = "";
                    }

                    // Build the data string



                    string postData = "Description=" + m.description + "&FAILURECODE=" + m.failurecode +
                        "&LOCATION=" + m.location + "&ORGID=" + m.orgid + "&PROBLEMCODE="
                        + "&WO11=" + m.email + "&WOL1=" + m.name + "&WO11=" + m.email + "&WOLO1=" + m.phone
                        + "&SITEID=" + m.siteid + "&STATUS=WAPPR" + "&STARREZID=" + m.starrezid + "&SUPERVISOR=" + supervisor + "&LEAD=" + lead;

                    // Create POST data and convert it to a byte array.  
                    //string postData = "This is a test that posts this string to a Web server.";


                    //Convert the data into a byte array
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);


                    // Set the ContentType property of the WebRequest.  
                    request.ContentType = "application/x-www-form-urlencoded";

                    // Set the ContentLength property of the WebRequest.  
                    request.ContentLength = byteArray.Length;


                    // Get the request stream.  
                    Stream dataStream = request.GetRequestStream();

                    // Write the data to the request stream.  
                    dataStream.Write(byteArray, 0, byteArray.Length);



                    // Close the Stream object.  
                    dataStream.Close();

                    // Get the response.  
                    WebResponse response = request.GetResponse();



                    // Display the status.  
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    if (((HttpWebResponse)response).StatusDescription == "OK")
                    {
                        Console.WriteLine("WO Created in Maximo");
                    string username = Global.StarRezusername;
                        string password = Global.StarRezpassword;

                        //StarRez Production
                        string URL = Global.StarRezProdBaseURL + "/update/RoomSpaceMaintenance/" + m.starrezid;

                        //StarRez Test
                       // string URL =  Global.StarRezTestBaseURL + "/update/RoomSpaceMaintenance/" + m.starrezid;
                        string method = "POST";
                        HttpStatusCode sstatus;
                        string RESULTS;

                        //Build string data for Updating RoomSpaceMaintenance table once job has been sent to Maximo
                        // Set JobSent to True and Indicate Status of StarRez WO to Sent to Res Life Maintenance
                        string postdata = string.Format(@"<RoomSpaceMaintenance>
                                                            <JobSent>1</JobSent>
                                                            <JobStatus>Sent to Res Life Maintena</JobStatus>
                                                      </RoomSpaceMaintenance>");

                        var Status = Sub.DataRequest(postdata, username, password, method, URL);

                    //Write out to verify status when observing job run.
                        Console.WriteLine("Status is {0}", Status);
                    }
                    // Get the stream containing content returned by the server.  
                    dataStream = response.GetResponseStream();


                    // Open the stream using a StreamReader for easy access.  
                    StreamReader reader = new StreamReader(dataStream);


                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();



                    // Display the content.  
                    Console.WriteLine(responseFromServer);
                    //Console.ReadKey();

                    // Clean up the streams.  
                    reader.Close();

                    dataStream.Close();

                    response.Close();

                    // Writeline of description of problem, Resident email, room location and Name.
                    //This was used to observe the data while troubleshooting issues.
                    Console.WriteLine("Description:{0} email:{1} location:{2} Name:{3}", m.description, m.email, m.location, m.name);
                    Console.WriteLine("-----");
                }



            }

        }
    }
