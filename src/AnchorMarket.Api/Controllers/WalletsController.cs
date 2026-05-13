using AnchorMarket.Application.Features.Wallets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly ISender _sender;

    public WalletsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var wallet = await _sender.Send(new GetWalletQuery(id), cancellationToken);
        return wallet is null ? NotFound() : Ok(wallet);
    }

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id, CancellationToken cancellationToken)
    {
        var transactions = await _sender.Send(new GetWalletTransactionsQuery(id), cancellationToken);
        return Ok(transactions);
    }
}
