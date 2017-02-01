using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ChannelAdam.Soap;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace WSFOAUTHREST.Controllers
{
    public class ProgressController : ApiController
    {

        // GET: api/Progress/5
        public IEnumerable<task> Get(int id)
        {
            var accessToken = GetAccessToken("PJCC3", "PASSWORD");
            var RES = GetProgress(id, accessToken);
            var parse = Regex.Split(RES, "[0-9]{6}=").Where(a => a != "").Select(a => 
                new task () { Name = a.Split('#')[0].Split('=')[1].Replace("!",""), Progress = a.Split('#')[1].Split('=')[1].Replace("!", "") } );
            var tasks = parse.Where(a => a.Name != "" && a.Name != "Summary").ToList();
            tasks.Add(new task()
            {
                Name = "Summary",
                Progress = (parse.Where(a => a.Name != "" && a.Name != "Summary").Average(a => Convert.ToDouble(a.Progress))).ToString()
            });
            return tasks;
        }


        public static HttpWebRequest CreateWebRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"https://wsv-srssnd.le.ac.uk/sand/sits.srd/siw_oauth");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static string GetAccessToken(string serviceAcct, string serviceAcctPass)
        {
            HttpWebRequest request = CreateWebRequest();
            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:uniface:applic:wrapped:services:SIW_OAUTH"">
<soapenv:Header />
<soapenv:Body>
<urn:GRANT_ACCESS_TOKEN>
<urn:GRANT_TYPE>password</urn:GRANT_TYPE>
<urn:USERNAME>" + serviceAcct + @"</urn:USERNAME>
<urn:PASSWORD>" + serviceAcctPass + @"</urn:PASSWORD>
<urn:TOKEN_SCOPE>ENR_PROGRESS</urn:TOKEN_SCOPE>
</urn:GRANT_ACCESS_TOKEN>
</soapenv:Body>
</soapenv:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
            string soapResult = string.Empty;
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    soapEnvelopeXml.LoadXml(rd.ReadToEnd());
                }
            }

            return soapEnvelopeXml.GetElementsByTagName("ACCESS_TOKEN").Item(0).InnerText;
        }

        public static string GetProgress(int id, string token)
        {
            HttpWebRequest request = CreateWebRequest();
            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:uniface:applic:wrapped:services:SIW_WSF"">
<soapenv:Header/>
<soapenv:Body>
<urn:OAUTH_ACTION>
<urn:TOKEN>" + token + @"</urn:TOKEN>
<urn:FUNCTION>ENR_PROGRESS</urn:FUNCTION>
<urn:PARAMETERS></urn:PARAMETERS>
<urn:INDATA></urn:INDATA>
</urn:OAUTH_ACTION>
</soapenv:Body>
</soapenv:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
            string soapResult = string.Empty;
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    soapEnvelopeXml.LoadXml(rd.ReadToEnd());



                }
            }
            return soapEnvelopeXml.GetElementsByTagName("OUTDATA").Item(0).InnerText;
        }
    }
    public class task
    {
        public string Name { get; set; }
        public string Progress { get; set; }
    }
}
