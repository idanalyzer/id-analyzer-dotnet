using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IDAnalyzer
{
    /// <summary>
    /// APIException Class will be raised upon API related error if ThrowAPIException is enabled.
    /// </summary>
    public class APIException : Exception
    {
        public int ErrorCode;

        public APIException() : base()
        {
        }

        public APIException(string message, int code) : base(message)
        {
            this.ErrorCode = code;
        }
        public APIException(string message, Exception inner, int code) : base(message, inner)
        {
            this.ErrorCode = code;
        }
    }

    /// <summary>
    /// Multi-function ID verification API to verify document, its authenticity and face match the document with your user photo. 
    /// </summary>
    public class CoreAPI
    {

        private Hashtable config = new Hashtable();
        private string ApiEndpoint;
        private string Apikey;
        private bool throwError = false;
        private HttpClient APIclient = new HttpClient();

        private bool IsValidURL(string url)
        {
            return Regex.Match(url, @"(http(s)?:\/\/.)(www\.)?[-a-zA-Z0-9@:%._+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_+.~#?&/=]*)").Success;
        }
        private static bool IsHexColor(string inputColor)
        {
            return Regex.Match(inputColor, "^(?:[0-9a-fA-F]{6})$").Success;
        }
        private static bool IsAgeRange(string ageRange)
        {
            return Regex.Match(ageRange, @"^(\d+-\d+)$").Success;
        }




        /// <summary>
        /// Initialize Core API with an API key and optional region (US, EU)
        /// </summary>
        /// <param name="apikey">You API key</param>
        /// <param name="region">US/EU</param>
        public CoreAPI(string apikey, string region = "US")
        {
            if (apikey == "") throw new ArgumentException("Please provide an API key");
            this.Apikey = apikey;
            this.ResetConfig();
            if (region.ToLower() == "eu"){
                this.ApiEndpoint = "https://api-eu.idanalyzer.com/";
            }else if (region.ToLower() == "us"){
                this.ApiEndpoint = "https://api.idanalyzer.com/";
            }else{
                this.ApiEndpoint = region;
            }

        }


        /// <summary>
        /// Raise APIException when API returns error
        /// </summary>
        /// <param name="throwException">Enable or Disable APIException being thrown</param>
        public void ThrowAPIException(bool throwException = false)
        {
            this.throwError = throwException;
        }

        /// <summary>
        /// Reset all API configurations except API key and region.
        /// </summary>
        public void ResetConfig()
        {

            this.config["accuracy"] = 2;
            this.config["authenticate"] = false;
            this.config["authenticate_module"] = "1";
            this.config["ocr_scaledown"] = 2000;
            this.config["outputimage"] = false;
            this.config["outputface"] = false;
            this.config["outputmode"] = "url";
            this.config["dualsidecheck"] = false;
            this.config["verify_expiry"] = true;
            this.config["verify_documentno"] = "";
            this.config["verify_name"] = "";
            this.config["verify_dob"] = "";
            this.config["verify_age"] = "";
            this.config["verify_address"] = "";
            this.config["verify_postcode"] = "";
            this.config["country"] = "";
            this.config["region"] = "";
            this.config["type"] = "";
            this.config["checkblocklist"] = false;
            this.config["vault_save"] = true;
            this.config["vault_saveunrecognized"] = false;
            this.config["vault_noduplicate"] = false;
            this.config["vault_automerge"] = false;
            this.config["vault_customdata1"] = "";
            this.config["vault_customdata2"] = "";
            this.config["vault_customdata3"] = "";
            this.config["vault_customdata4"] = "";
            this.config["vault_customdata5"] = "";
            this.config["barcodemode"] = false;
            this.config["biometric_threshold"] = 0.4;
            this.config["client"] = "dotnet-sdk";

        }

        /// <summary>
        /// Set OCR Accuracy
        /// </summary>
        /// <param name="accuracy">0 = Fast, 1 = Balanced, 2 = Accurate, defaults to 2</param>
        public void SetAccuracy(int accuracy = 2)
        {
            this.config["accuracy"] = accuracy;
        }



        /// <summary>
        /// Validate the document to check whether the document is authentic and has not been tampered, and set authentication module
        /// </summary>
        /// <param name="enabled">Enable or disable document authentication</param>
        /// <param name="module">Authentication module version: "1", "2" or "quick"</param>
        public void EnableAuthentication(bool enabled = false, string module = "2")
        {
            this.config["authenticate"] = enabled;

            if (enabled && module != "1" && module != "2" && module != "quick"){
                throw new ArgumentException("Invalid authentication module, 1, 2 or 'quick' accepted.");
            }

            this.config["authenticate_module"] = module;
        }

        /// <summary>
        /// Scale down the uploaded image before sending to OCR engine. Adjust this value to fine tune recognition accuracy on large full-resolution images. Set 0 to disable image resizing.
        /// </summary>
        /// <param name="maxScale">Number of 0 or between 500~4000</param>
        public void SetOCRImageResize(int maxScale = 2000)
        {
            if (maxScale != 0 && (maxScale < 500 || maxScale > 4000)){
                throw new ArgumentException("Invalid scale value, 0, or 500 to 4000 accepted.");
            }
            this.config["ocr_scaledown"] = maxScale;

        }

        /// <summary>
        /// Set the minimum confidence score to consider faces being identical
        /// </summary>
        /// <param name="threshold">Number between 0 to 1, higher value yields more strict verification</param>
        public void SetBiometricThreshold(double threshold = 0.4)
        {
            if (threshold <= 0 || threshold > 1){
                throw new ArgumentException("Invalid threshold value, float between 0 to 1 accepted.");
            }

            this.config["biometric_threshold"] = threshold;

        }

        /// <summary>
        /// Generate cropped image of document and/or face, and set output format [url, base64]
        /// </summary>
        /// <param name="cropDocument">Enable or disable document cropping</param>
        /// <param name="cropFace">Enable or disable face cropping</param>
        /// <param name="outputFormat">"url" or "base64"</param>
        public void EnableImageOutput(bool cropDocument = false, bool cropFace = false, string outputFormat = "url")
        {
            if (outputFormat != "url" && outputFormat != "base64"){
                throw new ArgumentException("Invalid output format, 'url' or 'base64' accepted.");
            }
            this.config["outputimage"] = cropDocument;
            this.config["outputface"] = cropFace;
            this.config["outputmode"] = outputFormat;

        }

        /// <summary>
        /// Check if the names, document number and document type matches between the front and the back of the document when performing dual-side scan. If any information mismatches error 14 will be thrown.
        /// </summary>
        /// <param name="enabled">Enable or disable dual-side information check</param>
        public void EnableDualsideCheck(bool enabled = false)
        {
            this.config["dualsidecheck"] = enabled;

        }

        /// <summary>
        /// Check if the document is still valid based on its expiry date.
        /// </summary>
        /// <param name="enabled">Enable or disable  expiry check</param>
        public void VerifyExpiry(bool enabled = false)
        {
            this.config["verify_expiry"] = enabled ;
        }

        /// <summary>
        /// Check if supplied document or personal number matches with document.
        /// </summary>
        /// <param name="documentNumber">Document or personal number requiring validation</param>
        public void VerifyDocumentNumber(string documentNumber = "X1234567")
        {
            this.config["verify_documentno"] =  documentNumber;
        }

        /// <summary>
        /// Check if supplied name matches with document.
        /// </summary>
        /// <param name="fullName">Full name requiring validation</param>
        public void VerifyName(string fullName = "ELON MUSK")
        {

            this.config["verify_name"] =  fullName;
            

        }


        /// <summary>
        /// Check if supplied date of birth matches with document.
        /// </summary>
        /// <param name="dob">Date of birth in YYYY/MM/DD</param>
        public void VerifyDOB(string dob = "1990/01/01")
        {
            if (dob == ""){
                this.config["verify_dob"] =  "";
            }else{
                if (!DateTime.TryParseExact(
                     dob,
                     "yyyy/MM/dd",
                     CultureInfo.InvariantCulture,
                     DateTimeStyles.AssumeUniversal,
                     out DateTime result))
                {
                    throw new ArgumentException("Invalid birthday format (YYYY/MM/DD)");
                };
              
                this.config["verify_dob"] = dob;
            }
        }

        /// <summary>
        /// Check if the document holder is aged between the given range.
        /// </summary>
        /// <param name="ageRange">Age range, example: 18-40</param>
        public void VerifyAge(string ageRange = "18-99")
        {
            if (ageRange == ""){
                this.config["verify_age"] =  "";
            }else{
                if (!IsAgeRange(ageRange))
                {
                    throw new ArgumentException("Invalid age range format (minAge-maxAge)");
                }

                this.config["verify_age"] =  ageRange;
            }

        }

        /// <summary>
        /// Check if supplied address matches with document.
        /// </summary>
        /// <param name="address">Address requiring validation</param>
        public void VerifyAddress(string address = "123 Sample St, California, US")
        {
        
            this.config["verify_address"] =  address;
            

        }

        /// <summary>
        /// Check if supplied postcode matches with document.
        /// </summary>
        /// <param name="postcode">Postcode requiring validation</param>
        public void VerifyPostcode(string postcode = "90001")
        {
    
            this.config["verify_postcode"] =  postcode;
            

        }

        /// <summary>
        /// Check if the document was issued by specified countries, if not error code 10 will be thrown. Separate multiple values with comma. For example "US,CA" would accept documents from United States and Canada.
        /// </summary>
        /// <param name="countryCodes">ISO ALPHA-2 Country Code separated by comma</param>
        public void RestrictCountry(string countryCodes = "US,CA,UK")
        {

            this.config["country"] = countryCodes;
            
        }

        /// <summary>
        /// Check if the document was issued by specified state, if not error code 11 will be thrown. Separate multiple values with comma. For example "CA,TX" would accept documents from California and Texas.
        /// </summary>
        /// <param name="states">State full name or abbreviation separated by comma</param>
        public void RestrictState(string states = "CA,TX")
        {

            
            this.config["region"] = states;
            
        }

        /// <summary>
        /// Check if the document was one of the specified types, if not error code 12 will be thrown. For example, "PD" would accept both passport and drivers license.
        /// </summary>
        /// <param name="documentType">P: Passport, D: Driver's License, I: Identity Card</param>
        public void RestrictType(string documentType = "DIP")
        {

            this.config["type"] =  documentType;
            

        }


        /// <summary>
        /// Disable Visual OCR and read data from AAMVA Barcodes only
        /// </summary>
        /// <param name="enabled">Enable or disable Barcode Mode</param>
        public void EnableBarcodeMode(bool enabled = false)
        {
            this.config["barcodemode"] =  enabled;

        }


        /// <summary>
        /// Save document image and parsed information in your secured vault. You can list, search and update document entries in your vault through Vault API or web portal.
        /// </summary>
        /// <param name="enabled">Enable or disable Vault</param>
        /// <param name="saveUnrecognized">Save document image in your vault even if the document cannot be recognized</param>
        /// <param name="autoMergeDocument">Prevent duplicated images from being saved</param>
        /// <param name="noDuplicateImage">Automatically merge images with same document number into a single entry inside vault</param>
        public void EnableVault(bool enabled = true, bool saveUnrecognized = false, bool noDuplicateImage = false, bool autoMergeDocument = false)
        {
            this.config["vault_save"] =  enabled;
            this.config["vault_saveunrecognized"] =  saveUnrecognized;
            this.config["vault_noduplicate"] =  noDuplicateImage;
            this.config["vault_automerge"] =  autoMergeDocument;
        }


        /// <summary>
        /// Add up to 5 custom strings that will be associated with the vault entry, this can be useful for filtering and searching entries.
        /// </summary>
        /// <param name="data1">Custom data field 1</param>
        /// <param name="data2">Custom data field 2</param>
        /// <param name="data3">Custom data field 3</param>
        /// <param name="data4">Custom data field 4</param>
        /// <param name="data5">Custom data field 5</param>
        public void SetVaultData(string data1 = "", string data2 = "", string data3 = "", string data4 = "", string data5 = "")
        {
            this.config["vault_customdata1"] = data1;
            this.config["vault_customdata2"] = data2;
            this.config["vault_customdata3"] = data3;
            this.config["vault_customdata4"] = data4;
            this.config["vault_customdata5"] = data5;

        }

        /// <summary>
        /// Set an API parameter and its value, this function allows you to set any API parameter without using the built-in functions
        /// </summary>
        /// <param name="parameterKey">Parameter key</param>
        /// <param name="parameterValue">Parameter value</param>
        public void SetParameter(string parameterKey, dynamic parameterValue)
        {
            this.config[parameterKey] = parameterValue;
        }


        /// <summary>
        /// Scan an ID document with Core API, optionally specify document back image, face verification image, face verification video and video passcode
        /// </summary>
        /// <param name="document_primary">Front of Document (File path or URL)</param>
        /// <param name="document_secondary">Back of Document (File path or URL)</param>
        /// <param name="biometric_photo">Face Photo (File path or URL)</param>
        /// <param name="biometric_video">Face Video (File path or URL)</param>
        /// <param name="biometric_video_passcode">Face Video Passcode (4 Digit Number)</param>
        /// <returns>Core API response data</returns>
        public async Task<JObject> Scan(string document_primary, string document_secondary = "", string biometric_photo = "", string biometric_video = "", string biometric_video_passcode = "")
        {

            var payload = this.config;
            payload["apikey"] = this.Apikey;


            if (document_primary == ""){
                throw new ArgumentException("Primary document image required.");
            }
            if (IsValidURL(document_primary))
            {
                payload["url"] = document_primary;
            }
            else if (File.Exists(document_primary))
            {
                Byte[] bytes = File.ReadAllBytes(document_primary);
                payload["file_base64"] = Convert.ToBase64String(bytes);
            }
            else
            {
                throw new ArgumentException("Invalid primary document image, file not found or malformed URL.");
            }
            if (document_secondary != ""){
                if (IsValidURL(document_secondary))
                {
                    payload["url_back"] = document_secondary;
                }
                else if (File.Exists(document_secondary))
                {
                    Byte[] bytes = File.ReadAllBytes(document_secondary);
                    payload["file_back_base64"] = Convert.ToBase64String(bytes);
                }
                else {
                    throw new ArgumentException("Invalid secondary document image, file not found or malformed URL.");
                }
            }
            if(biometric_photo != ""){
                if(IsValidURL(biometric_photo)){
                    payload["faceurl"] =  biometric_photo;
                }
                    else if(File.Exists(biometric_photo)){
                        Byte[] bytes = File.ReadAllBytes(biometric_photo);
                        payload["face_base64"] = Convert.ToBase64String(bytes);
                }
                    else {
                    throw new ArgumentException("Invalid face image, file not found or malformed URL.");
                }
            }
            if(biometric_video != ""){
                if(IsValidURL(biometric_video)){
                    payload["videourl"] =  biometric_video;
                }
                    else if(File.Exists(biometric_video)){
                        Byte[] bytes = File.ReadAllBytes(biometric_video);
                        payload["video_base64"] = Convert.ToBase64String(bytes);
                }
                    else {
                    throw new ArgumentException("Invalid face video, file not found or malformed URL.");
                }
                if (!Regex.Match(biometric_video_passcode, @"^([0-9]{4})$").Success) {
                    throw new ArgumentException("Please provide a 4 digit passcode for video biometric verification.");
                }else{
                    payload["passcode"] = biometric_video_passcode;
                }
            }
         
            string json = JsonConvert.SerializeObject(payload);
            Console.WriteLine(json);
            var response = await APIclient.PostAsync(this.ApiEndpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            string jsonresponse = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(jsonresponse);

            if (this.throwError)
            {
            
                if (result.ContainsKey("error"))
                {
                    throw new APIException((string)result["error"]["message"], (int)result["error"]["code"]);
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return result;
            }


        }

    }




    /// <summary>
    /// DocuPass identity verification solution, simply send your user a verification link to start DIY identity verification, or embed DocuPass inside your website and mobile app.
    /// </summary>
    public class DocuPass
    {

        private Hashtable config = new Hashtable();
        private string ApiEndpoint;
        private string Apikey;
        private string CompanyName = "Your Company Name";
        private bool throwError = false;
        private HttpClient APIclient = new HttpClient();

        private bool IsValidURL(string url)
        {
            return Regex.Match(url, @"(http(s)?:\/\/.)(www\.)?[-a-zA-Z0-9@:%._+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_+.~#?&/=]*)").Success;
        }
        private static bool IsHexColor(string inputColor)
        {
            return Regex.Match(inputColor, "^(?:[0-9a-fA-F]{6})$").Success;
        }
        private static bool IsAgeRange(string ageRange)
        {
            return Regex.Match(ageRange, @"^(\d+-\d+)$").Success;
        }



        /// <summary>
        /// Initialize DocuPass API with an API key, company name and optional region (US, EU)
        /// </summary>
        /// <param name="apikey">You API key</param>
        /// <param name="companyName">Your company name</param>
        /// <param name="region">US/EU</param>
        public DocuPass(string apikey, string companyName ="Your Company Name", string region = "US")
        {
            if (apikey == "") throw new ArgumentException("Please provide an API key");
            if (companyName == "") throw new ArgumentException("Please provide your company name");
            this.Apikey = apikey;
            this.CompanyName = companyName;
            this.ResetConfig();
            if (region.ToLower() == "eu")
            {
                this.ApiEndpoint = "https://api-eu.idanalyzer.com/";
            }
            else if (region.ToLower() == "us")
            {
                this.ApiEndpoint = "https://api.idanalyzer.com/";
            }
            else
            {
                this.ApiEndpoint = region;
            }

        }

        /// <summary>
        /// Raise APIException when API returns an error
        /// </summary>
        /// <param name="throwException">Enable or Disable APIException being thrown</param>
        public void ThrowAPIException(bool throwException = false)
        {
            this.throwError = throwException;
        }


        /// <summary>
        /// Reset all API configurations except API key and region.
        /// </summary>
        public void ResetConfig()
        {
            this.config["companyname"] =  this.CompanyName;
            this.config["callbackurl"] =  "";
            this.config["biometric"] =  0;
            this.config["authenticate_minscore"] =  0;
            this.config["authenticate_module"] =  2;
            this.config["maxattempt"] =  1;
            this.config["documenttype"] =  "";
            this.config["documentcountry"] =  "";
            this.config["documentregion"] =  "";
            this.config["dualsidecheck"] =  false;
            this.config["verify_expiry"] =  false;
            this.config["verify_documentno"] =  "";
            this.config["verify_name"] =  "";
            this.config["verify_dob"] =  "";
            this.config["verify_age"] =  "";
            this.config["verify_address"] =  "";
            this.config["verify_postcode"] =  "";
            this.config["successredir"] =  "";
            this.config["failredir"] =  "";
            this.config["customid"] =  "";
            this.config["vault_save"] =  true;
            this.config["return_documentimage"] =  true;
            this.config["return_faceimage"] =  true;
            this.config["return_type"] =  1;
            this.config["return_type"] =  1;
            this.config["qr_color"] =  "";
            this.config["qr_bgcolor"] =  "";
            this.config["qr_size"] =  "";
            this.config["qr_margin"] =  "";
            this.config["welcomemessage"] =  "";
            this.config["nobranding"] =  "";
            this.config["logo"] =  "";
            this.config["language"] =  "";
            this.config["biometric_threshold"] =  0.4;
            this.config["reusable"] =  false;
            this.config["client"] = "dotnet-sdk";
        }

        /// <summary>
        /// Set max verification attempt per user
        /// </summary>
        /// <param name="max_attempt">1 to 10</param>
        public void SetMaxAttempt(int max_attempt = 1)
        {
            if (max_attempt < 1 || max_attempt > 10){
                throw new ArgumentException("Invalid max attempt, please specify integer between 1 to 10.");

            }
            this.config["maxattempt"] = max_attempt;
        }

        /// <summary>
        /// Set a custom string that will be sent back to your server's callback URL, and appended to redirection URLs as a query string. It is useful for identifying your user within your database. This value will be stored under docupass_customid under Vault.
        /// </summary>
        /// <param name="customID">A string used to identify your customer internally</param>
        public void SetCustomID(string customID = "")
        {
            this.config["customid"] = customID;
        }

        /// <summary>
        /// Display a custom message to the user in the beginning of verification
        /// </summary>
        /// <param name="message">Plain text string</param>
        public void SetWelcomeMessage(string message = "")
        {
            this.config["welcomemessage"] = message;
        }


        /// <summary>
        /// Replace footer logo with your own logo
        /// </summary>
        /// <param name="url">Logo URL</param>
        public void SetLogo(string url = "https://docupass.app/asset/logo1.png")
        {
            this.config["logo"] = url;
        }


        /// <summary>
        /// Hide all branding logo
        /// </summary>
        /// <param name="hide">Hide logo</param>
        public void HideBrandingLogo(bool hide = false)
        {
            this.config["nobranding"] = hide;
        }

        /// <summary>
        /// DocuPass automatically detects user device language and display corresponding language. Set this parameter to override automatic language detection.
        /// </summary>
        /// <param name="language">Check DocuPass API reference for language code</param>
        public void SetLanguage(string language = "")
        {
            this.config["language"] = language;
        }


        /// <summary>
        /// Replace DocuPass page content with your own HTML and CSS, you can download the HTML/CSS template from DocuPass API Reference page
        /// </summary>
        /// <param name="url">URL pointing to your own HTML page</param>
        public void SetCustomHTML(string url = "")
        {
            this.config["customhtmlurl"] = url;
        }

        /// <summary>
        /// Set an API parameter and its value, this function allows you to set any API parameter without using the built-in functions
        /// </summary>
        /// <param name="parameterKey">Parameter key</param>
        /// <param name="parameterValue">Parameter value</param>
        public void SetParameter(string parameterKey, dynamic parameterValue)
        {
            this.config[parameterKey] = parameterValue;
        }




        /// <summary>
        /// Set server-side callback/webhook URL to receive verification results
        /// </summary>
        /// <param name="url">URL</param>
        public void SetCallbackURL(string url = "https://www.example.com/docupass_callback.php")
        {
            if (!IsValidURL(url))
            {
                throw new ArgumentException("Invalid URL format");
            }

     
            this.config["callbackurl"] = url;
        }


        /// <summary>
        /// Redirect client browser to set URLs after verification. DocuPass reference code and customid will be appended to the end of URL, e.g. https://www.example.com/success.php?reference=XXXXXXXX&customid=XXXXXXXX
        /// </summary>
        /// <param name="successURL">Redirection URL after verification succeeded</param>
        /// <param name="failURL">Redirection URL after verification failed</param>
        public void SetRedirectionURL(string successURL = "https://www.example.com/success.php", string failURL = "https://www.example.com/failed.php")
        {
            if (!IsValidURL(successURL))
            {
                throw new ArgumentException("Invalid URL format for success URL");
            }
            if (!IsValidURL(failURL))
            {
                throw new ArgumentException("Invalid URL format for fail URL");
            }

            this.config["successredir"] = successURL;
            this.config["failredir"] = failURL;
        }


        /// <summary>
        /// Validate the document to check whether the document is authentic and has not been tampered
        /// </summary>
        /// <param name="enabled">Enable or disable Document Authentication</param>
        /// <param name="module">Authentication Module: "1", "2" or "quick"</param>
        /// <param name="minimum_score">Minimum score to pass verification</param>
        public void EnableAuthentication(bool enabled = false, string module = "2", double minimum_score = 0.3)
        {
            if (enabled == false){
                this.config["authenticate_minscore"] = 0;
            }else{
            
                if(minimum_score<0 || minimum_score>1){
                    throw new ArgumentException("Invalid minimum score, please specify float between 0 to 1.");
                }
                
                if(enabled && module != "1" && module != "2" && module != "quick"){
                    throw new ArgumentException("Invalid authentication module, 1, 2 or 'quick' accepted.");
                }
                this.config["authenticate_module"] = module;
                this.config["authenticate_minscore"] = minimum_score;
            }
        }



        /// <summary>
        /// Whether users will be required to submit a selfie photo or record selfie video for facial verification.
        /// </summary>
        /// <param name="enabled">Enable or disable Facial Biometric Verification</param>
        /// <param name="verification_type">1 for photo verification, 2 for video verification</param>
        /// <param name="threshold">Minimum confidence score required to pass verification, value between 0 to 1</param>
        public void EnableFaceVerification(bool enabled = false, int verification_type = 1, double threshold = 0.4)
        {
            if(enabled == false){
                this.config["biometric"] = 0;
            }else{
                if(verification_type == 1 || verification_type == 2 ){
                    this.config["biometric"] =  verification_type;
                    this.config["biometric_threshold"] =  threshold;
                }else{
                    throw new ArgumentException("Invalid verification type, 1 for photo verification, 2 for video verification.");
                }
            }
        }


        /// <summary>
        /// Enabling this parameter will allow multiple users to verify their identity through the same URL, a new DocuPass reference code will be generated for each user automatically.
        /// </summary>
        /// <param name="reusable">Set true to allow unlimited verification for a single DocuPass session</param>
        public void SetReusable(bool reusable = false)
        {
            this.config["reusable"] = reusable;
        }


        /// <summary>
        /// Enable/Disable returning user uploaded document and face image in callback, and image data format.
        /// </summary>
        /// <param name="return_documentimage">Return document image in callback data</param>
        /// <param name="return_faceimage">Return face image in callback data</param>
        /// <param name="return_type">0 for base64, 1 for url</param>
        public void SetCallbackImage(bool return_documentimage = true, bool return_faceimage = true, int return_type = 1)
        {
            this.config["return_documentimage"] =  return_documentimage;
            this.config["return_faceimage"] =  return_faceimage;
            this.config["return_type"] =  return_type == 0? 0:1;
        }


        /// <summary>
        /// Configure QR code generated for DocuPass Mobile and Live Mobile
        /// </summary>
        /// <param name="foregroundColor">Image foreground color HEX code</param>
        /// <param name="backgroundColor">Image background color HEX code</param>
        /// <param name="size">1 to 50</param>
        /// <param name="margin">1 to 50</param>
        public void SetQRCodeFormat(string foregroundColor = "000000", string backgroundColor = "FFFFFF", int size = 5, int margin = 1)
        {
            if(!IsHexColor(foregroundColor)){
                throw new ArgumentException("Invalid foreground color HEX code");
            }
            if(!IsHexColor(foregroundColor)){
                throw new ArgumentException("Invalid background color HEX code");
            }

            this.config["qr_color"] =  foregroundColor;
            this.config["qr_bgcolor"] =  backgroundColor;
            this.config["qr_size"] =  size;
            this.config["qr_margin"] =  margin;
        }

        /// <summary>
        /// Check if the names, document number and document type matches between the front and the back of the document when performing dual-side scan. If any information mismatches error 14 will be thrown.
        /// </summary>
        /// <param name="enabled">Enable or disable dual-side information check</param>
        public void EnableDualsideCheck(bool enabled = false)
        {
            this.config["dualsidecheck"] =  enabled;

        }

        /// <summary>
        /// Check if the document is still valid based on its expiry date.
        /// </summary>
        /// <param name="enabled">Enable or disable expiry check</param>
        public void VerifyExpiry(bool enabled = false)
        {
            this.config["verify_expiry"] =  enabled;
        }

        /// <summary>
        /// Check if supplied document or personal number matches with document.
        /// </summary>
        /// <param name="documentNumber">Document or personal number requiring validation</param>
        public void VerifyDocumentNumber(string documentNumber = "X1234567")
        {
    
            this.config["verify_documentno"] = documentNumber;


        }

        /// <summary>
        /// Check if supplied name matches with document.
        /// </summary>
        /// <param name="fullName">Full name requiring validation</param>
        public void VerifyName(string fullName = "ELON MUSK")
        {
            this.config["verify_name"] = fullName;

        }



        /// <summary>
        /// Check if supplied date of birth matches with document.
        /// </summary>
        /// <param name="dob">Date of birth in YYYY/MM/DD</param>
        public void VerifyDOB(string dob = "1990/01/01")
        {
            if (dob == "")
            {
                this.config["verify_dob"] = "";
            }
            else
            {
                if (!DateTime.TryParseExact(
                     dob,
                     "yyyy/MM/dd",
                     CultureInfo.InvariantCulture,
                     DateTimeStyles.AssumeUniversal,
                     out DateTime result))
                {
                    throw new ArgumentException("Invalid birthday format (YYYY/MM/DD)");
                };

                this.config["verify_dob"] = dob;
            }
        }

        /// <summary>
        /// Check if the document holder is aged between the given range.
        /// </summary>
        /// <param name="ageRange">Age range, example: 18-40</param>
        public void VerifyAge(string ageRange = "18-99")
        {
            if (ageRange == "")
            {
                this.config["verify_age"] = "";
            }
            else
            {
                if (!IsAgeRange(ageRange))
                {
                    throw new ArgumentException("Invalid age range format (minAge-maxAge)");
                }

                this.config["verify_age"] = ageRange;
            }

        }

        /// <summary>
        /// Check if supplied address matches with document.
        /// </summary>
        /// <param name="address">Address requiring validation</param>
        public void VerifyAddress(string address = "123 Sample St, California, US")
        {

            this.config["verify_address"] = address;


        }

        /// <summary>
        /// Check if supplied postcode matches with document.
        /// </summary>
        /// <param name="postcode">Postcode requiring validation</param>
        public void VerifyPostcode(string postcode = "90001")
        {

            this.config["verify_postcode"] = postcode;


        }

        /// <summary>
        /// Only accept document issued by specified countries. Separate multiple values with comma. For example "US,CA" would accept documents from United States and Canada.
        /// </summary>
        /// <param name="countryCodes">ISO ALPHA-2 Country Code separated by comma</param>
        public void RestrictCountry(string countryCodes = "US,CA,UK")
        {

            this.config["documentcountry"] = countryCodes;

        }

        /// <summary>
        /// Only accept document issued by specified state. Separate multiple values with comma. For example "CA,TX" would accept documents from California and Texas.
        /// </summary>
        /// <param name="states">State full name or abbreviation separated by comma</param>
        public void RestrictState(string states = "CA,TX")
        {
            this.config["documentregion"] = states;

        }

        /// <summary>
        ///  Only accept document of specified types. For example, "PD" would accept both passport and drivers license.
        /// </summary>
        /// <param name="documentType">P: Passport, D: Driver's License, I: Identity Card.  </param>
        public void RestrictType(string documentType = "DIP")
        {

            this.config["documenttype"] = documentType;


        }

        /// <summary>
        /// Save document image and parsed information in your secured vault. You can list, search and update document entries in your vault through Vault API or web portal.
        /// </summary>
        /// <param name="enabled">Enable or disable Vault</param>
        public void EnableVault(bool enabled = true)
        {
            this.config["vault_save"] =  enabled ;
        }



        /// <summary>
        /// Create a DocuPass session for embedding in web page as iframe
        /// </summary>
        /// <returns>DocuPass API response</returns>
        public async Task<JObject> CreateIframe(){
            return await this.Create(0);
        }

        /// <summary>
        /// Create a DocuPass session for users to open on mobile phone, or embedding in mobile app
        /// </summary>
        /// <returns>DocuPass API response</returns>
        public async Task<JObject> CreateMobile(){
            return await this.Create(1);
        }

        /// <summary>
        /// Create a DocuPass session for users to open in any browser
        /// </summary>
        /// <returns>DocuPass API response</returns>
        public async Task<JObject> CreateRedirection(){
            return await this.Create(2);
        }

        /// <summary>
        /// Create a DocuPass Live Mobile verification session for users to open on mobile phone
        /// </summary>
        /// <returns>DocuPass API response</returns>
        public async Task<JObject> CreateLiveMobile(){
            return await this.Create(3);
        }


        private async Task<JObject> Create(int docupass_module){

            var payload = this.config;
            payload["apikey"] = this.Apikey;
            payload["type"] = docupass_module;



            string json = JsonConvert.SerializeObject(payload);
            var response = await APIclient.PostAsync(this.ApiEndpoint + "docupass/create", new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            string jsonresponse = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(jsonresponse);

            if (this.throwError)
            {

                if (result.ContainsKey("error"))
                {
                    throw new APIException((string)result["error"]["message"], (int)result["error"]["code"]);
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return result;
            }


        }


        /// <summary>
        /// Validate a data received through DocuPass Callback against DocuPass Server to prevent request spoofing
        /// </summary>
        /// <param name="reference">DocuPass reference</param>
        /// <param name="hash">DocuPass callback hash</param>
        /// <returns>Validation result</returns>
        public async Task<bool> Validate(string reference, string hash){

       

            Hashtable payload = new Hashtable
            {
                ["apikey"] = this.Apikey,
                ["reference"] = reference,
                ["hash"] = hash
            };
            string json = JsonConvert.SerializeObject(payload);
            var response = await APIclient.PostAsync(this.ApiEndpoint + "docupass/validate", new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            string jsonresponse = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(jsonresponse);

            return result.ContainsKey("success");

        }


    }

    /// <summary>
    /// Biometric-search enabled database to securely store and query your user's identity information and documents
    /// </summary>
    public class Vault
    {
        private string Apikey;
        private string ApiEndpoint = "";
        private bool throwError = false;
        private HttpClient APIclient = new HttpClient();

        private bool IsValidURL(string url)
        {
            return Regex.Match(url, @"(http(s)?:\/\/.)(www\.)?[-a-zA-Z0-9@:%._+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_+.~#?&/=]*)").Success;
        }


        /// <summary>
        /// Initialize Vault API with an API key, and optional region (US, EU)
        /// </summary>
        /// <param name="apikey">You API key</param>
        /// <param name="region">US/EU</param>
        public Vault(string apikey,string region = "US")
        {
            if (apikey == "") throw new ArgumentException("Please provide an API key");
       
            this.Apikey = apikey;

            if (region.ToLower() == "eu")
            {
                this.ApiEndpoint = "https://api-eu.idanalyzer.com/";
            }
            else if (region.ToLower() == "us")
            {
                this.ApiEndpoint = "https://api.idanalyzer.com/";
            }
            else
            {
                this.ApiEndpoint = region;
            }

        }

        /// <summary>
        /// Raise APIException when API returns an error
        /// </summary>
        /// <param name="throwException">Enable or Disable APIException being thrown</param>
        public void ThrowAPIException(bool throwException = false)
        {
            this.throwError = throwException;
        }



        /// <summary>
        /// Get a single vault entry
        /// </summary>
        /// <param name="vault_id">Vault entry ID</param>
        /// <returns>Vault entry data</returns>
        public async Task<JObject> Get(string vault_id = "")
        {
            if(vault_id == ""){
                throw new ArgumentException("Vault entry ID required.");
            }
            Hashtable payload = new Hashtable
            {
                ["id"] = vault_id
            };
            return await this.CallAPI("get", payload);

        }


        /// <summary>
        /// List multiple vault entries with optional filter, sorting and paging arguments
        /// </summary>
        /// <param name="filter">Array of filter statements, refer to https://developer.idanalyzer.com/vaultapi.html for filter construction</param>
        /// <param name="orderby">Field name used to order the results, refer to https://developer.idanalyzer.com/vaultapi.html for available fields</param>
        /// <param name="sort">Sort results by ASC = Ascending, DESC = DESCENDING</param>
        /// <param name="limit">Number of results to return</param>
        /// <param name="offset">Offset the first result using specified index</param>
        /// <returns>Vault entry list</returns>
        public async Task<JObject> List(string[] filter, string orderby = "createtime", string sort = "DESC", int limit = 10, int offset = 0)
        {

            if (filter.Length > 5) {
                throw new ArgumentException("Filter should be an array containing maximum of 5 filter statements.");
            }

            Hashtable payload = new Hashtable
            {
                ["filter"] = filter,
                ["orderby"] = orderby,
                ["sort"] = sort,
                ["limit"] = limit,
                ["offset"] = offset
            };

            return await this.CallAPI("list", payload);


        }


        /// <summary>
        /// Update vault entry with new data
        /// </summary>
        /// <param name="vault_id">Vault entry ID</param>
        /// <param name="data">Key-value hashtable of the field name and its value</param>
        public async Task<JObject> Update(string vault_id, Hashtable data)
        {
            if(vault_id == ""){
                throw new ArgumentException("Vault entry ID required.");
            }
            if(data.Count < 1){
                throw new ArgumentException("Data required.");
            }
            data["id"] = vault_id;
            return await this.CallAPI("update", data);

        }

        /// <summary>
        /// Delete a single or multiple vault entries
        /// </summary>
        /// <param name="vault_id">Vault entry ID array</param>
        public async Task<bool> Delete(string[] vault_id)
        {
            if(vault_id.Length <1){
                throw new ArgumentException("Vault entry ID required.");
            }
            Hashtable payload = new Hashtable
            {
                ["id"] = vault_id
            };
            await this.CallAPI("delete", payload);
            return true;


        }


        /// <summary>
        /// Add a document or face image into an existing vault entry
        /// </summary>
        /// <param name="vault_id">Vault entry ID</param>
        /// <param name="image">Image file path or URL</param>
        /// <param name="type">Type of image: 0 = document, 1 = person</param>
        public async Task<JObject> AddImage(string vault_id, string image, int type = 0)
        {
            if(vault_id == ""){
                throw new ArgumentException("Vault entry ID required.");
            }
            if(type != 0 && type!=1){
                throw new ArgumentException("Invalid image type, 0 or 1 accepted.");
            }
            Hashtable payload = new Hashtable
            {
                ["id"] = vault_id,
                ["type"] = type
            };

  
            if(IsValidURL(image)){
                payload["imageurl"] = image;
            }else if(File.Exists(image)){
                Byte[] bytes = File.ReadAllBytes(image);
                payload["image"] = Convert.ToBase64String(bytes);
            }else{
                throw new ArgumentException("Invalid image, file not found or malformed URL.");
            }

            return await this.CallAPI("addimage", payload);

        }


        /// <summary>
        /// Delete an image from vault
        /// </summary>
        /// <param name="vault_id">Vault entry ID</param>
        /// <param name="image_id">Image ID</param>
        public async Task<bool> DeleteImage(string vault_id, string image_id)
        {
            if(vault_id == ""){
                throw new ArgumentException("Vault entry ID required.");
            }
            if(image_id == ""){
                throw new ArgumentException("Image ID required.");
            }
            Hashtable payload = new Hashtable
            {
                ["id"] = vault_id,
                ["imageid"] = image_id
            };
            await this.CallAPI("deleteimage", payload);
            return true;


        }


        /// <summary>
        /// Search vault using a person's face image
        /// </summary>
        /// <param name="image">Face image file path or URL</param>
        /// <param name="maxEntry">Number of entries to return, 1 to 10.</param>
        /// <param name="threshold">Minimum confidence score required for face matching</param>
        public async Task<JObject> SearchFace(string image, int maxEntry = 10, double threshold = 0.5)
        {
            Hashtable payload = new Hashtable
            {
                ["maxentry"] = maxEntry,
                ["threshold"] = threshold
            };
          
            if(IsValidURL(image)){
                payload["imageurl"] = image;
            }else if(File.Exists(image)){
                Byte[] bytes = File.ReadAllBytes(image);
                payload["image"] = Convert.ToBase64String(bytes);
            }
            else{
                throw new ArgumentException("Invalid image, file not found or malformed URL.");
            }

            return await this.CallAPI("searchface", payload);

        }

        /// <summary>
        /// Train vault for face search
        /// </summary>
        public async Task<JObject> TrainFace()
        {
            return await this.CallAPI("train", new Hashtable());
        }

        /// <summary>
        /// Get vault training status
        /// </summary>
        public async Task<JObject> TrainingStatus()
        {
            return await this.CallAPI("trainstatus", new Hashtable());

        }



        private async Task<JObject> CallAPI(string action, Hashtable payload){
            payload["apikey"] = this.Apikey;
            payload["client"] = "dotnet-sdk";


            string json = JsonConvert.SerializeObject(payload);
            Console.WriteLine(json);
            var response = await APIclient.PostAsync(this.ApiEndpoint + "vault/" + action, new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            string jsonresponse = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(jsonresponse);

            if (this.throwError)
            {

                if (result.ContainsKey("error"))
                {
                    throw new APIException((string)result["error"]["message"], (int)result["error"]["code"]);
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return result;
            }


        }




    }
}
