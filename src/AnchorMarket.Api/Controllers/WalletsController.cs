using System;
using AnchorMarket.Application.Features.Wallets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AnchorMarket.Api.Controllers;

/// <summary>Manages user wallets and transactions.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController(ISender sender) : ControllerBase
{
    /// <summary>Retrieves a wallet by user ID.</summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The wallet if found; otherwise 404.</returns>
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var wallet = await sender.Send(new GetWalletQuery(userId, callerId), cancellationToken);
        return wallet is null ? NotFound() : Ok(wallet);
    }

    /// <summary>Retrieves wallet transactions for a user.</summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of transactions.</returns>
    [HttpGet("user/{userId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid userId, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await sender.Send(new GetWalletTransactionsQuery(userId, callerId), cancellationToken));
    }
}
