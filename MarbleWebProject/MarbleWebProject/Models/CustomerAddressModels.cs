namespace MarbleWebProject.Models;



public sealed class CustomerAddressForm

{

    public int? Id { get; set; }

    public string? Label { get; set; }

    public string? ContactFirstName { get; set; }

    public string? ContactLastName { get; set; }

    public string? ContactPhone { get; set; }

    public bool IsDefaultBilling { get; set; }

    public bool IsDefaultShipping { get; set; }

    public long CountryId { get; set; }

    public long? CityId { get; set; }

    public long? TownId { get; set; }

    public string AddressLine1 { get; set; } = "";

    public string? AddressLine2 { get; set; }

    public string? BuildingNo { get; set; }

    public string? ApartmentNo { get; set; }

    public string? PostalCode { get; set; }

    public string? DeliveryInstructions { get; set; }

    public string InvoiceType { get; set; } = "Individual";

    public string? TaxNumber { get; set; }

    public string? TaxOffice { get; set; }

    public string? CompanyName { get; set; }

    public bool IsEInvoice { get; set; }

}



public sealed class CustomerAddressModel

{

    public int Id { get; set; }

    public long AddressId { get; set; }

    public string? Label { get; set; }

    public string? ContactFirstName { get; set; }

    public string? ContactLastName { get; set; }

    public string? ContactPhone { get; set; }

    public bool IsDefaultBilling { get; set; }

    public bool IsDefaultShipping { get; set; }

    public long CountryId { get; set; }

    public string? CountryName { get; set; }

    public long? CityId { get; set; }

    public string? CityName { get; set; }

    public long? TownId { get; set; }

    public string? TownName { get; set; }

    public string AddressLine1 { get; set; } = "";

    public string? AddressLine2 { get; set; }

    public string? BuildingNo { get; set; }

    public string? ApartmentNo { get; set; }

    public string? PostalCode { get; set; }

    public string? DeliveryInstructions { get; set; }

    public string DisplayLine { get; set; } = "";

    public string InvoiceType { get; set; } = "Individual";

    public string? TaxNumber { get; set; }

    public string? TaxOffice { get; set; }

    public string? CompanyName { get; set; }

    public bool IsEInvoice { get; set; }

}



public sealed class LocationLookupItemModel

{

    public long Id { get; set; }

    public string Label { get; set; } = "";

}



public sealed class CustomerProfileUpdateForm

{

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? CompanyName { get; set; }

    public string? TaxOffice { get; set; }

    public string? TaxNumber { get; set; }

    public DateTime? BirthDate { get; set; }

}

