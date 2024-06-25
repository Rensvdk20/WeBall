﻿namespace CustomerAccountManagement.Domain.Events;

public class CustomerCreated
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}