using System.Collections.Generic;

public class Invoice
{
    public string ProfileID { get; set; }
    public string ID { get; set; }
    public string UUID { get; set; }
    public string IssueDate { get; set; }
    public string IssueTime { get; set; }
    public string InvoiceTypeCode { get; set; }
    public string Note { get; set; }
    public string DocumentCurrencyCode { get; set; }
    public string TaxCurrencyCode { get; set; }
    public BillingReference BillingReference { get; set; }
    public Party AccountingSupplierParty { get; set; }
    public Party AccountingCustomerParty { get; set; }
    public List<InvoiceLine> InvoiceLines { get; set; }
}

public class BillingReference
{
    public string InvoiceDocumentReferenceID { get; set; }
}

public class Party
{
    public string ID { get; set; }
    public string StreetName { get; set; }
    public string BuildingNumber { get; set; }
    public string CityName { get; set; }
    public string PostalZone { get; set; }
    public string CountryCode { get; set; }
}

public class InvoiceLine
{
    public string ID { get; set; }
    public string InvoicedQuantity { get; set; }
    public string PriceAmount { get; set; }
    public string ItemName { get; set; }
    public string TaxPercent { get; set; }
}
