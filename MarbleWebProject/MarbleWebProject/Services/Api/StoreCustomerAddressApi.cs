using MarbleWebProject.Models;



namespace MarbleWebProject.Services.Api;



public sealed class StoreCustomerAddressApi : IStoreCustomerAddressApi

{

    private readonly IStoreApiClient _api;

    private readonly IStoreCustomerSession _session;



    public StoreCustomerAddressApi(IStoreApiClient api, IStoreCustomerSession session)

    {

        _api = api;

        _session = session;

    }



    public async Task<BaseResponse<List<CustomerAddressModel>>> ListAsync(string? languageCode = null, CancellationToken cancellationToken = default)

    {

        var token = await _session.GetTokenAsync(cancellationToken);

        var query = string.IsNullOrWhiteSpace(languageCode) ? "" : $"?languageCode={Uri.EscapeDataString(languageCode)}";

        return await _api.GetAsync<BaseResponse<List<CustomerAddressModel>>>($"/api/customers/addresses{query}", token, cancellationToken);

    }



    public async Task<BaseResponse<CustomerAddressModel>> GetAsync(int id, string? languageCode = null, CancellationToken cancellationToken = default)

    {

        var token = await _session.GetTokenAsync(cancellationToken);

        var query = string.IsNullOrWhiteSpace(languageCode) ? "" : $"?languageCode={Uri.EscapeDataString(languageCode)}";

        return await _api.GetAsync<BaseResponse<CustomerAddressModel>>($"/api/customers/addresses/{id}{query}", token, cancellationToken);

    }



    public async Task<BaseResponse<CustomerAddressModel>> CreateAsync(CustomerAddressForm form, CancellationToken cancellationToken = default)

    {

        var token = await _session.GetTokenAsync(cancellationToken);

        return await _api.PostAsync<BaseResponse<CustomerAddressModel>>("/api/customers/addresses", MapBody(form), token, cancellationToken);

    }



    public async Task<BaseResponse<CustomerAddressModel>> UpdateAsync(int id, CustomerAddressForm form, CancellationToken cancellationToken = default)

    {

        var token = await _session.GetTokenAsync(cancellationToken);

        return await _api.PutAsync<BaseResponse<CustomerAddressModel>>($"/api/customers/addresses/{id}", MapBody(form), token, cancellationToken);

    }



    public async Task<BaseResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)

    {

        var token = await _session.GetTokenAsync(cancellationToken);

        return await _api.DeleteAsync<BaseResponse>($"/api/customers/addresses/{id}", token, cancellationToken);

    }



    public async Task<BaseResponse<CustomerAddressModel>> SetDefaultAsync(int id, string type, CancellationToken cancellationToken = default)

    {

        var token = await _session.GetTokenAsync(cancellationToken);

        return await _api.PostAsync<BaseResponse<CustomerAddressModel>>(

            $"/api/customers/addresses/{id}/set-default",

            new { type },

            token,

            cancellationToken);

    }



    private static object MapBody(CustomerAddressForm form) => new

    {

        label = form.Label,

        contactFirstName = form.ContactFirstName,

        contactLastName = form.ContactLastName,

        contactPhone = form.ContactPhone,

        isDefaultBilling = form.IsDefaultBilling,

        isDefaultShipping = form.IsDefaultShipping,

        countryId = form.CountryId,

        cityId = form.CityId,

        townId = form.TownId,

        addressLine1 = form.AddressLine1,

        addressLine2 = form.AddressLine2,

        buildingNo = form.BuildingNo,

        apartmentNo = form.ApartmentNo,

        postalCode = form.PostalCode,

        deliveryInstructions = form.DeliveryInstructions,

        invoiceType = form.InvoiceType,

        taxNumber = form.TaxNumber,

        taxOffice = form.TaxOffice,

        companyName = form.CompanyName,

        isEInvoice = form.IsEInvoice

    };

}

