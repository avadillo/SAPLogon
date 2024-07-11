namespace SAPLogon.Web.Common;

public class AlgComparer : IComparer<object> {
    public int Compare(object stringA, object stringB) {
        string string1 = stringA.ToString()!.Replace("DSA", "1").Replace("RSA", "2").Replace("ECDSA", "3");
        string string2 = stringB.ToString()!.Replace("DSA", "1").Replace("RSA", "2").Replace("ECDSA", "3");
        return string1.CompareTo(string1);
    }
}