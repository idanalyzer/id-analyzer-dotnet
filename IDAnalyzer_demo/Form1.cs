using System;
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
        const string API_KEY = "Your APU Key";
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
    }
}
