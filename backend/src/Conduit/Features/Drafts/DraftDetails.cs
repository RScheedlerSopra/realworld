using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Drafts;

public class DraftDetails
{
    public record Query(int Id) : IRequest<DraftEnvelope>;

    public class QueryHandler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, DraftEnvelope>
    {
        public async Task<DraftEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var article = await context
                .Articles.Include(x => x.Author)
                .Include(x => x.ArticleFavorites)
                .Include(x => x.ArticleTags)
                .Where(x => x.ArticleId == message.Id)
                .AsNoTracking()
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
                    new { Draft = "You are not authorized to access this draft" }
                );
            }

            return new DraftEnvelope(article);
        }
    }
}
