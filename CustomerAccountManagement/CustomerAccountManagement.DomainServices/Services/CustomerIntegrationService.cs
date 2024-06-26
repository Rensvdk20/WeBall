using System.Globalization;
using CsvHelper;
using CustomerAccountManagement.Domain.Entities;
using CustomerAccountManagement.DomainServices.Interfaces;
using Events;
using MassTransit;

namespace CustomerAccountManagement.DomainServices.Services;

public class CustomerIntegrationService(IPublishEndpoint bus) : ICustomerIntegration
{
    public async Task ImportExternalCustomers()
    {
        const string url =
            "https://marcavans.blob.core.windows.net/solarch/fake_customer_data_export.csv?sv=2023-01-03&st=2024-06-14T10%3A31%3A07Z&se=2032-06-15T10%3A31%3A00Z&sr=b&sp=r&sig=q4Ie3kKpguMakW6sbcKl0KAWutzpMi747O4yIr8lQLI%3D";

        var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(url);

        using var reader = new StringReader(response);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<CustomerCSVDataMap>();
        var records = csv.GetRecords<CustomerCSVData>();

        foreach (var record in records)
        {
            var customer = new Customer
            {
                Name = $"{record.FirstName} {record.LastName}",
                Email = $"{record.FirstName.ToLower()}.{record.LastName.ToLower()}@example.com",
                Street = ParseStreetFromAddress(record.Address),
                City = ParseCityFromAddress(record.Address),
                ZipCode = ParseZipCodeFromAddress(record.Address)
            };

            var externalCustomerCreated = new ExternalCustomerCreated
            {
                Name = customer.Name,
                Email = customer.Email,
                Street = customer.Street,
                City = customer.City,
                ZipCode = customer.ZipCode
            };

            await bus.Publish(externalCustomerCreated);
        }
    }

    private string ParseStreetFromAddress(string address)
    {
        var parts = address.Split(',');
        return parts[0].Trim();
    }

    private string ParseCityFromAddress(string address)
    {
        var parts = address.Split(',');
        return parts[1].Trim().Split(' ')[1].Trim();
    }

    private string ParseZipCodeFromAddress(string address)
    {
        var parts = address.Split(',');
        return parts[1].Trim().Split(' ')[0].Trim();
    }
}