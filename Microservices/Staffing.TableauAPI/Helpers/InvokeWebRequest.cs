using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Staffing.TableauAPI.Helpers
{
    public class InvokeWebRequest
    {

        public string GetWebResponse()
        {
            using (XmlWriter loginxml = XmlWriter.Create("login.xml"))
            {
                loginxml.WriteStartDocument();
                loginxml.WriteStartElement("tsRequest");
                loginxml.WriteStartElement("credentials");
                loginxml.WriteAttributeString("name", "54474@bain.com");
                loginxml.WriteAttributeString("password", "Veera123");
                loginxml.WriteStartElement("site");
                loginxml.WriteAttributeString("contentUrl", "");
                loginxml.WriteEndElement();
                loginxml.WriteEndElement();
                loginxml.WriteEndElement();
                loginxml.WriteEndDocument();
            }
            XElement myxml = XElement.Load("login.xml");


            //Convert the XML payload to a string and display so we can check that it's well-formed
            string myxmlstring = myxml.ToString();
            System.Console.WriteLine(myxmlstring);
            System.Console.WriteLine();


            //send payload to routine to make the web request
            string response = LoginToTableau(myxmlstring);


            //display the response from the web request
            return response;
            //System.Console.WriteLine(response);
        }
        static string LoginToTableau(string xml)
        {
            //Is this the correct url we should be sending the web request to?
            string urltl = "https://tableau.bain.com/api/3.4/auth/signin";
            //Send the above url, the POST method, and the XML Payload string to create the web request
            var infotl = SendWebRequest(urltl, "POST", xml);
            return infotl;
        }
        static string SendWebRequest(string Url, string Method, string payload)
        {
            string response;
            //encode the XML payload
            byte[] buf = Encoding.UTF8.GetBytes(payload);
            //set the system to ignore certificate errors because Tableau server has an invalid cert.
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            //Create the web request and add the XML payload
            HttpWebRequest wc = WebRequest.Create(Url) as HttpWebRequest;
            wc.Method = Method;
            wc.ContentType = "text/xml";
            wc.ContentLength = buf.Length;
            wc.GetRequestStream().Write(buf, 0, buf.Length);
            try
            {
                //Send the web request and parse the response into a string
                HttpWebResponse wr = wc.GetResponse() as HttpWebResponse;
                Stream receiveStream = wr.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                response = readStream.ReadToEnd();
                receiveStream.Close();
                readStream.Close();
                wr.Close();
                readStream.Dispose();
                receiveStream.Dispose();
            }
            catch (WebException we)
            {
                //Catch failed request and return the response code
                response = ((HttpWebResponse)we.Response).StatusCode.ToString();
            }
            return response;
        }
    }
}
