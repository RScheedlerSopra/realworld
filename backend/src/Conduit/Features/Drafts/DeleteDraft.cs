using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Drafts;

public class DeleteDraft
{
    public record Command(int Id) : IRequest;

    public class Handler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command message, CancellationToken cancellationToken)
        {
            var article = await context
                .Articles.Include(x => x.Author)
                .Where(x => x.ArticleId == message.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (article == null || !article.IsDraft)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Draft = Constants.NOT_FOUND }
                );
            }

            // Verify current user is the author
            if (article.Author?.Username != currentUserAccessor.GetCurrentUsername())
            {
                throw new RestException(
                    HttpStatusCode.Forbidden,
                    new { Draft = "You are not authorized to delete this draft" }
                );
            }

            context.Articles.Remove(article);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
