namespace NotificationService.Application.Interfaces;

public interface IEmailNotifier
{
    Task SendEmailAsync(string to, string subject, string body);
}