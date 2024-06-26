using Events;

namespace PaymentManagement.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Order Order { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Apply(PaymentEvent @event)
    {
        switch (@event)
        {
            case PaymentCreated created:
                Apply(created);
                break;
            case PaymentFailed cancelled:
                Apply(cancelled);
                break;
            case PaymentPaid paid:
                Apply(paid);
                break;
        }
    }

    private void Apply(PaymentCreated created)
    {
        Id = created.PaymentId;
        Amount = created.Amount;
        Status = created.Status;
        Order = created.Order;
        CreatedAt = created.CreatedAtUtc;
    }

    private void Apply(PaymentFailed failed)
    {
        Status = failed.Status;
    }

    private void Apply(PaymentPaid paid)
    {
        Status = paid.Status;
    }
}