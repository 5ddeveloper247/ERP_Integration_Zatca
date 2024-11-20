using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using static NetFx48.Models;
using Microsoft.Owin.Hosting;

namespace NetFx48
{
    class Program
    {
        private static CertificateInfo certificateInfo;

        static async Task Main()
        {


            // Base address for the API
            string baseAddress = "http://localhost:9000/";

            // Start the OWIN host
            using (WebApp.Start<Startup>(baseAddress))
            {
                Console.WriteLine($"API is running at {baseAddress}");
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
           

            // Start of the invoice generation process
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templateInvoice.json");
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.xml");

            InvoiceGenerator invoiceGenerator = new InvoiceGenerator(jsonPath, outputPath);
            invoiceGenerator.GenerateInvoice();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("\nStarting the console app...");

            try
            {
                Console.WriteLine("\nStarting Onboarding process...");
                OnboardingStep zatcaService = new();
                certificateInfo = await OnboardingStep.DeviceOnboarding();
                Helpers.SerializeToFile(certificateInfo, Helpers.GetAbsolutePath(AppConfig.CertificateInfoPath));
                Console.WriteLine("\nOnboarding process completed successfully.\n");

                Console.WriteLine("\nStarting Test Approval...\n");
                certificateInfo = Helpers.DeserializeFromFile<CertificateInfo>(Helpers.GetAbsolutePath(AppConfig.CertificateInfoPath));

                if (certificateInfo != null)
                {
                    XmlDocument document = new() { PreserveWhitespace = true };
                    document.Load(Helpers.GetAbsolutePath(AppConfig.TemplateInvoicePath));

                    await ProcessStandardDocuments(document);
                    await ProcessSimplifiedDocuments(document);

                    Console.WriteLine("\n\nALL DONE!\n\n");
                }
                else
                {
                    Console.WriteLine("TEST FAILED!");
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.ReadLine();
        }


        // Placeholder for missing methods
        private static async Task ProcessStandardDocuments(XmlDocument document)
        {
            // Implement the logic for processing standard documents
            await Task.CompletedTask;
        }

        private static async Task ProcessSimplifiedDocuments(XmlDocument document)
        {
            // Implement the logic for processing simplified documents
            await Task.CompletedTask;
        }

        private static void LogServerResult(ServerResult serverResult, string description)
        {
            JsonSerializerSettings settings = new() { NullValueHandling = NullValueHandling.Ignore };

            if (serverResult != null)
            {
                bool isClearedOrReported = (serverResult.ClearanceStatus?.Contains("CLEARED") == true) ||
                                           (serverResult.ReportingStatus?.Contains("REPORTED") == true);
                bool isNotClearedOrReported = (serverResult.ClearanceStatus?.Contains("NOT") == true) ||
                                              (serverResult.ReportingStatus?.Contains("NOT") == true);

                if (isClearedOrReported || isNotClearedOrReported)
                {
                    certificateInfo.ICV += 1;
                    certificateInfo.PIH = serverResult.InvoiceHash;
                    serverResult.InvoiceHash = null;
                    Console.WriteLine($"{description}\n\n{JsonConvert.SerializeObject(serverResult, Newtonsoft.Json.Formatting.Indented, settings)}\n\n");

                    Helpers.SerializeToFile(certificateInfo, Helpers.GetAbsolutePath(AppConfig.CertificateInfoPath));
                }
                else
                {
                    Console.WriteLine($"{description} was Rejected! \n\n{JsonConvert.SerializeObject(serverResult, Newtonsoft.Json.Formatting.Indented, settings)}\n\n");
                }
            }
            else
            {
                Console.WriteLine($"\n\nError processing {description}\n\n");
            }
        }
    }
}
