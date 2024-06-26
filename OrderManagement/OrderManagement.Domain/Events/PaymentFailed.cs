using OrderManagement.Domain;

namespace Events;

public record PaymentFailed()
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public PaymentStatus Status { get; init; }
};