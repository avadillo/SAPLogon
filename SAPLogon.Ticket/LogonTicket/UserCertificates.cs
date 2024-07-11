using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace SAPTools.LogonTicket;
public static class UserCertificates
{
    // Read the certificates from the CurrentUser\My store and sort them by algorithm
    // This is executed only once when the class is loaded
    public static List<X509Certificate2> Certificates { get; set; } = GetCertificates();

    private static List<X509Certificate2> GetCertificates() {
        AlgComparer comparer = new();
        X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        List<X509Certificate2> certList = [.. store.Certificates.Find(X509FindType.FindBySubjectName, "SAP Tools", false)
            .OrderBy(x => x.Subject, comparer)];
        store.Close();
        return certList;
    }

    public static void Refresh() => Certificates = GetCertificates();

    public static X509Certificate2 GetCertificate(string thumbprint) =>
        Certificates.FirstOrDefault(cert => cert.Thumbprint == thumbprint)?? throw new Exception("Certificate not found");

    public static X509Certificate2 GetCertificateBySubject(string subject) =>
        Certificates.FirstOrDefault(cert => cert.Subject == subject) ?? throw new Exception("Certificate not found");

    public static string GetThumbprintBySubject(string subject) =>
        Certificates.FirstOrDefault(cert => cert.Subject == subject)?.Thumbprint ?? throw new Exception("Certificate not found");

    // Sorts the certificates by algorithm: DSA, RSA, ECDSA
    private class AlgComparer : IComparer<object> {
        public int Compare(object? stringA, object? stringB) {
            string string1 = stringA?.ToString()!.Replace("DSA", "1").Replace("RSA", "2").Replace("ECDSA", "3") ?? "";
            string string2 = stringB?.ToString()!.Replace("DSA", "1").Replace("RSA", "2").Replace("ECDSA", "3") ?? "";
            return string1.CompareTo(string2); // Fixed comparison to compare string1 with string2
        }
    }

    public static void PrintCertificates()
    {
        foreach (X509Certificate2 cert in Certificates)
        {
            Console.WriteLine($"Subject: {cert.Subject}");
            Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
            Console.WriteLine($"Valid from: {cert.NotBefore}");
            Console.WriteLine($"Valid until: {cert.NotAfter}");
            Console.WriteLine($"Serial Number: {cert.SerialNumber}");
            (string alg, string pos) = GetTypeAndPosition(cert.Thumbprint);
            Console.WriteLine($"Algorithm: {alg} Position: {pos}");
            Console.WriteLine();
        }
    }

    public static (string, string) GetTypeAndPosition(string thumbprint)
    {
        // Get artiticial values for SID and Client to send the ticket
        X509Certificate2 cert = GetCertificate(thumbprint);
        string alg = cert.PublicKey.Oid.Value switch {
            "1.2.840.10040.4.1" => "DSA",
            "1.2.840.113549.1.1.1" => "RSA",
            "1.2.840.10045.2.1" => "ECDSA",
            _ => throw new Exception("Invalid certificate algorithm")
        };

        // filter the certificates by algorithm and get the index of the thumbprint
         int index = Certificates.Where(x => x.PublicKey.Oid.Value == cert.PublicKey.Oid.Value).
            ToList().FindIndex(x => x.Thumbprint == thumbprint);

        return (alg, $"{index:D3}");
    }

    public static string GetWebsiteLoadCertificates() =>
        "WEBSITE_LOAD_CERTIFICATES=" + String.Join(",", Certificates.Select(cert => cert.Thumbprint));

}
