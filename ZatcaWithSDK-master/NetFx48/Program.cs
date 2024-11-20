using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using static NetFx48.Models;

namespace NetFx48
{
    class Program
    {
        private static CertificateInfo certificateInfo;

        static async Task Main()
        {
            // Start of the invoice generation process
            Console.WriteLine("----------------------------Starting invoice generation------------------------------------------------------");

            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templateInvoice.json");

            // Check if the JSON file exists
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("JSON file not found!");
                return;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            dynamic jsonData = JsonConvert.DeserializeObject<ExpandoObject>(jsonContent);

            XmlDocument invoiceXml = new XmlDocument();

            // Create XML structure
            XmlElement root = invoiceXml.CreateElement("Invoice");
            root.SetAttribute("xmlns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            root.SetAttribute("xmlns:cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            root.SetAttribute("xmlns:cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            invoiceXml.AppendChild(root);

            // Adding data to XML from JSON with null checks
            AppendChildElement(invoiceXml, root, "cbc:ProfileID", jsonData.ProfileID, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:ID", jsonData.ID, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:UUID", jsonData.UUID, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:IssueDate", jsonData.IssueDate, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:IssueTime", jsonData.IssueTime, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");


            // Invoice Type Code with Attributes
            XmlElement invoiceTypeCode = invoiceXml.CreateElement("cbc:InvoiceTypeCode");
            invoiceTypeCode.SetAttribute("name", jsonData.InvoiceTypeCode?.name);
            invoiceTypeCode.InnerText = jsonData.InvoiceTypeCode?.value;
            root.AppendChild(invoiceTypeCode);

            // Note with Attributes
            XmlElement note = invoiceXml.CreateElement("cbc:Note");
            note.SetAttribute("languageID", jsonData.Note?.languageID);
            note.InnerText = jsonData.Note?.value;
            root.AppendChild(note);

            // Adding more fields to XML
            AppendChildElement(invoiceXml, root, "cbc:DocumentCurrencyCode", jsonData.DocumentCurrencyCode, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:TaxCurrencyCode", jsonData.TaxCurrencyCode, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            // Billing Reference
            XmlElement billingReference = invoiceXml.CreateElement("cac:BillingReference");
            root.AppendChild(billingReference);
            XmlElement invoiceDocRef = invoiceXml.CreateElement("cac:InvoiceDocumentReference");
            billingReference.AppendChild(invoiceDocRef);
            AppendChildElement(invoiceXml, invoiceDocRef, "cbc:ID", jsonData.BillingReference?.InvoiceDocumentReference?.ID, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            // Define the full output path where the file will be saved
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.xml");

            // Print the full path for debugging
            Console.WriteLine("Saving XML to: " + outputPath);

            try
            {
                // Save the XML file to the determined path
                invoiceXml.Save(outputPath);
                Console.WriteLine("XML file saved successfully.");
            }
            catch (Exception ex)
            {
                // Handle any error that occurs during file saving
                Console.WriteLine("Error saving XML file: " + ex.Message);
            }

            // Print the generated XML content
            Console.WriteLine("\nGenerated XML content:");
            Console.WriteLine(invoiceXml.OuterXml);

            Console.WriteLine("---------------------------------------------end invoice generation-----------------------------------");

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

        private static void AppendChildElement(XmlDocument doc, XmlElement parent, string elementName, string value, string namespaceUri)
        {
            if (value != null)
            {
                XmlElement child = doc.CreateElement(elementName, namespaceUri);
                child.InnerText = value;
                parent.AppendChild(child);
            }
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
