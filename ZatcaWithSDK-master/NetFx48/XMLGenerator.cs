using System;
using System.Linq;
using System.Xml.Linq;

public class XMLGenerator
{
    public XDocument GenerateInvoiceXML(Invoice invoice)
    {
        XNamespace ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        XDocument xml = new XDocument(
            new XElement(ns + "Invoice",
                new XElement(cbc + "ProfileID", invoice.ProfileID),
                new XElement(cbc + "ID", invoice.ID),
                new XElement(cbc + "UUID", invoice.UUID),
                new XElement(cbc + "IssueDate", invoice.IssueDate),
                new XElement(cbc + "IssueTime", invoice.IssueTime),
                new XElement(cbc + "InvoiceTypeCode", invoice.InvoiceTypeCode),
                new XElement(cbc + "Note", invoice.Note),
                new XElement(cbc + "DocumentCurrencyCode", invoice.DocumentCurrencyCode),
                new XElement(cbc + "TaxCurrencyCode", invoice.TaxCurrencyCode),
                new XElement(cac + "BillingReference",
                    new XElement(cac + "InvoiceDocumentReference",
                        new XElement(cbc + "ID", invoice.BillingReference.InvoiceDocumentReferenceID)
                    )
                ),
                new XElement(cac + "AccountingSupplierParty",
                    new XElement(cac + "Party",
                        new XElement(cac + "PartyIdentification",
                            new XElement(cbc + "ID", invoice.AccountingSupplierParty.ID)
                        ),
                        new XElement(cac + "PostalAddress",
                            new XElement(cbc + "StreetName", invoice.AccountingSupplierParty.StreetName),
                            new XElement(cbc + "BuildingNumber", invoice.AccountingSupplierParty.BuildingNumber),
                            new XElement(cbc + "CityName", invoice.AccountingSupplierParty.CityName),
                            new XElement(cbc + "PostalZone", invoice.AccountingSupplierParty.PostalZone),
                            new XElement(cac + "Country",
                                new XElement(cbc + "IdentificationCode", invoice.AccountingSupplierParty.CountryCode)
                            )
                        )
                    )
                ),
                new XElement(cac + "AccountingCustomerParty",
                    new XElement(cac + "Party",
                        new XElement(cac + "PartyIdentification",
                            new XElement(cbc + "ID", invoice.AccountingCustomerParty.ID)
                        ),
                        new XElement(cac + "PostalAddress",
                            new XElement(cbc + "StreetName", invoice.AccountingCustomerParty.StreetName),
                            new XElement(cbc + "BuildingNumber", invoice.AccountingCustomerParty.BuildingNumber),
                            new XElement(cbc + "CityName", invoice.AccountingCustomerParty.CityName),
                            new XElement(cbc + "PostalZone", invoice.AccountingCustomerParty.PostalZone),
                            new XElement(cac + "Country",
                                new XElement(cbc + "IdentificationCode", invoice.AccountingCustomerParty.CountryCode)
                            )
                        )
                    )
                ),
                new XElement(cac + "InvoiceLine",
                    from line in invoice.InvoiceLines
                    select new XElement(cac + "InvoiceLine",
                        new XElement(cbc + "ID", line.ID),
                        new XElement(cbc + "InvoicedQuantity", line.InvoicedQuantity),
                        new XElement(cac + "Item",
                            new XElement(cbc + "Name", line.ItemName)
                        ),
                        new XElement(cac + "Price",
                            new XElement(cbc + "PriceAmount", line.PriceAmount)
                        )
                    )
                )
            )
        );

        return xml;
    }
}
