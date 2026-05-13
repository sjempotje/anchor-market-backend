using AnchorMarket.Application.Features.Groups.Commands;
using AnchorMarket.Application.Features.Groups.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnchorMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly ISender _sender;

    public GroupsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var groups = await _sender.Send(new GetGroupsQuery(), cancellationToken);
        return Ok(groups);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var group = await _sender.Send(new GetGroupByIdQuery(id), cancellationToken);
        return group is null ? NotFound() : Ok(group);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupCommand command, CancellationToken cancellationToken)
    {
        if (id != command.GroupId) return BadRequest();
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteGroupCommand(id), cancellationToken);
        return NoContent();
    }
}
