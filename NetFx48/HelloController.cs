using System;
using System.Threading.Tasks;
using System.Web.Http;
using static NetFx48.Models;
using System.Xml;

namespace NetFx48
{
    public class HelloController : ApiController
    {
        // Use default route for testing
        [HttpGet]
        public async Task<IHttpActionResult> GetGreeting()
        {
            try
            {
                Console.WriteLine("\nStarting Onboarding process...");

                // Assuming OnboardingStep.DeviceOnboarding is an async method.
                OnboardingStep zatcaService = new();
                var certificateInfo = await OnboardingStep.DeviceOnboarding();

                Helpers.SerializeToFile(certificateInfo, Helpers.GetAbsolutePath(AppConfig.CertificateInfoPath));
                Console.WriteLine("\nOnboarding process completed successfully.\n");

                Console.WriteLine("\nStarting Test Approval...\n");

                certificateInfo = Helpers.DeserializeFromFile<CertificateInfo>(Helpers.GetAbsolutePath(AppConfig.CertificateInfoPath));

                if (certificateInfo != null)
                {
                    // Assuming that the XML loading and processing is also async
                    XmlDocument document = new() { PreserveWhitespace = true };
                    document.Load(Helpers.GetAbsolutePath(AppConfig.TemplateInvoicePath));

                    // Awaiting any asynchronous document processing methods.
                    await ProcessStandardDocuments(document);
                    await ProcessSimplifiedDocuments(document);

                    Console.WriteLine("\n\nALL DONE!\n\n");
                }
                else
                {
                    Console.WriteLine("TEST FAILED!");
                }

                // Return a response indicating success
                return Ok("Hello from the API, onboarding and testing complete!");
            }
            catch (Exception ex)
            {
                // Logging the error and returning an internal server error response
                Console.WriteLine($"An error occurred: {ex.Message}");
                return InternalServerError(ex);
            }
        }

        // Assuming these are defined elsewhere in your code
        private async Task ProcessStandardDocuments(XmlDocument document)
        {
            // Implement your processing logic here (async if required)
        }

        private async Task ProcessSimplifiedDocuments(XmlDocument document)
        {
            // Implement your processing logic here (async if required)
        }
    }
}
