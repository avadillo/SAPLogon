# SAP Logon Ticket Generator in C#

This app creates Logon Tickets to be interpreded by SAP NetWeaver Systems. See [Using Logon Tickets](https://help.sap.com/doc/saphelp_nw75/7.5.5/en-US/4d/a5ddc832211dcde10000000a42189c/content.htm)

This app supports two kinds of certificates:
* DSA: Can have keys up to 1024-bit and it is obsolete (SAP Standard)
* RSA: More secure

### Deployment
For an Azure deployment, make sure to add the certificate(s) to the Application Settings.
e.g.:
```json
[
  {
    "name": "WEBSITE_LOAD_CERTIFICATES",
    "value": "thumbprint_cert1,thumbrint_cert2,...",
    "slotSetting": false
  }
]
```

### Demo System
Test the application here: [Demo System](https://sapnwa.saptools.mx/)

Paste the generated MYSAPSSO2 cookie to [SAP Logon Ticket Decoder](https://saptools.mx/mysapsso2) to analyze it.