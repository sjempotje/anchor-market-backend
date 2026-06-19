using System.Net;
using System.Net.Http.Json;
using AnchorMarket.Application.Features.Notifications.DTOs;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class NotificationTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    private async Task SeedNotification(Guid userId, string title, string body = "")
    {
        var db = Factory.CreateDbContext();
        var notification = Notification.Create(userId, NotificationType.MarketResolved, title, body);
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetNotifications_ReturnsListForUser()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"notif_{suffix}", $"notif_{suffix}@example.com");
        await SeedNotification(userId, "Market resolved", "Your market was resolved.");

        var response = await Client.GetAsync("/api/notifications");
        response.EnsureSuccessStatusCode();

        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
        Assert.NotNull(notifications);
        Assert.NotEmpty(notifications);
    }

    [Fact]
    public async Task GetNotifications_UnreadOnly_ReturnsOnlyUnread()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"notifur_{suffix}", $"notifur_{suffix}@example.com");
        await SeedNotification(userId, "Unread notification", "Body");

        var response = await Client.GetAsync("/api/notifications?unreadOnly=true");
        response.EnsureSuccessStatusCode();

        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
        Assert.NotNull(notifications);
        Assert.All(notifications, n => Assert.False(n.IsRead));
    }

    [Fact]
    public async Task MarkNotificationRead_ReturnsNoContent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var userId = await RegisterUser($"notifrd_{suffix}", $"notifrd_{suffix}@example.com");
        await SeedNotification(userId, "Mark me read", "Body");

        var listResponse = await Client.GetAsync("/api/notifications");
        var notifications = await listResponse.Content.ReadFromJsonAsync<List<NotificationDto>>();
        var notification = Assert.Single(notifications!);

        var response = await Client.PutAsync($"/api/notifications/{notification.Id}/read", null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var afterResponse = await Client.GetAsync("/api/notifications?unreadOnly=true");
        var unread = await afterResponse.Content.ReadFromJsonAsync<List<NotificationDto>>();
        Assert.NotNull(unread);
        Assert.Empty(unread);
    }
}
