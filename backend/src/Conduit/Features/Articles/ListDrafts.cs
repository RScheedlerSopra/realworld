using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Articles;

public class ListDrafts
{
    public record Query(int? Limit, int? Offset) : IRequest<ArticlesEnvelope>;

    public class QueryHandler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, ArticlesEnvelope>
    {
        public async Task<ArticlesEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var currentUsername = currentUserAccessor.GetCurrentUsername();

            if (currentUsername == null)
            {
                throw new RestException(
                    HttpStatusCode.Unauthorized,
                    new { User = "User must be authenticated to view drafts" }
                );
            }

            var currentUser = await context
                .Persons.FirstOrDefaultAsync(
                    x => x.Username == currentUsername,
                    cancellationToken
                );

            if (currentUser is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { User = Constants.NOT_FOUND }
                );
            }

            var queryable = context
                .Articles.GetAllData()
                .Where(x => x.IsDraft && x.Author!.PersonId == currentUser.PersonId);

            var articles = await queryable
                .OrderByDescending(x => x.UpdatedAt)
                .Skip(message.Offset ?? 0)
                .Take(message.Limit ?? 20)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new ArticlesEnvelope { Articles = articles, ArticlesCount = queryable.Count() };
        }
    }
}
