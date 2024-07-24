using SAPTools.Ticket;
using SAPTools.Ticket.Extensions;
using System.Data;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace SAPLogon.Web.Common;
public static class Catalogs {
    /// <summary>
    /// Certificate subject to use for the MYSAPSSO2 token
    /// </summary>
    public static string CertSubject { get; set; } = "OU=SAP Tools, CN=SAP SSO ECDSA P-256";
    /// <summary>
    /// Language to use for the SAP system calls
    /// </summary>
    public static SAPLanguage Language { get; set; } = SAPLanguage.EN;

    private static Lazy<Task<List<SAPUser>>> _webGUIUsers = new(() => GetUsersByGroup("WEBGUI"));
    private static Lazy<Task<List<SAPUser>>> _webServiceUsers = new(() => GetUsersByGroup("WEBSERVICE"));
    private static Lazy<Task<List<SAPLang>>> _installedLangs = new(() => GetInstalledLanguages());

    /// <summary>
    /// Gets an asynchronous task that returns a list of SAP users belonging to the "WEBGUI" user group.
    /// This property utilizes a lazy initialization pattern to ensure that the list is fetched only once and only when needed.
    /// </summary>
    /// <value>
    /// A task that, when awaited, returns a <see cref="List{SAPUser}"/> representing the users in the "WEBGUI" group.
    /// </value>
    public static Task<List<SAPUser>> WebGUIUsers => _webGUIUsers.Value;

    /// <summary>
    /// Gets an asynchronous task that returns a list of SAP users belonging to the "WEBSERVICE" user group.
    /// Similar to <see cref="WebGUIUsers"/>, this property uses lazy initialization to fetch the user list efficiently.
    /// </summary>
    /// <value>
    /// A task that, when awaited, returns a <see cref="List{SAPUser}"/> representing the users in the "WEBSERVICE" group.
    /// </value>
    public static Task<List<SAPUser>> WebServiceUsers => _webServiceUsers.Value;

    /// <summary>
    /// Gets an asynchronous task that returns a list of languages installed in the SAP system.
    /// This property uses lazy initialization to ensure that the list of languages is fetched and parsed only once upon first access.
    /// </summary>
    /// <value>
    /// A task that, when awaited, returns a <see cref="List{SAPLang}"/> representing the installed languages in the SAP system.
    /// </value>
    public static Task<List<SAPLang>> InstalledLanguages => _installedLangs.Value;

    
    public static void ResetInstalledLanguages() => _installedLangs = new(() => GetInstalledLanguages());
    public static void ResetWebGUIUsers() => _webGUIUsers = new(() => GetUsersByGroup("WEBGUI"));
    public static void ResetWebServiceUsers() => _webServiceUsers = new(() => GetUsersByGroup("WEBSERVICE"));

    private const string Url = "https://demo.saptools.mx/sap/bc/srt/rfc/sap/zssodemo/752/ssodemo/services";
    private const string ConnectionUser = "WSUSER";
    private const string RcptSysId = "NWA";
    private const string RcptSysClient = "752";

    public static async Task<List<SAPUser>> GetUsersByGroup(string userGroup) {
        string soapString = GetUsersByGroupEnv(userGroup);
        string soapAction = "urn:sap-com:document:sap:soap:functions:mc-style:ZSSODEMO:ZGetUsersByGroupRequest";
        string xPathQuery = "//soapenv:Envelope/soapenv:Body/d:ZGetUsersByGroupResponse/Names/item";

        return await FetchSAPData<SAPUser>(soapAction, soapString, xPathQuery, ["Bname", "NameText"], row => new SAPUser {
            User = row["Bname"]?.ToString() ?? "",
            FullName = row["NameText"]?.ToString() ?? ""
        });
    }

    public static async Task<List<SAPLang>> GetInstalledLanguages() {
        string soapString = GetInstalledLanguagesEnv();
        string soapAction = "urn:sap-com:document:sap:soap:functions:mc-style:ZSSODEMO:ZGetInstalledLanguagesRequest";
        string xPathQuery = "//soapenv:Envelope/soapenv:Body/d:ZGetInstalledLanguagesResponse/TLangu/item";

        return await FetchSAPData<SAPLang>(soapAction, soapString, xPathQuery, ["Spras", "Laiso", "Sptxt"], row => new SAPLang {
            SAPCode = row["Spras"]?.ToString() ?? "",
            ISOCode = row["Laiso"]?.ToString() ?? "",
            Name = row["Sptxt"]?.ToString() ?? "",
        });
    }

    private static async Task<List<T>> FetchSAPData<T>(string soapAction, string soapString, string xPathQuery, string[] columns, Func<DataRow, T> createObject) {
        List<T> list = [];

        DataTable dt = await FetchAndParseSoapResponse(soapAction, soapString, xPathQuery, columns);

        foreach(DataRow row in dt.Rows) {
            list.Add(createObject(row));
        }

        return list;
    }

    /// <summary>
    /// Fetches the SOAP response and parses it into a DataTable.
    /// This method sends a SOAP request to a specified URL with given parameters and parses the XML response.
    /// </summary>
    /// <param name="soapAction">The SOAP action header value, specifying the action to be performed by the web service.</param>
    /// <param name="soapString">The SOAP request XML string.</param>
    /// <param name="xPathQuery">The XPath query used to extract relevant data from the SOAP response.</param>
    /// <param name="columns">The columns to be included in the resulting DataTable, corresponding to the data extracted using the XPath query.</param>
    /// <returns>A DataTable containing the data extracted from the SOAP response based on the specified XPath query and columns.</returns>
    private static async Task<DataTable> FetchAndParseSoapResponse(string soapAction, string soapString, string xPathQuery, IEnumerable<string> columns) {
        // Initialize a new HttpClient instance for sending the SOAP request.
        using(HttpClient client = new()) {
            // Add the SOAPAction header, which is required for SOAP requests.
            client.DefaultRequestHeaders.Add("SOAPAction", soapAction);
            // Add the MYSAPSSO2 header, likely for authentication purposes, obtained by calling GetMySAPSSO2 method.
            client.DefaultRequestHeaders.Add("MYSAPSSO2", GetMySAPSSO2());

            // Create the HTTP content from the soapString, specifying the content type as text/xml.
            using(HttpContent content = new StringContent(soapString, Encoding.UTF8, "text/xml")) {
                // Send the SOAP request asynchronously and wait for the response.
                HttpResponseMessage response = await client.PostAsync(Url, content);

                // Check if the response status code indicates success (HTTP 200 OK).
                if(response.StatusCode == HttpStatusCode.OK) {
                    // Read the response content as a string asynchronously.
                    string responseString = await response.Content.ReadAsStringAsync();
                    // Parse the SOAP response XML into a DataTable and return it.
                    return ParseSoapResponse(responseString, xPathQuery, columns);
                }
            }
        }
        // If the request fails (e.g., due to a network error or an error response from the web service), return an empty DataTable.
        return new DataTable();
    }

    /// <summary>
    /// Parses the SOAP response XML into a DataTable.
    /// </summary>
    /// <param name="responseString">The SOAP response XML as a string.</param>
    /// <param name="xpathQuery">The XPath query used to select nodes from the XML that contain the desired data.</param>
    /// <param name="columns">A collection of column names that correspond to the data to be extracted from the XML nodes.</param>
    /// <returns>A DataTable containing the data extracted from the SOAP response XML.</returns>
    public static DataTable ParseSoapResponse(string responseString, string xpathQuery, IEnumerable<string> columns) {
        DataTable dt = new();
        // Add columns to the DataTable based on the specified column names.
        foreach(var column in columns) {
            dt.Columns.Add(column, typeof(string));
        }

        XmlDocument xmlDoc = new();
        // Load the SOAP response XML into the XmlDocument.
        xmlDoc.LoadXml(responseString);

        // Define namespaces to be used in the XPath query.
        XmlNamespaceManager nsmgr = new(xmlDoc.NameTable);
        nsmgr.AddNamespace("d", "urn:sap-com:document:sap:soap:functions:mc-style");
        nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");

        // Select nodes from the XML based on the XPath query.
        XmlNodeList? nodes = xmlDoc.SelectNodes(xpathQuery, nsmgr);

        if(nodes != null) {
            // Iterate through each node selected by the XPath query.
            foreach(XmlNode node in nodes) {
                DataRow dr = dt.NewRow();
                // For each specified column, extract the corresponding data from the node and add it to the DataRow.
                foreach(var column in columns) {
                    dr[column] = node.SelectSingleNode(column, nsmgr)?.InnerText ?? "";
                }
                // Add the DataRow to the DataTable.
                dt.Rows.Add(dr);
            }
        }
        // Return the DataTable containing the extracted data.
        return dt;
    }

    /// <summary>
    /// Gets the signing certificate by subject and creates the MYSAPSSO2 token
    /// </summary>
    /// <returns>A Base64-encoded MYSAPSSO2 Token</returns>
    public static string GetMySAPSSO2() {
        AssertionTicket ticket = new() {
            User = ConnectionUser,
            SysID = "ECDSA",
            SysClient = "000",
            RcptSysID = RcptSysId,
            RcptSysClient = RcptSysClient,
            Language = Language,
            Subject = "OU=SAP Tools, CN=SAP SSO ECDSA P-256",
        };
        return ticket.Create(); // Await the Create method if it's asynchronous
    }

    /// <summary>
    /// Result from the WS call to get the users by group
    /// </summary>
    public class SAPUser {
        /// <summary>
        /// SAP User ID (BNAME) up to 12 characters
        /// </summary>
        public string User { get; set; } = "";
        /// <summary>
        /// Full name of the user
        /// </summary>
        public string FullName { get; set; } = "";
    }

    /// <summary>
    /// Result from the WS call to get the installed languages
    /// </summary>
    public class SAPLang {
        /// <summary>
        /// Language key (SPRAS)
        /// </summary>
        public string SAPCode { get; set; } = "";
        /// <summary>
        /// ISO Language Code (ISO 639-1)
        /// </summary>
        public string ISOCode { get; set; } = "";
        /// <summary>
        /// Language name
        /// </summary>
        public string Name { get; set; } = "";
    }

    public static string GetSystemInfoEnv() => $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:soap:functions:mc-style""><soapenv:Header/><soapenv:Body><urn:ZGetSystemInfo/></soapenv:Body></soapenv:Envelope>";
    public static string GetInstalledLanguagesEnv() => $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:soap:functions:mc-style""><soapenv:Header/><soapenv:Body><urn:ZGetInstalledLanguages><TLangu><item><Spras>?</Spras><Laiso>?</Laiso><Sptxt>?</Sptxt></item></TLangu></urn:ZGetInstalledLanguages></soapenv:Body></soapenv:Envelope>";
    public static string GetUsersByGroupEnv(string userGroup) => $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:sap-com:document:sap:soap:functions:mc-style""><soapenv:Header/><soapenv:Body><urn:ZGetUsersByGroup><Names><item><Bname></Bname><Class></Class><NameText></NameText></item></Names><UserGroup>{userGroup}</UserGroup></urn:ZGetUsersByGroup></soapenv:Body></soapenv:Envelope>";
}