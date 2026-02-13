using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Comments;

public class List
{
    public record Query(string Slug) : IRequest<CommentsEnvelope>;

    public class QueryHandler(ConduitContext context, ICurrentUserAccessor currentUserAccessor) : IRequestHandler<Query, CommentsEnvelope>
    {
        public async Task<CommentsEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var article = await context
                .Articles
                .Include(x => x.Comments)
                .ThenInclude(x => x.Author)
                .Include(x => x.Author)
                .FirstOrDefaultAsync(x => x.Slug == message.Slug, cancellationToken);

            if (article == null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            // Prevent listing comments on draft articles unless user is the author
            if (article.IsDraft)
            {
                var currentUsername = currentUserAccessor.GetCurrentUsername();
                if (article.Author?.Username != currentUsername)
                {
                    throw new RestException(
                        HttpStatusCode.Forbidden,
                        new { Article = "Cannot view comments on draft articles" }
                    );
                }
            }

            return new CommentsEnvelope(article.Comments);
        }
    }
}
