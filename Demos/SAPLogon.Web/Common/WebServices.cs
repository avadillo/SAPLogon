using SAPTools.LogonTicket;
using System.Data;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace SAPLogon.Web.Common;
public static class WebServices {
    public static string CertSubject { get; set; } = "OU=SAP Tools, CN=SAP SSO RSA 4096";
    public static string Language { get; set; } = "EN";

    private static readonly Lazy<Task<List<SAPUser>>> _webGUIUsers = new(() => GetUserList("WEBGUI"));
    private static readonly Lazy<Task<List<SAPUser>>> _webServiceUsers = new(() => GetUserList("WEBSERVICE"));

    public static Task<List<SAPUser>> WebGUIUsers => _webGUIUsers.Value;
    public static Task<List<SAPUser>> WebServiceUsers => _webServiceUsers.Value;

    private const string Url = "https://sapnwa.saptools.mx/sap/bc/srt/rfc/sap/zssodemo/752/ssodemo/services";
    private const string ConnectionUser = "WSUSER";
    private const string RcptSysId = "NWA";
    private const string RcptSysClient = "752";


    private static async Task<List<SAPUser>> GetUserList(string userGroup) {
        List<SAPUser> list = new List<SAPUser>();
        string soapString = CreateUserRequest(userGroup);

        using(HttpClient client = new HttpClient()) {
            client.DefaultRequestHeaders.Add("SOAPAction", "urn:sap-com:document:sap:soap:functions:mc-style:ZSSODEMO:ZGetUsersByGroupRequest");
            client.DefaultRequestHeaders.Add("MYSAPSSO2", await GetMySAPSSO2());
            using(HttpContent content = new StringContent(soapString, Encoding.UTF8, "text/xml")) {
                HttpResponseMessage response = await client.PostAsync(Url, content);
                if(response.StatusCode == HttpStatusCode.OK) {
                    string responseString = await response.Content.ReadAsStringAsync();
                    DataTable dt = ParseResponse(responseString);
                    foreach(DataRow row in dt.Rows) {
                        list.Add(new SAPUser() {
                            Bname = row["Bname"]?.ToString() ?? "", // Handle possible nulls
                            NameText = row["NameText"]?.ToString() ?? "" // Handle possible nulls
                        });
                    }
                }
            }
        }
        return list;
    }

    private static DataTable ParseResponse(string responseString) {
        DataTable dt = new DataTable();
        dt.Columns.Add("Bname", typeof(string));
        dt.Columns.Add("NameText", typeof(string));

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(responseString);

        // Assuming the response XML namespace as "urn:sap-com:document:sap:soap:functions:mc-style"
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("d", "urn:sap-com:document:sap:soap:functions:mc-style");
        nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");

        // Adjust the XPath query according to the actual structure of your SOAP response
        // Example adjustment: navigating through the soap envelope to the actual data
        XmlNodeList? userNodes = xmlDoc.SelectNodes("//soapenv:Envelope/soapenv:Body/d:ZGetUsersByGroupResponse/Names/item", nsmgr);

        if(userNodes != null) {
            foreach(XmlNode node in userNodes) {
                DataRow dr = dt.NewRow();
                dr["Bname"] = node.SelectSingleNode("Bname", nsmgr)?.InnerText ?? "";
                dr["NameText"] = node.SelectSingleNode("NameText", nsmgr)?.InnerText ?? "";
                dt.Rows.Add(dr);
            }
        }

        return dt;
    }


    public static async Task<string> GetMySAPSSO2() {
        X509Certificate2 cert = await UserCertificates.GetCertificateBySubject(CertSubject);
        var (sysId, sysClient) = await UserCertificates.GetTypeAndPosition(cert.Thumbprint);
        AssertionTicket ticket = new() {
            User = ConnectionUser,
            SysID = sysId,
            SysClient = sysClient,
            RcptSysID = RcptSysId,
            RcptSysClient = RcptSysClient,
            Language = SAPTools.LogonTicket.Extensions.SAPLanguage.EN,
            Certificate = cert
        };
        return ticket.Create(); // Await the Create method if it's asynchronous
    }


    private static string CreateUserRequest(string userGroup) => $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:soap:functions:mc-style"">
   <soapenv:Header/>
   <soapenv:Body>
      <urn:ZGetUsersByGroup>
         <Names>
            <!--Zero or more repetitions:-->
            <item>
               <Bname></Bname>
               <NameText></NameText>
            </item>
         </Names>
         <UserGroup>{userGroup}</UserGroup>
      </urn:ZGetUsersByGroup>
   </soapenv:Body>
</soapenv:Envelope>
";
}

public class SAPUser {
    public string Bname { get; set; } = "";
    public string NameText { get; set; } = "";
}
