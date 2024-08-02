# SAP Logon Ticket Generator in C#

This application generates Logon Tickets for SAP NetWeaver Systems. For more information on using Logon Tickets, see [Using Logon Tickets](https://help.sap.com/doc/saphelp_nw75/7.5.5/en-US/4d/a5ddc832211dcde10000000a42189c/content.htm).

## Demo System
Experience the application in action here: [Demo System](https://demo.saptools.mx/).

Analyze the generated MYSAPSSO2 cookie using the [SAP Logon Ticket Decoder](https://saptools.mx/mysapsso2).

## Certificate Algorithms
The application supports three types of certificate algorithms: DSA, RSA, and ECDSA. The key size varies depending on the chosen algorithm. The following certificates are pre-loaded into the application and the SAP Demo System for demonstration purposes:

 | Name                | Algorithm | Key Size |
 |---------------------|-----------|----------|
 | SAP SSO DSA 1024    | DSA       | 1024     |
 | SAP SSO RSA 1024    | RSA       | 1024     |
 | SAP SSO RSA 2048    | RSA       | 2048     |
 | SAP SSO RSA 3072    | RSA       | 3072     |
 | SAP SSO RSA 4096    | RSA       | 4096     |
 | SAP SSO ECDSA P-256 | ECDSA     | P-256    |
 | SAP SSO ECDSA P-384 | ECDSA     | P-384    |
 | SAP SSO ECDSA P-521 | ECDSA     | P-521    |

> **Note:** Please verify that your chosen algorithm is supported by your SAP System: [How can I verify this?](README.md#how-can-i-verify-if-a-specific-certificate-algorithm-is-supported-by-my-sap-netweaver-version)

## Certificate Generation
Certificates can be generated using `OpenSSL` or `sapgenpse`. Below is an example of generating a certificate with `sapgenpse`:


```console
# Choose an algorihtm and a key size:
ALG=RSA
SIZE=4096

# Set the Common Name
FILENAME="${ALG}-${SIZE}"
CN="OU=SAP Tools, CN=SAP SSO ${ALG} ${SIZE}"

# Delete the previous .pse files from the $SECUDIR directory
# Delete the previous .crt and .pfx files
rm ${SECUDIR}/${FILENAME}.pse ${FILENAME}.crt ${FILENAME}.pfx 2>/dev/null
sapgenpse gen_pse -a $ALG -s $SIZE -p ${FILENAME} -x 12345678 "${CN}"
sapgenpse export_own_cert -p $FILENAME -x 12345678 -o ${FILENAME}.crt
sapgenpse export_p12 -p $FILENAME -x 12345678 -z 12345678 -f cn ${FILENAME}.pfx
```
The `OpenSSL` command to generate a certificate is as follows:
```console
# Choose an algorithm and a key size:
ALG=RSA
SIZE=4096
PASSWORD=12345678

# Set the Common Name
FILENAME="${ALG}-${SIZE}"
CN="/OU=SAP Tools/CN=SAP SSO ${ALG} ${SIZE}"

# Generate a private key
openssl genpkey -algorithm $ALG -out ${FILENAME}.key -pkeyopt rsa_keygen_bits:$SIZE

# Generate a certificate signing request (CSR)
openssl req -new -key ${FILENAME}.key -out ${FILENAME}.csr -subj "$CN"

# Generate a self-signed certificate
openssl x509 -req -days 3650 -in ${FILENAME}.csr -signkey ${FILENAME}.key -out ${FILENAME}.crt

# Export the certificate and private key to a PKCS#12 file
openssl pkcs12 -export -out ${FILENAME}.pfx -inkey ${FILENAME}.key -in ${FILENAME}.crt -password pass:${PASSWORD}
```
The `OpenSSL` commands for ECDSA require additional steps:
```console
# Choose an algorithm and a key size:
ALG=ECDSA
CURVE=prime256v1 # or secp384r1, secp521r1
PASSWORD=12345678 

# Set the Common Name
FILENAME="${ALG}-${CURVE}"
CN="/OU=SAP Tools/CN=SAP SSO ${ALG} ${CURVE}"

# Generate a private key
openssl ecparam -name $CURVE -genkey -noout -out ${FILENAME}.key

# Generate a certificate signing request (CSR)
openssl req -new -key ${FILENAME}.key -out ${FILENAME}.csr -subj "$CN"

# Generate a self-signed certificate
openssl x509 -req -days 3650 -in ${FILENAME}.csr -signkey ${FILENAME}.key -out ${FILENAME}.crt

# Export the certificate and private key to a PKCS#12 file
openssl pkcs12 -export -out ${FILENAME}.pfx -inkey ${FILENAME}.key -in ${FILENAME}.crt -password pass:${PASSWORD}
```



## Deployment
### Azure Deployment
For Azure deployment, import the .pfx certificate(s) to **Certificates -> Bring Your Own Certificates**. Then, ensure the certificate thumbprints are added to the Environment Variable `WEBSITE_LOAD_CERTIFICATES` to make the certificate(s) accessible to your application.

Example configuration:

```json
[
  {
    "name": "WEBSITE_LOAD_CERTIFICATES",
    "value": "thumbprint_cert1,thumbprint_cert2,...",
    "slotSetting": false
  }
]
```


### Local Installation on Microsoft Windows
For local installations, import the generated .pfx files into the Personal Certificate Store using `certmgr.msc`.



## How can I verify if a specific certificate algorithm is supported by my SAP NetWeaver version?
To verify if a specific certificate algorithm is supported by your SAP NetWeaver version, you can follow these general steps. However, keep in mind that the exact process might vary slightly depending on the specific version and configuration of your SAP NetWeaver system.
1.	SAP NetWeaver Documentation: Start by consulting the official SAP NetWeaver documentation for your specific version. SAP documentation often includes detailed information about security features, including supported cryptographic algorithms for different components and services.
2.	SAP Notes and SAP Support Portal: SAP frequently publishes SAP Notes that provide detailed information, updates, patches, and guidelines for various components of the SAP ecosystem, including NetWeaver. You can search the SAP Support Portal for notes related to cryptographic support and compatibility. Use keywords like "cryptographic algorithms", "certificate support", and your specific NetWeaver version.
3.	SAP Cryptographic Library: The SAP Cryptographic Library (SAPCRYPTOLIB) is the default security product delivered with SAP systems for encryption and digital signatures. Check the documentation or configuration of the SAP Cryptographic Library associated with your NetWeaver version to see which algorithms it supports.
4.	Check System Configuration: In some cases, you can directly check the system configuration through the SAP NetWeaver Administrator (NWA) or other administrative interfaces. Look for sections related to SSL, TLS, or cryptographic settings, where you might find information about enabled or supported algorithms.
5.	Test Configuration: If you have the capability, you might try configuring a service (e.g., HTTPS for a web service) with a certificate using the algorithm in question and see if the system accepts it and functions correctly. This should be done in a test environment to avoid impacting production systems.
6.	Contact SAP Support: If you're unable to find the information you need or if you have specific questions about your system's capabilities, contacting SAP Support directly can be a helpful way to get authoritative answers.
Remember, the support for specific cryptographic algorithms can depend not only on the SAP NetWeaver version but also on the underlying operating system


## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

## Acknowledgments
- [SAP Logon Ticket Decoder](https://saptools.mx/mysapsso2)
- [SAP NetWeaver Documentation](https://help.sap.com/doc/saphelp_nw75/7.5.5/en-US/4d/a5ddc832211dcde10000000a42189c/content.htm)
- [SAP Cryptographic Library](https://help.sap.com/doc/saphelp_nw75/7.5.5/en-US/4d/a5ddc832211dcde10000000a42189c/content.htm)
- [SAP Support Portal](https://support.sap.com/en/index.html)
- [SAP Notes](https://support.sap.com/en/my-support/notes.html)
- [Microsoft Azure Documentation](https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate)
- [Microsoft Windows Documentation](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/certmgr)
- [OpenSSL Documentation](https://www.openssl.org)
- [SAPGENPSE Documentation](https://help.sap.com/doc/saphelp_nw75/7.5.5/en-US/4d/a5ddc832211dcde10000000a42189c/content.htm)
- [SAP Tools](https://saptools.mx)
