using System.Net.WebSockets;
using System.Text.Json;
using Xunit;

namespace AnchorMarket.IntegrationTests;

[Collection("IntegrationTests")]
public class GroupMarketWebSocketTests(CustomWebApplicationFactory factory) : TestBase(factory)
{
    private async Task<string> SubscribeAndReadType(Guid asUser, object subscription)
    {
        TestAuthHandler.CurrentUserId = asUser;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var wsClient = Factory.Server.CreateWebSocketClient();
        var ws = await wsClient.ConnectAsync(new Uri(Factory.Server.BaseAddress, "ws"), cts.Token);
        try
        {
            await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(subscription), WebSocketMessageType.Text, true, cts.Token);

            var buffer = new byte[8192];
            var result = await ws.ReceiveAsync(buffer, cts.Token);
            using var doc = JsonDocument.Parse(buffer.AsMemory(0, result.Count));
            return doc.RootElement.GetProperty("type").GetString()!;
        }
        finally
        {
            try
            {
                if (ws.State == WebSocketState.Open)
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            catch (WebSocketException) { /* server already closing */ }
            catch (IOException) { /* server already closing */ }
        }
    }

    [Fact]
    public async Task Subscribe_PublicMarketPrice_IsAllowed()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var creator = await RegisterUser($"wsp_{suffix}", $"wsp_{suffix}@x.com");
        var marketId = await CreateMarket($"Price Movement {suffix}", "Up or down?", creator, ["UP", "DOWN"]);
        var upId = (await GetOutcomes(marketId))["UP"];

        var type = await SubscribeAndReadType(creator, new { action = "subscribe", channel = "price", outcomeId = upId });

        Assert.Equal("subscribed", type);
    }

    [Fact]
    public async Task Subscribe_GroupMarketPrice_AsMember_IsAllowed()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var owner = await RegisterUser($"wsm_o_{suffix}", $"wsm_o_{suffix}@x.com");
        var member = await RegisterUser($"wsm_m_{suffix}", $"wsm_m_{suffix}@x.com");
        var groupId = await CreateGroup($"Grp {suffix}", "g", owner);
        await AddGroupMembership(owner, groupId);
        await AddGroupMembership(member, groupId);
        var marketId = await CreateGroupMarket(groupId, owner, $"Grp Market {suffix}", "g", ["UP", "DOWN"], member);
        var upId = (await GetOutcomes(marketId))["UP"];

        var type = await SubscribeAndReadType(member, new { action = "subscribe", channel = "price", outcomeId = upId });

        Assert.Equal("subscribed", type);
    }

    [Fact]
    public async Task Subscribe_GroupMarketPrice_AsNonMember_IsRejected()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var owner = await RegisterUser($"wsn_o_{suffix}", $"wsn_o_{suffix}@x.com");
        var outsider = await RegisterUser($"wsn_x_{suffix}", $"wsn_x_{suffix}@x.com");
        var groupId = await CreateGroup($"Grp {suffix}", "g", owner);
        await AddGroupMembership(owner, groupId);
        var resolverId = await RegisterUser($"wsn_r_{suffix}", $"wsn_r_{suffix}@x.com");
        await AddGroupMembership(resolverId, groupId);
        TestAuthHandler.CurrentUserId = owner; // the group market must be created by a member
        var marketId = await CreateGroupMarket(groupId, owner, $"Grp Market {suffix}", "g", ["UP", "DOWN"], resolverId);
        var upId = (await GetOutcomes(marketId))["UP"];

        // The outsider is not a group member, so the private group market's price stream is denied.
        var type = await SubscribeAndReadType(outsider, new { action = "subscribe", channel = "price", outcomeId = upId });

        Assert.Equal("error", type);
    }
}
