using Agenda.Application.Common.Models;
using Agenda.Application.Contacts;
using Agenda.Application.Contacts.Commands.CreateContact;
using Agenda.Application.Contacts.Commands.DeleteContact;
using Agenda.Application.Contacts.Commands.UpdateContact;
using Agenda.Application.Contacts.Queries.GetContactById;
using Agenda.Application.Contacts.Queries.GetContacts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agenda.Api.Controllers;

[ApiController]
[Route("api/contacts")]
[Produces("application/json")]
[Authorize]
public sealed class ContactsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetContactsQuery(search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetContactByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContact(
        [FromBody] CreateContactCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetContactById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateContact(
        Guid id,
        [FromBody] UpdateContactCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteContactCommand(id), cancellationToken);
        return NoContent();
    }
}
