using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace NetFx48
{
    public class InvoiceGenerator
    {
        private readonly string jsonPath;
        private readonly string outputPath;

        public InvoiceGenerator(string jsonPath, string outputPath)
        {
            this.jsonPath = jsonPath;
            this.outputPath = outputPath;
        }

        public void GenerateInvoice()
        {
            Console.WriteLine("----------------------------Starting invoice generation------------------------------------------------------");

            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("JSON file not found!");
                return;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            dynamic jsonData = JsonConvert.DeserializeObject<ExpandoObject>(jsonContent);

            XmlDocument invoiceXml = new XmlDocument();
            // Create the XML declaration
            XmlDeclaration xmlDeclaration = invoiceXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            // Append the XML declaration to the document
            invoiceXml.AppendChild(xmlDeclaration);


            XmlElement root = invoiceXml.CreateElement("Invoice");
            root.SetAttribute("xmlns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            root.SetAttribute("xmlns:cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            root.SetAttribute("xmlns:cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            root.SetAttribute("xmlns:ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            invoiceXml.AppendChild(root);

            // Basic fields
            AppendChildElement(invoiceXml, root, "cbc:ProfileID", jsonData.Invoice.ProfileID, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:ID", jsonData.Invoice.ID, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:UUID", jsonData.Invoice.UUID, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:IssueDate", jsonData.Invoice.IssueDate, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:IssueTime", jsonData.Invoice.IssueTime, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");



            string cbcNamespaceUri = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            string cacNamespaceUri = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Invoice Type Code
            XmlElement invoiceTypeCode = invoiceXml.CreateElement("cbc:InvoiceTypeCode", cbcNamespaceUri);
            invoiceTypeCode.SetAttribute("name", jsonData.Invoice.InvoiceTypeCode?.name);
            invoiceTypeCode.InnerText = jsonData.Invoice.InvoiceTypeCode?.value;
            root.AppendChild(invoiceTypeCode);
          


          
                // Note with language attribute
                XmlElement note = invoiceXml.CreateElement("cbc:Note", cbcNamespaceUri);
                // Set the languageID attribute if it exists in JSON data
                note.SetAttribute("languageID", jsonData.Invoice.Note.languageID);
                note.InnerText = jsonData.Invoice.Note.value;
                root.AppendChild(note); 

          
            // Document Currency Code and Tax Currency Code
            AppendChildElement(invoiceXml, root, "cbc:DocumentCurrencyCode", jsonData.Invoice.DocumentCurrencyCode, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            AppendChildElement(invoiceXml, root, "cbc:TaxCurrencyCode", jsonData.Invoice.TaxCurrencyCode, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");


            // Billing Reference
            XmlElement billingReference = invoiceXml.CreateElement("cac:BillingReference", cacNamespaceUri);
            XmlElement invoiceDocRef = invoiceXml.CreateElement("cac:InvoiceDocumentReference", cacNamespaceUri);
            // Create the cbc:ID element with the required content
            XmlElement idElement = invoiceXml.CreateElement("cbc:ID", cbcNamespaceUri);
            idElement.InnerText = jsonData.Invoice.BillingReference?.InvoiceDocumentReference?.ID ?? "Invoice Number: 354; Invoice Issue Date: 2021-02-10";

            // Append elements in the correct hierarchy
            invoiceDocRef.AppendChild(idElement);
            billingReference.AppendChild(invoiceDocRef);
            root.AppendChild(billingReference);
           

            // Additional Document Reference
           
            XmlElement AdditionalDocumentReference = invoiceXml.CreateElement("cac:AdditionalDocumentReference", cacNamespaceUri);
            XmlElement idElementRef = invoiceXml.CreateElement("cbc:ID", cbcNamespaceUri);
            XmlElement UUidElement = invoiceXml.CreateElement("cbc:UUID", cbcNamespaceUri);
            idElementRef.InnerText = jsonData.Invoice.AdditionalDocumentReference[0].ID;
            UUidElement.InnerText = jsonData.Invoice.AdditionalDocumentReference[0].UUID;
            AdditionalDocumentReference.AppendChild(idElementRef);
            AdditionalDocumentReference.AppendChild(UUidElement);
            root.AppendChild(AdditionalDocumentReference);


            //Additional Document Reference 2
            XmlElement AdditionalDocumentReference2 = invoiceXml.CreateElement("cac:AdditionalDocumentReference", cacNamespaceUri);
            XmlElement idElementRef2 = invoiceXml.CreateElement("cbc:ID", cbcNamespaceUri);
            idElementRef2.InnerText = jsonData.Invoice.AdditionalDocumentReference[1].ID;
            AdditionalDocumentReference2.AppendChild(idElementRef2);

           XmlElement refAttachment= invoiceXml.CreateElement("cac:Attachment", cacNamespaceUri);
           XmlElement refBinaryObj= invoiceXml.CreateElement("cbc:EmbeddedDocumentBinaryObject", cbcNamespaceUri);
           refBinaryObj.SetAttribute("mimeCode", "text/plain");
           refAttachment.AppendChild(refBinaryObj);
            refBinaryObj.InnerText=jsonData.Invoice.AdditionalDocumentReference[1].Attachment.EmbeddedDocumentBinaryObject.value;
            AdditionalDocumentReference2.AppendChild(refAttachment);
            AdditionalDocumentReference2.AppendChild(refAttachment);
            root.AppendChild(AdditionalDocumentReference2);





            //Accounting Supplier Party 
            XmlElement accountingSupplierParty = invoiceXml.CreateElement("cac", "AccountingSupplierParty", cacNamespaceUri);
            XmlElement party = invoiceXml.CreateElement("cac", "Party", cacNamespaceUri);

            // Party Identification
            XmlElement partyIdentification = invoiceXml.CreateElement("cac", "PartyIdentification", cacNamespaceUri);
            XmlElement id = invoiceXml.CreateElement("cbc", "ID", cbcNamespaceUri);
            id.SetAttribute("schemeID", "CRN");
            id.InnerText = "1010010000";
            partyIdentification.AppendChild(id);
            party.AppendChild(partyIdentification);

            // Postal Address
            XmlElement postalAddress = invoiceXml.CreateElement("cac", "PostalAddress", cacNamespaceUri);
            XmlElement streetName = invoiceXml.CreateElement("cbc", "StreetName", cbcNamespaceUri);
            streetName.InnerText = "الامير سلطان | Prince Sultan";
            postalAddress.AppendChild(streetName);

            XmlElement buildingNumber = invoiceXml.CreateElement("cbc", "BuildingNumber", cbcNamespaceUri);
            buildingNumber.InnerText = "2322";
            postalAddress.AppendChild(buildingNumber);

            XmlElement citySubdivisionName = invoiceXml.CreateElement("cbc", "CitySubdivisionName", cbcNamespaceUri);
            citySubdivisionName.InnerText = "المربع | Al-Murabba";
            postalAddress.AppendChild(citySubdivisionName);

            XmlElement cityName = invoiceXml.CreateElement("cbc", "CityName", cbcNamespaceUri);
            cityName.InnerText = "الرياض | Riyadh";
            postalAddress.AppendChild(cityName);

            XmlElement postalZone = invoiceXml.CreateElement("cbc", "PostalZone", cbcNamespaceUri);
            postalZone.InnerText = "23333";
            postalAddress.AppendChild(postalZone);

            // Country
            XmlElement country = invoiceXml.CreateElement("cac", "Country", cacNamespaceUri);
            XmlElement countryId = invoiceXml.CreateElement("cbc", "IdentificationCode", cbcNamespaceUri);
            countryId.InnerText = "SA";
            country.AppendChild(countryId);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            // Party Tax Scheme
            XmlElement partyTaxScheme = invoiceXml.CreateElement("cac", "PartyTaxScheme", cacNamespaceUri);
            XmlElement companyID = invoiceXml.CreateElement("cbc", "CompanyID", cbcNamespaceUri);
            companyID.InnerText = "399999999900003";
            partyTaxScheme.AppendChild(companyID);

            XmlElement taxScheme = invoiceXml.CreateElement("cac", "TaxScheme", cacNamespaceUri);
            XmlElement taxId = invoiceXml.CreateElement("cbc", "ID", cbcNamespaceUri);
            taxId.InnerText = "VAT";
            taxScheme.AppendChild(taxId);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            // Party Legal Entity
            XmlElement partyLegalEntity = invoiceXml.CreateElement("cac", "PartyLegalEntity", cacNamespaceUri);
            XmlElement registrationName = invoiceXml.CreateElement("cbc", "RegistrationName", cbcNamespaceUri);
            registrationName.InnerText = "شركة توريد التكنولوجيا بأقصى سرعة المحدودة | Maximum Speed Tech Supply LTD";
            partyLegalEntity.AppendChild(registrationName);
            party.AppendChild(partyLegalEntity);

            // Append Party to Accounting Supplier Party and add to root
            accountingSupplierParty.AppendChild(party);
            root.AppendChild(accountingSupplierParty);

            

            //Accounting Customer Party EmbeddedDocumentBinaryObject 

            // Accounting Customer Party
            XmlElement accountingCustomerParty = invoiceXml.CreateElement("cac", "AccountingCustomerParty", cacNamespaceUri);
            XmlElement customerParty = invoiceXml.CreateElement("cac", "Party", cacNamespaceUri);

            // Postal Address
            XmlElement customerPostalAddress = invoiceXml.CreateElement("cac", "PostalAddress", cacNamespaceUri);
            XmlElement customerStreetName = invoiceXml.CreateElement("cbc", "StreetName", cbcNamespaceUri);
            customerStreetName.InnerText = "صلاح الدين | Salah Al-Din";
            customerPostalAddress.AppendChild(customerStreetName);

            XmlElement customerBuildingNumber = invoiceXml.CreateElement("cbc", "BuildingNumber", cbcNamespaceUri);
            customerBuildingNumber.InnerText = "1111";
            customerPostalAddress.AppendChild(customerBuildingNumber);

            XmlElement customerCitySubdivisionName = invoiceXml.CreateElement("cbc", "CitySubdivisionName", cbcNamespaceUri);
            customerCitySubdivisionName.InnerText = "المروج | Al-Murooj";
            customerPostalAddress.AppendChild(customerCitySubdivisionName);

            XmlElement customerCityName = invoiceXml.CreateElement("cbc", "CityName", cbcNamespaceUri);
            customerCityName.InnerText = "الرياض | Riyadh";
            customerPostalAddress.AppendChild(customerCityName);

            XmlElement customerPostalZone = invoiceXml.CreateElement("cbc", "PostalZone", cbcNamespaceUri);
            customerPostalZone.InnerText = "12222";
            customerPostalAddress.AppendChild(customerPostalZone);

            // Country
            XmlElement customerCountry = invoiceXml.CreateElement("cac", "Country", cacNamespaceUri);
            XmlElement customerCountryId = invoiceXml.CreateElement("cbc", "IdentificationCode", cbcNamespaceUri);
            customerCountryId.InnerText = "SA";
            customerCountry.AppendChild(customerCountryId);
            customerPostalAddress.AppendChild(customerCountry);
            customerParty.AppendChild(customerPostalAddress);

            // Party Tax Scheme
            XmlElement customerPartyTaxScheme = invoiceXml.CreateElement("cac", "PartyTaxScheme", cacNamespaceUri);
            XmlElement customerCompanyID = invoiceXml.CreateElement("cbc", "CompanyID", cbcNamespaceUri);
            customerCompanyID.InnerText = "399999999800003";
            customerPartyTaxScheme.AppendChild(customerCompanyID);

            XmlElement customerTaxScheme = invoiceXml.CreateElement("cac", "TaxScheme", cacNamespaceUri);
            XmlElement customerTaxId = invoiceXml.CreateElement("cbc", "ID", cbcNamespaceUri);
            customerTaxId.InnerText = "VAT";
            customerTaxScheme.AppendChild(customerTaxId);
            customerPartyTaxScheme.AppendChild(customerTaxScheme);
            customerParty.AppendChild(customerPartyTaxScheme);

            // Party Legal Entity
            XmlElement customerPartyLegalEntity = invoiceXml.CreateElement("cac", "PartyLegalEntity", cacNamespaceUri);
            XmlElement customerRegistrationName = invoiceXml.CreateElement("cbc", "RegistrationName", cbcNamespaceUri);
            customerRegistrationName.InnerText = "شركة نماذج فاتورة المحدودة | Fatoora Samples LTD";
            customerPartyLegalEntity.AppendChild(customerRegistrationName);
            customerParty.AppendChild(customerPartyLegalEntity);

            // Append Party to Accounting Customer Party and add to root
            accountingCustomerParty.AppendChild(customerParty);
            root.AppendChild(accountingCustomerParty);

            //Accounting Customer Party EmbeddedDocumentBinaryObject  end


            // Delivery
           
            XmlElement delivery = invoiceXml.CreateElement("cac", "Delivery", cacNamespaceUri);
            XmlElement actualDeliveryDate = invoiceXml.CreateElement("cbc", "ActualDeliveryDate", cbcNamespaceUri);
            actualDeliveryDate.InnerText = "2022-09-07"; 
            // Set the actual delivery date here
            // Append ActualDeliveryDate to Delivery
            delivery.AppendChild(actualDeliveryDate);
            // Append Delivery to root or desired parent element
            root.AppendChild(delivery);


           


            // payment means

            XmlElement paymentMeans = invoiceXml.CreateElement("cac", "PaymentMeans", cacNamespaceUri);
            XmlElement paymentMeansCode = invoiceXml.CreateElement("cbc", "PaymentMeansCode", cbcNamespaceUri);
            paymentMeansCode.InnerText = "10";  // Set the payment means code here
            // Append PaymentMeansCode to PaymentMeans
            paymentMeans.AppendChild(paymentMeansCode);
            // Append PaymentMeans to root or desired parent element
            root.AppendChild(paymentMeans);



            // Create AllowanceCharge element
            XmlElement allowanceCharge = invoiceXml.CreateElement("cac", "AllowanceCharge", cacNamespaceUri);

            // Create and set ChargeIndicator element
            XmlElement chargeIndicator = invoiceXml.CreateElement("cbc", "ChargeIndicator", cbcNamespaceUri);
            chargeIndicator.InnerText = "false";
            allowanceCharge.AppendChild(chargeIndicator);

            // Create and set AllowanceChargeReason element
            XmlElement allowanceChargeReason = invoiceXml.CreateElement("cbc", "AllowanceChargeReason", cbcNamespaceUri);
            allowanceChargeReason.InnerText = "discount";
            allowanceCharge.AppendChild(allowanceChargeReason);

            // Create and set Amount element with currencyID attribute
            XmlElement amount = invoiceXml.CreateElement("cbc", "Amount", cbcNamespaceUri);
            amount.SetAttribute("currencyID", "SAR");
            amount.InnerText = "0.00";
            allowanceCharge.AppendChild(amount);

            // Function to create TaxCategory element with nested elements
            XmlElement CreateTaxCategoryElement(XmlDocument xmlDoc, string id, string percent)
            {
                // Create TaxCategory element
                XmlElement taxCategory = xmlDoc.CreateElement("cac", "TaxCategory", cacNamespaceUri);

                // Create ID element with scheme attributes
                XmlElement taxCategoryID = xmlDoc.CreateElement("cbc", "ID", cbcNamespaceUri);
                taxCategoryID.SetAttribute("schemeID", "UN/ECE 5305");
                taxCategoryID.SetAttribute("schemeAgencyID", "6");
                taxCategoryID.InnerText = id;
                taxCategory.AppendChild(taxCategoryID);

                // Create Percent element
                XmlElement percentElem = xmlDoc.CreateElement("cbc", "Percent", cbcNamespaceUri);
                percentElem.InnerText = percent;
                taxCategory.AppendChild(percentElem);

                // Create TaxScheme element
                XmlElement taxScheme = xmlDoc.CreateElement("cac", "TaxScheme", cacNamespaceUri);

                // Create TaxScheme ID element with scheme attributes
                XmlElement taxSchemeID = xmlDoc.CreateElement("cbc", "ID", cbcNamespaceUri);
                taxSchemeID.SetAttribute("schemeID", "UN/ECE 5153");
                taxSchemeID.SetAttribute("schemeAgencyID", "6");
                taxSchemeID.InnerText = "VAT";
                taxScheme.AppendChild(taxSchemeID);

                // Append TaxScheme to TaxCategory
                taxCategory.AppendChild(taxScheme);

                return taxCategory;
            }

            // Add the first TaxCategory element
            allowanceCharge.AppendChild(CreateTaxCategoryElement(invoiceXml, "S", "15"));

            // Add the second TaxCategory element (repeating for demonstration)
            allowanceCharge.AppendChild(CreateTaxCategoryElement(invoiceXml, "S", "15"));

            // Append AllowanceCharge to the root or desired parent element
            root.AppendChild(allowanceCharge);





            // Create TaxTotal element


            XmlElement taxTotal1 = invoiceXml.CreateElement("cac", "TaxTotal", cacNamespaceUri);

            // Create TaxAmount element and set currencyID attribute
            XmlElement taxAmount1 = invoiceXml.CreateElement("cbc", "TaxAmount", cbcNamespaceUri);
            taxAmount1.SetAttribute("currencyID", "SAR");
            taxAmount1.InnerText = "30.15";
            taxTotal1.AppendChild(taxAmount1);

            // Append the first TaxTotal to the root or parent element
            root.AppendChild(taxTotal1);

            // Create second TaxTotal element with TaxSubtotal
            XmlElement taxTotal2 = invoiceXml.CreateElement("cac", "TaxTotal", cacNamespaceUri);

            // Create TaxAmount element for the second TaxTotal
            XmlElement taxAmount2 = invoiceXml.CreateElement("cbc", "TaxAmount", cbcNamespaceUri);
            taxAmount2.SetAttribute("currencyID", "SAR");
            taxAmount2.InnerText = "30.15";
            taxTotal2.AppendChild(taxAmount2);

            // Create TaxSubtotal element inside second TaxTotal
            XmlElement taxSubtotal = invoiceXml.CreateElement("cac", "TaxSubtotal", cacNamespaceUri);

            // Create TaxableAmount element
            XmlElement taxableAmount = invoiceXml.CreateElement("cbc", "TaxableAmount", cbcNamespaceUri);
            taxableAmount.SetAttribute("currencyID", "SAR");
            taxableAmount.InnerText = "201.00";
            taxSubtotal.AppendChild(taxableAmount);

            // Create TaxAmount element inside TaxSubtotal
            XmlElement taxAmountSubtotal = invoiceXml.CreateElement("cbc", "TaxAmount", cbcNamespaceUri);
            taxAmountSubtotal.SetAttribute("currencyID", "SAR");
            taxAmountSubtotal.InnerText = "30.15";
            taxSubtotal.AppendChild(taxAmountSubtotal);

            // Create TaxCategory element inside TaxSubtotal
            XmlElement taxCategory = invoiceXml.CreateElement("cac", "TaxCategory", cacNamespaceUri);

            // Create TaxCategory ID element with scheme attributes
            XmlElement taxCategoryID = invoiceXml.CreateElement("cbc", "ID", cbcNamespaceUri);
            taxCategoryID.SetAttribute("schemeID", "UN/ECE 5305");
            taxCategoryID.SetAttribute("schemeAgencyID", "6");
            taxCategoryID.InnerText = "S";
            taxCategory.AppendChild(taxCategoryID);

            // Create Percent element inside TaxCategory
            XmlElement percent = invoiceXml.CreateElement("cbc", "Percent", cbcNamespaceUri);
            percent.InnerText = "15.00";
            taxCategory.AppendChild(percent);

            // Create TaxScheme element inside TaxCategory
            XmlElement taxScheme2 = invoiceXml.CreateElement("cac", "TaxScheme", cacNamespaceUri);

            // Create TaxScheme ID element with scheme attributes
            XmlElement taxSchemeID = invoiceXml.CreateElement("cbc", "ID", cbcNamespaceUri);
            taxSchemeID.SetAttribute("schemeID", "UN/ECE 5153");
            taxSchemeID.SetAttribute("schemeAgencyID", "6");
            taxSchemeID.InnerText = "VAT";
            taxScheme2.AppendChild(taxSchemeID);

            // Append TaxScheme to TaxCategory
            taxCategory.AppendChild(taxScheme2);

            // Append TaxCategory to TaxSubtotal
            taxSubtotal.AppendChild(taxCategory);

            // Append TaxSubtotal to second TaxTotal
            taxTotal2.AppendChild(taxSubtotal);
            // Append the second TaxTotal to the root or parent element
            root.AppendChild(taxTotal2);







            //-------------------------------------------------------------------------
            //-------------------------------------------------------------------------

            // Create LegalMonetaryTotal element
            XmlElement legalMonetaryTotal = invoiceXml.CreateElement("cac", "LegalMonetaryTotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create LineExtensionAmount element and set currencyID attribute
            XmlElement lineExtensionAmount = invoiceXml.CreateElement("cbc", "LineExtensionAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            lineExtensionAmount.SetAttribute("currencyID", "SAR");
            lineExtensionAmount.InnerText = "201.00";
            legalMonetaryTotal.AppendChild(lineExtensionAmount);

            // Create TaxExclusiveAmount element
            XmlElement taxExclusiveAmount = invoiceXml.CreateElement("cbc", "TaxExclusiveAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxExclusiveAmount.SetAttribute("currencyID", "SAR");
            taxExclusiveAmount.InnerText = "201.00";
            legalMonetaryTotal.AppendChild(taxExclusiveAmount);

            // Create TaxInclusiveAmount element
            XmlElement taxInclusiveAmount = invoiceXml.CreateElement("cbc", "TaxInclusiveAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxInclusiveAmount.SetAttribute("currencyID", "SAR");
            taxInclusiveAmount.InnerText = "231.15";
            legalMonetaryTotal.AppendChild(taxInclusiveAmount);

            // Create AllowanceTotalAmount element
            XmlElement allowanceTotalAmount = invoiceXml.CreateElement("cbc", "AllowanceTotalAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            allowanceTotalAmount.SetAttribute("currencyID", "SAR");
            allowanceTotalAmount.InnerText = "0.00";
            legalMonetaryTotal.AppendChild(allowanceTotalAmount);

            // Create PrepaidAmount element
            XmlElement prepaidAmount = invoiceXml.CreateElement("cbc", "PrepaidAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            prepaidAmount.SetAttribute("currencyID", "SAR");
            prepaidAmount.InnerText = "0.00";
            legalMonetaryTotal.AppendChild(prepaidAmount);

            // Create PayableAmount element
            XmlElement payableAmount = invoiceXml.CreateElement("cbc", "PayableAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            payableAmount.SetAttribute("currencyID", "SAR");
            payableAmount.InnerText = "231.15";
            legalMonetaryTotal.AppendChild(payableAmount);

            // Append LegalMonetaryTotal to root or parent element
            root.AppendChild(legalMonetaryTotal);


            //--------------------------------invoice line--------
            // Create InvoiceLine element
            XmlElement invoiceLine = invoiceXml.CreateElement("cac", "InvoiceLine", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Rename 'id' to 'invoiceLineId' to avoid conflict
            XmlElement invoiceLineId = invoiceXml.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            invoiceLineId.InnerText = "1";
            invoiceLine.AppendChild(invoiceLineId);

            //qunatity 
            XmlElement InvoicedQuantityLine1 = invoiceXml.CreateElement("cbc", "InvoicedQuantity", cbcNamespaceUri);
            InvoicedQuantityLine1.SetAttribute("unitCode", "PCE");
            InvoicedQuantityLine1.InnerText = "33.000000";
            invoiceLine.AppendChild(InvoicedQuantityLine1);

            // Rename 'lineExtensionAmount' to 'lineExtensionAmountValue' to avoid conflict
            XmlElement lineExtensionAmountValue = invoiceXml.CreateElement("cbc", "LineExtensionAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            lineExtensionAmountValue.SetAttribute("currencyID", "SAR");
            lineExtensionAmountValue.InnerText = "99.00";
            invoiceLine.AppendChild(lineExtensionAmountValue);

            // Create TaxTotal element
            XmlElement taxTotal = invoiceXml.CreateElement("cac", "TaxTotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create TaxAmount element within TaxTotal
            XmlElement taxAmount = invoiceXml.CreateElement("cbc", "TaxAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxAmount.SetAttribute("currencyID", "SAR");
            taxAmount.InnerText = "14.85";
            taxTotal.AppendChild(taxAmount);

            // Create RoundingAmount element within TaxTotal
            XmlElement roundingAmount = invoiceXml.CreateElement("cbc", "RoundingAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            roundingAmount.SetAttribute("currencyID", "SAR");
            roundingAmount.InnerText = "113.85";
            taxTotal.AppendChild(roundingAmount);

            // Append TaxTotal to InvoiceLine
            invoiceLine.AppendChild(taxTotal);

            // Create Item element
            XmlElement item = invoiceXml.CreateElement("cac", "Item", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create Name element within Item
            XmlElement name = invoiceXml.CreateElement("cbc", "Name", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            name.InnerText = "كتاب";
            item.AppendChild(name);

            // Create ClassifiedTaxCategory element within Item
            XmlElement classifiedTaxCategoryNew = invoiceXml.CreateElement("cac", "ClassifiedTaxCategory", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create ID element within ClassifiedTaxCategory
            XmlElement taxCategoryIdNew = invoiceXml.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxCategoryIdNew.InnerText = "S";
            classifiedTaxCategoryNew.AppendChild(taxCategoryIdNew);

            // Create Percent element within ClassifiedTaxCategory
            XmlElement percentNew = invoiceXml.CreateElement("cbc", "Percent", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            percentNew.InnerText = "15.00";
            classifiedTaxCategoryNew.AppendChild(percentNew);

            // Create TaxScheme element within ClassifiedTaxCategory
            XmlElement taxSchemeNew = invoiceXml.CreateElement("cac", "TaxScheme", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create ID element within TaxScheme
            XmlElement taxSchemeIdNew = invoiceXml.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxSchemeIdNew.InnerText = "VAT";
            taxSchemeNew.AppendChild(taxSchemeIdNew);

            // Append TaxScheme to ClassifiedTaxCategory
            classifiedTaxCategoryNew.AppendChild(taxSchemeNew);

            // Append ClassifiedTaxCategory to Item
            item.AppendChild(classifiedTaxCategoryNew);

            // Append Item to InvoiceLine
            invoiceLine.AppendChild(item);

            // Create Price element
            XmlElement price = invoiceXml.CreateElement("cac", "Price", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create PriceAmount element within Price
            XmlElement priceAmount = invoiceXml.CreateElement("cbc", "PriceAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            priceAmount.SetAttribute("currencyID", "SAR");
            priceAmount.InnerText = "3.00";
            price.AppendChild(priceAmount);

            // Create AllowanceCharge element within Price
            XmlElement allowanceChargeNew = invoiceXml.CreateElement("cac", "AllowanceCharge", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create ChargeIndicator element within AllowanceCharge
            XmlElement chargeIndicatorNew = invoiceXml.CreateElement("cbc", "ChargeIndicator", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            chargeIndicatorNew.InnerText = "true";
            allowanceChargeNew.AppendChild(chargeIndicatorNew);

            // Create AllowanceChargeReason element within AllowanceCharge
            XmlElement allowanceChargeReasonNew = invoiceXml.CreateElement("cbc", "AllowanceChargeReason", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            allowanceChargeReasonNew.InnerText = "discount";
            allowanceChargeNew.AppendChild(allowanceChargeReasonNew);

            // Create Amount element within AllowanceCharge
            XmlElement amountNew = invoiceXml.CreateElement("cbc", "Amount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            amountNew.SetAttribute("currencyID", "SAR");
            amountNew.InnerText = "0.00";
            allowanceChargeNew.AppendChild(amountNew);

            // Append AllowanceCharge to Price
            price.AppendChild(allowanceChargeNew);

            // Append Price to InvoiceLine
            invoiceLine.AppendChild(price);

            // Append InvoiceLine to root or parent element
            root.AppendChild(invoiceLine);








            //-----------------------------------------------------
            //invice line 2
            //-----------------------------------------------------
            // Create second InvoiceLine element
            XmlElement invoiceLine2 = invoiceXml.CreateElement("cac", "InvoiceLine", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Rename 'id' to 'invoiceLineId2' to avoid conflict
            XmlElement invoiceLineId2 = invoiceXml.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            invoiceLineId2.InnerText = "2";
            invoiceLine2.AppendChild(invoiceLineId2);

            //quantity
            XmlElement InvoicedQuantityLine2 = invoiceXml.CreateElement("cbc", "InvoicedQuantity", cbcNamespaceUri);
            InvoicedQuantityLine2.SetAttribute("unitCode", "PCE");
            InvoicedQuantityLine2.InnerText = "3.000000";
            invoiceLine2.AppendChild(InvoicedQuantityLine2);




            // Rename 'lineExtensionAmount' to 'lineExtensionAmountValue2' to avoid conflict
            XmlElement lineExtensionAmountValue2 = invoiceXml.CreateElement("cbc", "LineExtensionAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            lineExtensionAmountValue2.SetAttribute("currencyID", "SAR");
            lineExtensionAmountValue2.InnerText = "102.00";
            invoiceLine2.AppendChild(lineExtensionAmountValue2);

            // Rename 'taxTotal2' to 'taxTotalValue2' to avoid conflict
            XmlElement taxTotalValue2 = invoiceXml.CreateElement("cac", "TaxTotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create TaxAmount element within TaxTotal for second InvoiceLine
            XmlElement taxAmountValue2 = invoiceXml.CreateElement("cbc", "TaxAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxAmountValue2.SetAttribute("currencyID", "SAR");
            taxAmountValue2.InnerText = "15.30";
            taxTotalValue2.AppendChild(taxAmountValue2);

            // Create RoundingAmount element within TaxTotal for second InvoiceLine
            XmlElement roundingAmountValue2 = invoiceXml.CreateElement("cbc", "RoundingAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            roundingAmountValue2.SetAttribute("currencyID", "SAR");
            roundingAmountValue2.InnerText = "117.30";
            taxTotalValue2.AppendChild(roundingAmountValue2);

            // Append TaxTotal to InvoiceLine2
            invoiceLine2.AppendChild(taxTotalValue2);

            // Create Item element for second InvoiceLine
            XmlElement item2 = invoiceXml.CreateElement("cac", "Item", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create Name element within Item for second InvoiceLine
            XmlElement nameValue2 = invoiceXml.CreateElement("cbc", "Name", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nameValue2.InnerText = "قلم";
            item2.AppendChild(nameValue2);

            // Create ClassifiedTaxCategory element within Item for second InvoiceLine
            XmlElement classifiedTaxCategoryValue2 = invoiceXml.CreateElement("cac", "ClassifiedTaxCategory", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create ID element within ClassifiedTaxCategory for second InvoiceLine
            XmlElement taxCategoryIdValue2 = invoiceXml.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxCategoryIdValue2.InnerText = "S";
            classifiedTaxCategoryValue2.AppendChild(taxCategoryIdValue2);

            // Create Percent element within ClassifiedTaxCategory for second InvoiceLine
            XmlElement percentValue2 = invoiceXml.CreateElement("cbc", "Percent", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            percentValue2.InnerText = "15.00";
            classifiedTaxCategoryValue2.AppendChild(percentValue2);

            // Create TaxScheme element within ClassifiedTaxCategory for second InvoiceLine
            XmlElement taxSchemeValue2 = invoiceXml.CreateElement("cac", "TaxScheme", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create ID element within TaxScheme for second InvoiceLine
            XmlElement taxSchemeIdValue2 = invoiceXml.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            taxSchemeIdValue2.InnerText = "VAT";
            taxSchemeValue2.AppendChild(taxSchemeIdValue2);

            // Append TaxScheme to ClassifiedTaxCategory
            classifiedTaxCategoryValue2.AppendChild(taxSchemeValue2);

            // Append ClassifiedTaxCategory to Item
            item2.AppendChild(classifiedTaxCategoryValue2);

            // Append Item to InvoiceLine2
            invoiceLine2.AppendChild(item2);

            // Create Price element for second InvoiceLine
            XmlElement price2 = invoiceXml.CreateElement("cac", "Price", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create PriceAmount element within Price for second InvoiceLine
            XmlElement priceAmountValue2 = invoiceXml.CreateElement("cbc", "PriceAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            priceAmountValue2.SetAttribute("currencyID", "SAR");
            priceAmountValue2.InnerText = "34.00";
            price2.AppendChild(priceAmountValue2);

            // Create AllowanceCharge element within Price for second InvoiceLine
            XmlElement allowanceChargeValue2 = invoiceXml.CreateElement("cac", "AllowanceCharge", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

            // Create ChargeIndicator element within AllowanceCharge for second InvoiceLine
            XmlElement chargeIndicatorValue2 = invoiceXml.CreateElement("cbc", "ChargeIndicator", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            chargeIndicatorValue2.InnerText = "true";
            allowanceChargeValue2.AppendChild(chargeIndicatorValue2);

            // Create AllowanceChargeReason element within AllowanceCharge for second InvoiceLine
            XmlElement allowanceChargeReasonValue2 = invoiceXml.CreateElement("cbc", "AllowanceChargeReason", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            allowanceChargeReasonValue2.InnerText = "discount";
            allowanceChargeValue2.AppendChild(allowanceChargeReasonValue2);

            // Create Amount element within AllowanceCharge for second InvoiceLine
            XmlElement amountValue2 = invoiceXml.CreateElement("cbc", "Amount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            amountValue2.SetAttribute("currencyID", "SAR");
            amountValue2.InnerText = "0.00";
            allowanceChargeValue2.AppendChild(amountValue2);

            // Append AllowanceCharge to Price
            price2.AppendChild(allowanceChargeValue2);

            // Append Price to InvoiceLine2
            invoiceLine2.AppendChild(price2);

            // Append InvoiceLine2 to root or parent element
            root.AppendChild(invoiceLine2);





            //invoiceXml.Save(outputPath);
            //Console.WriteLine("XML file saved successfully.");
            //Console.ReadLine();
            //Console.ReadLine();

            try
            {
                invoiceXml.Save(outputPath);
                Console.WriteLine("XML file saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving XML file: " + ex.Message);
            }

            Console.WriteLine("\nGenerated XML content:");
            Console.WriteLine(invoiceXml.OuterXml);
        }

        private XmlElement CreatePartyElement(XmlDocument doc, dynamic partyData)
        {
            XmlElement party = doc.CreateElement("cac:Party");
            if (partyData != null)
            {
                XmlElement partyName = doc.CreateElement("cac:PartyName");
                AppendChildElement(doc, partyName, "cbc:Name", partyData.PartyName?.Name, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                party.AppendChild(partyName);
            }
            return party;
        }





        private void AppendChildElement(XmlDocument doc, XmlElement parent, string elementName, string value, string namespaceUri)
        {
            if (!string.IsNullOrEmpty(value))
            {
                XmlElement childElement = doc.CreateElement(elementName, namespaceUri);
                childElement.InnerText = value;

                parent.AppendChild(childElement);
            }
        }   
    }
}
