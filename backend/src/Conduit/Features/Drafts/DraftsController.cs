using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Articles;
using Conduit.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Features.Drafts;

[Route("drafts")]
[Authorize(AuthenticationSchemes = JwtIssuerOptions.Schemes)]
public class DraftsController(IMediator mediator) : Controller
{
    [HttpPost]
    public Task<DraftEnvelope> Create(
        [FromBody] CreateDraft.Command command,
        CancellationToken cancellationToken
    ) => mediator.Send(command, cancellationToken);

    [HttpGet]
    public Task<DraftsEnvelope> List(
        [FromQuery] int? limit,
        [FromQuery] int? offset,
        CancellationToken cancellationToken
    ) => mediator.Send(new ListDrafts.Query(limit, offset), cancellationToken);

    [HttpGet("{id:int}")]
    public Task<DraftEnvelope> Get(int id, CancellationToken cancellationToken) =>
        mediator.Send(new DraftDetails.Query(id), cancellationToken);

    [HttpPut("{id:int}")]
    public Task<DraftEnvelope> Edit(
        int id,
        [FromBody] EditDraft.Model model,
        CancellationToken cancellationToken
    ) => mediator.Send(new EditDraft.Command(model, id), cancellationToken);

    [HttpPut("{id:int}/publish")]
    public Task<ArticleEnvelope> Publish(int id, CancellationToken cancellationToken) =>
        mediator.Send(new PublishDraft.Command(id), cancellationToken);

    [HttpDelete("{id:int}")]
    public Task Delete(int id, CancellationToken cancellationToken) =>
        mediator.Send(new DeleteDraft.Command(id), cancellationToken);
}
