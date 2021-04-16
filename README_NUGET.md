
# ID Analyzer .NET SDK
This is a .Net SDK for [ID Analyzer Identity Verification APIs](https://www.idanalyzer.com), though all the APIs can be called with without the SDK using simple HTTP requests as outlined in the [documentation](https://developer.idanalyzer.com), you can use this SDK to accelerate server-side development.

We strongly discourage users to connect to ID Analyzer API endpoint directly  from client-side applications that will be distributed to end user, such as mobile app, or in-browser JavaScript. Your API key could be easily compromised, and if you are storing your customer's information inside Vault they could use your API key to fetch all your user details. Therefore, the best practice is always to implement a client side connection to your server, and call our APIs from the server-side.

## Installation
Install through nuget CLI

```shell
nuget install IDAnalyzer
```
Install through Visual Studio Package Manager Console

```shell
Install-Package IDAnalyzer
```

Alternatively, download this package and add the project under`/IDAnalyzer` to your solution

## Core API
[ID Analyzer Core API](https://www.idanalyzer.com/products/id-analyzer-core-api.html) allows you to perform OCR data extraction, facial biometric verification, identity verification, age verification, document cropping, document authentication (fake ID check), paperwork automation using an ID image (JPG, PNG, PDF accepted) and user selfie photo or video. Core API has great global coverage, supporting over 98% of the passports, driver licenses and identification cards currently being circulated around the world.

![Sample ID](https://www.idanalyzer.com/img/sampleid1.jpg)

The sample code below will extract data from this sample Driver License issued in California, compare it with a [photo of Lena](https://upload.wikimedia.org/wikipedia/en/7/7d/Lenna_%28test_image%29.png), and check whether the ID is real or fake.              

```c#
CoreAPI coreapi = new CoreAPI(API_KEY, API_REGION);

// Throw exception if API returns error
coreapi.ThrowAPIException(true);

// Enable authentication and use 'quick' module to check if ID is authentic
coreapi.EnableAuthentication(true, "quick");

// Send document to Core API and get json response
JObject result = await coreapi.Scan("https://www.idanalyzer.com/img/sampleid1.jpg", "", "https://upload.wikimedia.org/wikipedia/en/7/7d/Lenna_%28test_image%29.png");

// Print document holder name
Console.WriteLine(String.Format("Hello your name is {0} {1}", (string)result.SelectToken("result.firstName"), (string)result.SelectToken("result.lastName")));

// Parse document authentication results  
if (result.ContainsKey("authentication")){
    if ((double)result.SelectToken("authentication.score") > 0.5) {
        Console.WriteLine("The document uploaded is authentic");
    }else if ((double)result.SelectToken("authentication.score") > 0.3){
        Console.WriteLine("The document uploaded looks little bit suspicious");
    }else
    {
        Console.WriteLine("The document uploaded is fake");
    }
}
// Parse face verification results  
if (result.ContainsKey("face")){
    if (result.SelectToken("face.error") != null){
        // View complete error codes under API reference: https://developer.idanalyzer.com/coreapi.html
        Console.WriteLine(String.Format("Face verification failed! Code: {0}, Reason: {1}", (string)result.SelectToken("face.error"), (string)result.SelectToken("face.error_message")));
    }else
    {
        if ((bool)result.SelectToken("face.isIdentical") == true){
            Console.WriteLine("Great! Your photo looks identical to the photo on document");
        }else
        {
            Console.WriteLine("Oh no! Your photo looks different to the photo on document");
        }
        Console.WriteLine(String.Format("Similarity score: {0}", (string)result.SelectToken("face.confidence")));
    }
}
// Print result
Console.WriteLine(Environment.NewLine + "Raw JSON Result: ");
Console.WriteLine(result.ToString());

```
## DocuPass API
[DocuPass](https://www.idanalyzer.com/products/docupass.html) allows you to verify user identity and have them sign legal documents without designing your own web page or mobile UI. A unique DocuPass URL can be generated for each of your users and your users can verify their own identity by simply opening the URL in their browser. DocuPass URLs can be directly opened using any browser,  you can also embed the URL inside an iframe on your website, or within a WebView inside your iOS/Android/Cordova mobile app.

![DocuPass Screen](https://www.idanalyzer.com/img/docupassliveflow.jpg)

DocuPass comes with 4 modules and you need to [choose an appropriate DocuPass module](https://www.idanalyzer.com/products/docupass.html) for integration.

To start, we will assume you are trying to **verify one of your user that has an ID of "5678"** in your own database, we need to **generate a DocuPass verification request for this user**. A unique **DocuPass reference code** and **URL** will be generated.

```c#
DocuPass docupass = new DocuPass(API_KEY, "My Company Inc.", API_REGION);

// Throw exception if API returns error
docupass.ThrowAPIException(true);

// We need to set an identifier so that we know internally who we are verifying, this string will be returned in the callback. You can use your own user/customer id.  
docupass.SetCustomID(textBox7.Text);  

// Enable vault cloud storage to store verification results, so we can look up the results  
docupass.EnableVault(true);  

// Set a callback URL where verification results will be sent, you can use docupass_callback.php in demo folder as a template  
docupass.SetCallbackURL("https://www.your-website.com/docupass_callback.php"); 

// We want DocuPass to return document image and user face image in URL format so we can store them on our own server later.  
docupass.SetCallbackImage(true, true, 1);  

// We will do a quick check on whether user have uploaded a fake ID  
docupass.EnableAuthentication(true, "quick", 0.3);  

// Enable photo facial biometric verification with threshold of 0.5  
docupass.EnableFaceVerification(true, 1, 0.5);  

// Users will have only 1 attempt at verification  
docupass.SetMaxAttempt(1);  

// We want to redirect user back to your website when they are done with verification  
docupass.SetRedirectionURL("https://www.your-website.com/verification_succeeded.php", "https://www.your-website.com/verification_failed.php");

// Create a DocuPass Mobile Session
JObject result = await docupass.CreateMobile();
// JObjectresult = await docupass.CreateLiveMobile();
Console.WriteLine("Scan the QR Code below to verify your identity: ");
Console.WriteLine((string)result["qrcode"]);
Console.WriteLine("Or open your mobile browser and type in: ");
Console.WriteLine((string)result["url"]);

//JObject result = await docupass.CreateIframe();
//Console.WriteLine("Embed following URL on your website: ");
//Console.WriteLine((string)result["url"]);

//JObject result = await docupass.CreateRedirection();
//Console.WriteLine("Redirect your user to the following URL: ");
//Console.WriteLine((string)result["url"]);

Console.WriteLine(Environment.NewLine+"Raw JSON Result: ");
Console.WriteLine(result.ToString());
```
## Demo
Check out **demo project** for more demo code.

## SDK Reference
Check out [ID Analyzer Dot Net SDK Reference](https://idanalyzer.github.io/id-analyzer-dotnet/)