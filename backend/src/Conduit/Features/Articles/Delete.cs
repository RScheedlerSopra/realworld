using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Articles;

public class Delete
{
    public record Command(string Slug) : IRequest;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator() => RuleFor(x => x.Slug).NotNull().NotEmpty();
    }

    public class QueryHandler(ConduitContext context, ICurrentUserAccessor currentUserAccessor) : IRequestHandler<Command>
    {
        public async Task Handle(Command message, CancellationToken cancellationToken)
        {
            var article =
                await context.Articles
                    .Include(x => x.Author)
                    .FirstOrDefaultAsync(
                        x => x.Slug == message.Slug,
                        cancellationToken
                    )
                ?? throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );

            // Check authorization - only article author can delete
            var currentUsername = currentUserAccessor.GetCurrentUsername();
            if (article.Author?.Username != currentUsername)
            {
                throw new RestException(
                    HttpStatusCode.Forbidden,
                    new { Article = "You are not authorized to delete this article" }
                );
            }

            context.Articles.Remove(article);
            await context.SaveChangesAsync(cancellationToken);
            await Task.FromResult(Unit.Value);
        }
    }
}
