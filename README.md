
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
[ID Analyzer Core API](https://www.idanalyzer.com/products/id-analyzer-core-api.html) allows you to perform OCR data extraction, facial biometric verification, identity verification, age verification, document cropping, document authentication (fake ID check) using an ID image (JPG, PNG, PDF accepted) and user selfie photo or video. Core API has great global coverage, supporting over 98% of the passports, driver licenses and identification cards currently being circulated around the world.

![Sample ID](https://www.idanalyzer.com/img/sampleid1.jpg)

The sample code below will extract data from this sample Driver License issued in California, compare it with a [photo of Lena](https://upload.wikimedia.org/wikipedia/en/7/7d/Lenna_%28test_image%29.png), and check whether the ID is real or fake.              

```c#
try {
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
}catch(APIException e)
{
    Console.WriteLine("Error Code: " + e.ErrorCode);
    Console.WriteLine("Error Message: " + e.Message);
}
catch (ArgumentException e)
{
    Console.WriteLine("Input Error: " + e.Message);
}
catch (Exception e)
{
    Console.WriteLine("Unexpected Error: " + e.Message);
}
```
You could also set additional parameters before performing ID scan:  
```c#
coreapi.EnableVault(true, false, false, false);  // enable vault cloud storage to store document information and image
coreapi.SetBiometricThreshold(0.6); // make face verification more strict  
coreapi.EnableAuthentication(true, "quick"); // check if document is real using 'quick' module  
coreapi.EnableBarcodeMode(false); // disable OCR and scan for AAMVA barcodes only  
coreapi.EnableImageOutput(true, true, "url"); // output cropped document and face region in URL format  
coreapi.EnableDualsideCheck(true); // check if data on front and back of ID matches  
coreapi.SetVaultData("user@example.com", "12345" , "AABBCC"); // store custom data into vault  
coreapi.RestrictCountry("US,CA,AU"); // accept documents from United States, Canada and Australia  
coreapi.RestrictState("CA,TX,WA"); // accept documents from california, texas and washington  
coreapi.RestrictType("DI"); // accept only driver license and identification card  
coreapi.SetOCRImageResize(0); // disable OCR resizing  
coreapi.VerifyExpiry(true); // check document expiry  
coreapi.VerifyAge("18-120"); // check if person is above 18  
coreapi.VerifyDOB("1990/01/01"); // check if person's birthday is 1990/01/01  
coreapi.VerifyDocumentNumber("X1234567"); // check if the person's ID number is X1234567  
coreapi.VerifyName("Elon Musk"); // check if the person is named Elon Musk  
coreapi.VerifyAddress("123 Sunny Rd, California"); // Check if address on ID matches with provided address  
coreapi.VerifyPostcode("90001"); // check if postcode on ID matches with provided postcode
```

To **scan both front and back of ID**:

```php
JObject result = await coreapi.Scan("path/to/id_front.jpg", "path/to/id_back.jpg");
```
To perform **biometric photo verification**:

```php
JObject result = await coreapi.Scan("path/to/id.jpg", "", "path/to/face.jpg");
```
To perform **biometric video verification**:

```php
JObject result = await coreapi.Scan("path/to/id.jpg", "", "", "path/to/video.mp4", "1234");
```
Check out sample response array fields visit [Core API reference](https://developer.idanalyzer.com/coreapi.html##readingresponse).

## DocuPass API
[DocuPass](https://www.idanalyzer.com/products/docupass.html) allows you to verify your users without designing your own web page or mobile UI. A unique DocuPass URL can be generated for each of your users and your users can verify their own identity by simply opening the URL in their browser. DocuPass URLs can be directly opened using any browser,  you can also embed the URL inside an iframe on your website, or within a WebView inside your iOS/Android/Cordova mobile app.

![DocuPass Screen](https://www.idanalyzer.com/img/docupassliveflow.jpg)

DocuPass comes with 4 modules and you need to [choose an appropriate DocuPass module](https://www.idanalyzer.com/products/docupass.html) for integration.

To start, we will assume you are trying to **verify one of your user that has an ID of "5678"** in your own database, we need to **generate a DocuPass verification request for this user**. A unique **DocuPass reference code** and **URL** will be generated.

```c#
try { 

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


}
catch (APIException e)
{
    Console.WriteLine("Error Code: " + e.ErrorCode);
    Console.WriteLine("Error Message: " + e.Message);
}
catch (ArgumentException e)
{
	Console.WriteLine("Input Error: " + e.Message);
}
catch (Exception e)
{
	Console.WriteLine("Unexpected Error: " + e.Message);
}
```
If you are looking to embed DocuPass into your mobile application, simply embed `result['url']` inside a WebView. To tell if verification has been completed monitor the WebView URL and check if it matches the URLs set in `setRedirectionURL`. (DocuPass Live Mobile currently cannot be embedded into native iOS App due to OS restrictions, you will need to open it with Safari)

Check out additional DocuPass settings:

```c#
docupass.SetReusable(true); // allow DocuPass URL/QR Code to be used by multiple users  
docupass.SetLanguage("en"); // override auto language detection  
docupass.SetQRCodeFormat("000000", "FFFFFF", 5, 1); // generate a QR code using custom colors and size  
docupass.SetWelcomeMessage("We need to verify your driver license before you make a rental booking with our company."); // Display your own greeting message  
docupass.SetLogo("https://www.your-website.com/logo.png"); // change default logo to your own  
docupass.HideBrandingLogo(true); // hide footer logo  
docupass.RestrictCountry("US,CA,AU"); // accept documents from United States, Canada and Australia  
docupass.RestrictState("CA,TX,WA"); // accept documents from california, texas and washington  
docupass.RestrictType("DI"); // accept only driver license and identification card  
docupass.VerifyExpiry(true); // check document expiry  
docupass.VerifyAge("18-120"); // check if person is above 18  
docupass.VerifyDOB("1990/01/01"); // check if person's birthday is 1990/01/01  
docupass.VerifyDocumentNumber("X1234567"); // check if the person's ID number is X1234567  
docupass.VerifyName("Elon Musk"); // check if the person is named Elon Musk  
docupass.VerifyAddress("123 Sunny Rd, California"); // Check if address on ID matches with provided address  
docupass.VerifyPostcode("90001"); // check if postcode on ID matches with provided postcode
```

Now you should write a **callback script** or a **webhook**, to receive the verification results.  Visit [DocuPass Callback reference](https://developer.idanalyzer.com/docupass_callback.html) to check out the full payload returned by DocuPass. Callback script is generally programmed in a server environment and is beyond the scope of this guide, you can check out our [PHP SDK](https://github.com/idanalyzer/id-analyzer-php-sdk) for creating a callback script in PHP.

For the final step, you could create two web pages (URLS set via `set_redirection_url`) that display the results to your user. DocuPass reference will be passed as a GET parameter when users are redirected, for example: https://www.your-website.com/verification_succeeded.php?reference=XXXXXXXXX, you could use the reference code to fetch the results from your database. P.S. We will always send callbacks to your server before redirecting your user to the set URL.

## Vault API
ID Analyzer provides free cloud database storage (Vault) for you to store data obtained through Core API and DocuPass. You can set whether you want to store your user data into Vault through `enableVault` while making an API request with PHP SDK. Data stored in Vault can be looked up through [Web Portal](https://portal.idanalyzer.com) or via Vault API.

If you have enabled Vault, Core API and DocuPass will both return a vault entry identifier string called `vaultid`,  you can use the identifier to look up your user data:

```c#
try
{
    Vault vault = new Vault(API_KEY, API_REGION);

    // Throw exception if API returns error
    vault.ThrowAPIException(true);

    JObject result = await vault.Get("Vault ID");

    Console.WriteLine(result.ToString());

}
catch (APIException e)
{
    Console.WriteLine("Error Code: " + e.ErrorCode);
    Console.WriteLine("Error Message: " + e.Message);
}
catch (ArgumentException e)
{
    Console.WriteLine("Input Error: " + e.Message);
}
catch (Exception e)
{
    Console.WriteLine("Unexpected Error: " + e.Message);
}
```
You can also list the items in your vault, the following example list 5 items created on or after 2021/02/25, sorted by first name in ascending order, starting from first item:

```c#
string[] filter = { "createtime>=2021/02/25" };
JObject result = await vault.List(filter, "createtime", "ASC", 5, 0);
```

Alternatively, you may have a DocuPass reference code which you want to search through vault to check whether user has completed identity verification:

```c#
string[] filter = { "docupass_reference=XXXXXXXXXXXX" };
JObject result = await vault.List(filter);
```
Learn more about [Vault API](https://developer.idanalyzer.com/vaultapi.html).

## Error Catching

The API server may return error responses such as when document cannot be recognized. You can either manually inspect the response returned by API, or you may set `ThrowAPIException(true);` to raise an `APIException` when API error is encountered. You can then use try/catch block on functions that calls the API such as `Scan`, `CreateMobile`, `CreateLiveMobile`, `CreateIframe`, `CreateRedirection`, and all the functions for Vault to catch the errors. `InvalidArgumentException` is thrown when you pass incorrect arguments to any of the functions.

```c#

try{
	//...    
}catch (APIException e){
    Console.WriteLine("Error Code: " + e.ErrorCode);
    Console.WriteLine("Error Message: " + e.Message);
    // View complete list of error codes under API reference: https://developer.idanalyzer.com/
    switch(e.ErrorCode){
        case 1:
            // Invalid API Key
            break;
        case 8:
            // Out of API quota
            break;
        case 9:
            // Document not recognized
            break;
        default:
            // Other error
    }
}catch(ArgumentException e){
    Console.WriteLine("Argument Error! " + e.Message);
}catch(Exception e){
    Console.WriteLine("Unexpected Error! " + e.Message);
}
```

## Demo
Check out **/demo** folder for more PHP demo codes.

## SDK Reference
Check out [ID Analyzer Dot Net SDK Reference](https://idanalyzer.github.io/id-analyzer-dotnet/)