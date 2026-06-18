using AnchorMarket.Application.Features.Wallets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnchorMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController(ISender sender) : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var wallet = await sender.Send(new GetWalletQuery(userId, callerId), cancellationToken);
        return wallet is null ? NotFound() : Ok(wallet);
    }

    [HttpGet("user/{userId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid userId, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await sender.Send(new GetWalletTransactionsQuery(userId, callerId), cancellationToken));
    }
}
