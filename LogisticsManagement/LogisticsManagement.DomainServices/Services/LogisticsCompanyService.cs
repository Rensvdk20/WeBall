using Events;
using LogisticsManagement.Domain.Entities;
using LogisticsManagement.DomainServices.Interfaces;
using MassTransit;

namespace LogisticsManagement.DomainServices.Services;

public class LogisticsCompanyService(IRepository<LogisticsCompany> repo, IEventStore eventStore, IBusControl serviceBus)
    : ILcManagement
{
    public async Task<LogisticsCompany?> GetLogisticsCompanyByIdAsync(Guid id)
    {
        var events = await eventStore.ReadAsync<LogisticsCompanyEvent>(id);
        if (events.Count == 0)
        {
            return null;
        }

        var logisticsCompany = new LogisticsCompany();
        foreach (var @event in events)
        {
            logisticsCompany.Apply(@event);
        }

        var @syncEvent = new LogisticsCompanySync()
        {
            LogisticsCompanyId = id,
            Name = logisticsCompany.Name!,
            ShippingRate = logisticsCompany.ShippingRate,
            CreatedAtUtc = DateTime.UtcNow
        };
        await serviceBus.Publish(syncEvent);

        return logisticsCompany;
    }

    public async Task<List<LogisticsCompany>> GetLogisticsCompaniesAsync()
    {
        return await repo.GetAllAsync();
    }

    public async Task<LogisticsCompany> CreateLogisticsCompanyAsync(LogisticsCompany logisticsCompany)
    {
        var @event = new LogisticsCompanyCreated()
        {
            LogisticsCompanyId = Guid.NewGuid(),
            Name = logisticsCompany.Name,
            ShippingRate = logisticsCompany.ShippingRate,
            CreatedAtUtc = DateTime.UtcNow,
        };
        await eventStore.AppendAsync(@event);
        await serviceBus.Publish(@event);

        logisticsCompany.Id = @event.LogisticsCompanyId;

        return logisticsCompany;
    }

    public async Task<LogisticsCompany> UpdateLogisticsCompanyAsync(Guid id, LogisticsCompany logisticsCompany)
    {
        var @event = new LogisticsCompanyUpdated()
        {
            LogisticsCompanyId = id,
            ShippingRate = logisticsCompany.ShippingRate,
            CreatedAtUtc = DateTime.UtcNow
        };
        await eventStore.AppendAsync(@event);
        await serviceBus.Publish(@event);

        return (await GetLogisticsCompanyByIdAsync(id))!;
    }

    public async Task DeleteLogisticsCompanyAsync(Guid id)
    {
        var @event = new LogisticsCompanyDeleted()
        {
            LogisticsCompanyId = id,
            CreatedAtUtc = DateTime.UtcNow
        };
        await eventStore.AppendAsync(@event);
        await serviceBus.Publish(@event);
    }
}