using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IDAnalyzer;
using Newtonsoft.Json.Linq;

namespace IDAnalyzer_demo
{
    public partial class Form1 : Form
    {
        const string API_KEY = "Your API Key";
        const string API_REGION = "US";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScanID();
        }

        private void writeOutput(string msg)
        {
            textBox4.AppendText(msg + Environment.NewLine);
        }
        private void clearOutput()
        {
            textBox4.Text = "";
        }
        private async void getVaultData()
        {
            button6.Enabled = false;
            clearOutput();

            try
            {
                Vault vault = new Vault(API_KEY, API_REGION);

                // Throw exception if API returns error
                vault.ThrowAPIException(true);

                JObject result = await vault.Get(textBox5.Text);

                writeOutput(result.ToString());

            }
            catch (APIException e)
            {
                writeOutput("Error Code: " + e.ErrorCode);
                writeOutput("Error Message: " + e.Message);
            }
            catch (ArgumentException e)
            {
                writeOutput("Input Error: " + e.Message);
            }
            catch (Exception e)
            {
                writeOutput("Unexpected Error: " + e.Message);
            }
           

            button6.Enabled = true;

        }
        private async void listVaultData()
        {
            button7.Enabled = false;
            clearOutput();

            try
            {
                Vault vault = new Vault(API_KEY, API_REGION);

                // Throw exception if API returns error
                vault.ThrowAPIException(true);

                // Construct a filter string array, can contain max of 5 filter statements
                string[] filter = { "createtime>=" + textBox6.Text};

                JObject result = await vault.List(filter, "createtime", "ASC", 5);

               
                if ((int)result["total"] > 0)
                {
                    foreach (JObject vaultitem in result["items"])
                    { 
                        writeOutput(vaultitem["createtime"] + ": " + vaultitem["id"]);
                    }
                }
                else
                {
                    writeOutput("No result");
                }

                // writeOutput(result.ToString());

            }
            catch (APIException e)
            {
                writeOutput("Error Code: " + e.ErrorCode);
                writeOutput("Error Message: " + e.Message);
            }
            catch (ArgumentException e)
            {
                writeOutput("Input Error: " + e.Message);
            }
            catch (Exception e)
            {
                writeOutput("Unexpected Error: " + e.Message);
            }


            button7.Enabled = true;

        }
        private async void searchAMLbyName()
        {
            try { 
                AMLAPI aml = new AMLAPI(API_KEY,  API_REGION);

                aml.ThrowAPIException(true);

                JObject result = await aml.SearchByName(textBox8.Text);

                writeOutput("AML Search Result: ");
                writeOutput(result.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (APIException e)
            {
                writeOutput("Error Code: " + e.ErrorCode);
                writeOutput("Error Message: " + e.Message);
            }
            catch (ArgumentException e)
            {
                writeOutput("Input Error: " + e.Message);
            }
            catch (Exception e)
            {
                writeOutput("Unexpected Error: " + e.Message);
            }

        }
        private async void searchAMLbyIDNumber()
        {

            try
            {
                AMLAPI aml = new AMLAPI(API_KEY, API_REGION);

                aml.ThrowAPIException(true);

                JObject result = await aml.SearchByIDNumber(textBox9.Text);

                writeOutput("AML Search Result: ");
                writeOutput(result.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (APIException e)
            {
                writeOutput("Error Code: " + e.ErrorCode);
                writeOutput("Error Message: " + e.Message);
            }
            catch (ArgumentException e)
            {
                writeOutput("Input Error: " + e.Message);
            }
            catch (Exception e)
            {
                writeOutput("Unexpected Error: " + e.Message);
            }
           

        }
        private async void CreateDocuPass()
        {
            button5.Enabled = false;
            clearOutput();
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


                /*
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
                docupass.SetCustomHTML("https://www.yourwebsite.com/docupass_template.html"); // use your own HTML/CSS for DocuPass page
                docupass.SMSVerificationLink("+1333444555"); // Send verification link to user's mobile phone
                docupass.EnablePhoneVerification(true); // get user to input their own phone number for verification
                docupass.VerifyPhone("+1333444555"); // verify user's phone number you already have in your database
                docupass.EnableAMLCheck(true); // enable AML/PEP compliance check
                docupass.SetAMLDatabase("global_politicians,eu_meps,eu_cors"); // limit AML check to only PEPs
                docupass.EnableAMLStrictMatch(true); // make AML matching more strict to prevent false positives
                docupass.GenerateContract("Template ID", "PDF", new Hashtable { ["somevariable"] = "somevalue" }); // automate paperwork by generating a document autofilled with ID data
                docupass.SignContract("Template ID", "PDF", new Hashtable { ["somevariable"] = "somevalue" }); // get user to review and sign legal document prefilled with ID data
                
                */


                string docupass_module = comboBox1.Text.Substring(0, 1);

                JObject result;

                // Create a session using DocuPass
                switch (docupass_module)
                {
                    case "0":
                        result = await docupass.CreateIframe();

                        writeOutput("Embed following URL on your website: ");
                        writeOutput((string)result["url"]);

                        break;
                    case "1":
                        result = await docupass.CreateMobile();

                        writeOutput("Scan the QR Code below to verify your identity: ");
                        writeOutput((string)result["qrcode"]);
                        writeOutput("Or open your mobile browser and type in: ");
                        writeOutput((string)result["url"]);


                        break;
                    case "2":
                        result = await docupass.CreateRedirection();

                        writeOutput("Redirect your user to the following URL: ");
                        writeOutput((string)result["url"]);
                        break;
                    case "3":
                        result = await docupass.CreateLiveMobile();

                        writeOutput("Scan the QR Code below to verify your identity:");
                        writeOutput((string)result["qrcode"]);
                        writeOutput("Or open your mobile browser and type in: ");
                        writeOutput((string)result["url"]);

                        break;
                    default:
                        throw new ArgumentException("Invalid Module");
                }

                writeOutput(Environment.NewLine+"Raw JSON Result: ");
                writeOutput(result.ToString());


            }
            catch (APIException e)
            {
                writeOutput("Error Code: " + e.ErrorCode);
                writeOutput("Error Message: " + e.Message);
            }
            catch (ArgumentException e)
            {
                writeOutput("Input Error: " + e.Message);
            }
            catch (Exception e)
            {
                writeOutput("Unexpected Error: " + e.Message);
            }

            button5.Enabled = true;
            
        }

        private async void CreateDocuPassSignature()
        {
            button10.Enabled = false;
            clearOutput();
            try
            {

                DocuPass docupass = new DocuPass(API_KEY, "My Company Inc.", API_REGION);

                // Throw exception if API returns error
                docupass.ThrowAPIException(true);

                // We need to set an identifier so that we know internally who is signing the document, this string will be returned in the callback. You can use your own user/customer id.
                docupass.SetCustomID(textBox11.Text);

                // Enable vault cloud storage to store signed document
                docupass.EnableVault(true);

                // Set a callback URL where signed document will be sent, you can use docupass_callback.php under this folder as a template to receive the result
                docupass.SetCallbackURL("https://www.your-website.com/docupass_callback.php");

                // We want to redirect user back to your website when they are done with document signing, there will be no fail URL unlike identity verification
                docupass.SetRedirectionURL("https://www.your-website.com/verification_succeeded.php");


                /*
                docupass.SetReusable(true); // allow DocuPass URL/QR Code to be used by multiple users  
                docupass.SetLanguage("en"); // override auto language detection  
                docupass.SetQRCodeFormat("000000", "FFFFFF", 5, 1); // generate a QR code using custom colors and size  
                docupass.HideBrandingLogo(true); // hide branding footer  
                docupass.SetCustomHTML("https://www.yourwebsite.com/docupass_template.html"); // use your own HTML/CSS for DocuPass page
                docupass.SMSContractLink("+1333444555"); // Send signing link to user's mobile phone
                */


                // Get template ID
                string template_id = textBox10.Text;

                // Assuming in your contract template you have a dynamic field %{email} and you want to fill it with user email

                Hashtable prefill = new Hashtable
                {
                    ["email"] = "user@example.com"
                };


                JObject result = await docupass.CreateSignature(template_id, "PDF", prefill);

                writeOutput("Scan the QR Code below to sign document:");
                writeOutput((string)result["qrcode"]);
                writeOutput("Or open your browser and navigate to: ");
                writeOutput((string)result["url"]);


                writeOutput(Environment.NewLine + "Raw JSON Result: ");
                writeOutput(result.ToString());


            }
            catch (APIException e)
            {
                writeOutput("Error Code: " + e.ErrorCode);
                writeOutput("Error Message: " + e.Message);
            }
            catch (ArgumentException e)
            {
                writeOutput("Input Error: " + e.Message);
            }
            catch (Exception e)
            {
                writeOutput("Unexpected Error: " + e.Message);
            }

            button10.Enabled = true;

        }

        private async void ScanID()
        {
            button1.Enabled = false;
            clearOutput();
            try {
               
                CoreAPI coreapi = new CoreAPI(API_KEY, API_REGION);

                // Throw exception if API returns error
                coreapi.ThrowAPIException(true);

                // Enable authentication and use 'quick' module to check if ID is authentic
                coreapi.EnableAuthentication(true, "quick");


                /*
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
                coreapi.EnableAMLCheck(true); // enable AML/PEP check
                coreapi.SetAMLDatabase("global_politicians,eu_meps,eu_cors"); // limit AML check to only PEPs
                coreapi.EnableAMLStrictMatch(true); // make AML matching more strict to prevent false positives
                coreapi.GenerateContract("Template ID", "PDF", new Hashtable { ["somevariable"] = "somevalue" }); // generate a PDF document autofilled with data from user ID
                 */



                // Send document to Core API and get json response
                JObject result = await coreapi.Scan(textBox1.Text, textBox2.Text, textBox3.Text);

                // Print document holder name
                writeOutput(String.Format("Hello your name is {0} {1}", (string)result.SelectToken("result.firstName"), (string)result.SelectToken("result.lastName")));

                // Parse document authentication results  
                if (result.ContainsKey("authentication")){
                    if ((double)result.SelectToken("authentication.score") > 0.5) {
                        writeOutput("The document uploaded is authentic");
                    }else if ((double)result.SelectToken("authentication.score") > 0.3){
                        writeOutput("The document uploaded looks little bit suspicious");
                    }else
                    {
                        writeOutput("The document uploaded is fake");
                    }
                }
                // Parse face verification results  
                if (result.ContainsKey("face")){
                    if (result.SelectToken("face.error") != null){
                        // View complete error codes under API reference: https://developer.idanalyzer.com/coreapi.html
                        writeOutput(String.Format("Face verification failed! Code: {0}, Reason: {1}", (string)result.SelectToken("face.error"), (string)result.SelectToken("face.error_message")));
                    }else
                    {
                        if ((bool)result.SelectToken("face.isIdentical") == true){
                            writeOutput("Great! Your photo looks identical to the photo on document");
                        }else
                        {
                            writeOutput("Oh no! Your photo looks different to the photo on document");
                        }
                        writeOutput(String.Format("Similarity score: {0}", (string)result.SelectToken("face.confidence")));
                    }
                }
                // Print result
                writeOutput(Environment.NewLine + "Raw JSON Result: ");
                writeOutput(result.ToString());
            }catch(APIException e)
            {
                writeOutput("Error Code: " + e.ErrorCode);
                writeOutput("Error Message: " + e.Message);
            }
            catch (ArgumentException e)
            {
                writeOutput("Input Error: " + e.Message);
            }
            catch (Exception e)
            {
                writeOutput("Unexpected Error: " + e.Message);
            }
            button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog1.FileName;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            CreateDocuPass();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            getVaultData();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listVaultData();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            searchAMLbyName();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            searchAMLbyIDNumber();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            CreateDocuPassSignature();
        }
    }
}
