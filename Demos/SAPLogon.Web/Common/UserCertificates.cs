using System.Security.Cryptography.X509Certificates;

namespace SAPLogon.Web;
public static class UserCertificates {
    // Read the certificates from the CurrentUser\My store and sort them by algorithm
    // This is executed only once when the class is loaded
    public static Task<List<X509Certificate2>> Certificates => _certificates.Value;

    private static Lazy<Task<List<X509Certificate2>>> _certificates = new(GetCertificatesAsync);

    private static async Task<List<X509Certificate2>> GetCertificatesAsync() => await Task.Run(() => {
        AlgComparer comparer = new();
        X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        List<X509Certificate2> certList = [.. store.Certificates
            .Find(X509FindType.FindBySubjectName, "SAP Tools", false)
            .Cast<X509Certificate2>()
            .OrderBy(x => x.Subject, comparer)]; // For demo purposes, sort the certificates by algorithm
        store.Close();
        return certList;
    });

    // Refresh the certificates list
    public static void Refresh() => _certificates = new Lazy<Task<List<X509Certificate2>>>(GetCertificatesAsync);

    public static async Task<X509Certificate2> GetCertificate(string thumbprint) =>
        (await Certificates).FirstOrDefault(cert => cert.Thumbprint == thumbprint) ?? throw new Exception("Certificate not found");

    public static async Task<X509Certificate2> GetCertificateBySubject(string subject) =>
        (await Certificates).FirstOrDefault(cert => cert.Subject == subject) ?? throw new Exception("Certificate not found");

    public static async Task<string> GetThumbprintBySubject(string subject) =>
        (await Certificates).FirstOrDefault(cert => cert.Subject == subject)?.Thumbprint ?? throw new Exception("Certificate not found");

    // Sorts the certificates by algorithm: DSA, RSA, ECDSA
    // This is only for demo purposes. The real implementation do not need to sort the certificates.
    private class AlgComparer : IComparer<object> {
        public int Compare(object? stringA, object? stringB) {
            string string1 = stringA?.ToString()!.Replace("DSA", "1").Replace("RSA", "2").Replace("ECDSA", "3") ?? "";
            string string2 = stringB?.ToString()!.Replace("DSA", "1").Replace("RSA", "2").Replace("ECDSA", "3") ?? "";
            return string1.CompareTo(string2); // Fixed comparison to compare string1 with string2
        }
    }

    public static async Task<(string, string)> GetTypeAndPosition(string thumbprint) {
        // Get artificial values for SID and Client to send the ticket
        // This is just for building the certificate list for demo purposes
        var cert = await GetCertificate(thumbprint);
        string alg = cert.PublicKey.Oid.Value switch {
            "1.2.840.10040.4.1" => "DSA",
            "1.2.840.113549.1.1.1" => "RSA",
            "1.2.840.10045.2.1" => "ECDSA",
            _ => throw new Exception("Invalid certificate algorithm")
        };

        // Filter the certificates by algorithm and get the index of the thumbprint
        var certificates = await Certificates;
        int index = certificates.Where(x => x.PublicKey.Oid.Value == cert.PublicKey.Oid.Value)
                                .ToList()
                                .FindIndex(x => x.Thumbprint == thumbprint);

        return (alg, $"{index:D3}");
    }

    // Construct the value for the WEBSITE_LOAD_CERTIFICATES environment variable, needed for Azure Web Apps
    public static async Task<string> GetWebsiteLoadCertificates() =>
        "WEBSITE_LOAD_CERTIFICATES=" + String.Join(",", (await Certificates).Select(cert => cert.Thumbprint));

}
