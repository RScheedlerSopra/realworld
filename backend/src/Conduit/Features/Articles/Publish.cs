using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Articles;

public class Publish
{
    public record Command(string Slug) : IRequest<ArticleEnvelope>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator() => RuleFor(x => x.Slug).NotNull().NotEmpty();
    }

    public class Handler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Command, ArticleEnvelope>
    {
        public async Task<ArticleEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            // Try to find a draft with this slug first
            var article = await context
                .Articles.Include(x => x.Author)
                .FirstOrDefaultAsync(x => x.Slug == message.Slug && x.IsDraft, cancellationToken);

            // If no draft found, check if a published article exists with this slug
            if (article == null)
            {
                var publishedExists = await context.Articles
                    .AnyAsync(x => x.Slug == message.Slug && !x.IsDraft, cancellationToken);

                if (publishedExists)
                {
                    throw new RestException(
                        HttpStatusCode.BadRequest,
                        new { Article = "Article is already published" }
                    );
                }

                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            var currentUsername = currentUserAccessor.GetCurrentUsername();
            if (article.Author?.Username != currentUsername)
            {
                throw new RestException(
                    HttpStatusCode.Forbidden,
                    new { Article = "You are not authorized to publish this draft" }
                );
            }

            // Ensure slug uniqueness - append suffix if slug already exists
            var originalSlug = article.Slug;
            var slugSuffix = 1;
            while (await context.Articles.AnyAsync(
                x => x.Slug == article.Slug && x.ArticleId != article.ArticleId,
                cancellationToken))
            {
                article.Slug = $"{originalSlug}-{slugSuffix}";
                slugSuffix++;
            }

            article.IsDraft = false;
            article.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            article = await context
                .Articles.GetAllData()
                .FirstOrDefaultAsync(x => x.ArticleId == article.ArticleId, cancellationToken);

            if (article is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Article = Constants.NOT_FOUND }
                );
            }

            return new ArticleEnvelope(article);
        }
    }
}
