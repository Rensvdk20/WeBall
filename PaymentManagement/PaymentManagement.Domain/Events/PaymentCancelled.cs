using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PaymentManagement.Domain.Entities;

namespace Events;

public record PaymentCancelled() : PaymentEvent
{
    public Guid PaymentId { get; init; }

    [BsonRepresentation(BsonType.String)] public PaymentStatus Status { get; init; }
    public override Guid StreamId => PaymentId;
}