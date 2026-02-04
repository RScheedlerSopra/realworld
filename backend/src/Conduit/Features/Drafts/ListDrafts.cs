using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Drafts;

public class ListDrafts
{
    public record Query(int? Limit, int? Offset) : IRequest<DraftsEnvelope>;

    public class QueryHandler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, DraftsEnvelope>
    {
        public async Task<DraftsEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var currentUsername = currentUserAccessor.GetCurrentUsername();

            var queryable = context
                .Articles.Include(x => x.Author)
                .Include(x => x.ArticleFavorites)
                .Include(x => x.ArticleTags)
                .Where(x => x.IsDraft && x.Author!.Username == currentUsername)
                .AsNoTracking();

            var drafts = await queryable
                .OrderByDescending(x => x.UpdatedAt)
                .Skip(message.Offset ?? 0)
                .Take(message.Limit ?? 20)
                .ToListAsync(cancellationToken);

            return new DraftsEnvelope
            {
                Drafts = drafts,
                DraftsCount = await queryable.CountAsync(cancellationToken),
            };
        }
    }
}
