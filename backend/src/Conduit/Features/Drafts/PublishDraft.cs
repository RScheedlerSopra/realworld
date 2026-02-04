using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Articles;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Drafts;

public class PublishDraft
{
    private static readonly string[] TitleRequiredError = ["Title is required"];
    private static readonly string[] DescriptionRequiredError = ["Description is required"];
    private static readonly string[] BodyRequiredError = ["Body is required"];

    public record Command(int Id) : IRequest<ArticleEnvelope>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
    }

    public class Handler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Command, ArticleEnvelope>
    {
        public async Task<ArticleEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var article = await context
                .Articles.Include(x => x.Author)
                .Include(x => x.ArticleTags)
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
                    new { Draft = "You are not authorized to publish this draft" }
                );
            }

            // Validate all required fields are present for publication
            if (string.IsNullOrWhiteSpace(article.Title))
            {
                throw new RestException(
                    HttpStatusCode.UnprocessableEntity,
                    new { Title = TitleRequiredError }
                );
            }

            if (string.IsNullOrWhiteSpace(article.Description))
            {
                throw new RestException(
                    HttpStatusCode.UnprocessableEntity,
                    new { Description = DescriptionRequiredError }
                );
            }

            if (string.IsNullOrWhiteSpace(article.Body))
            {
                throw new RestException(
                    HttpStatusCode.UnprocessableEntity,
                    new { Body = BodyRequiredError }
                );
            }

            // Publish the draft
            article.IsDraft = false;
            article.Slug = article.Title.GenerateSlug();
            article.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            // Reload article with all data
            article = await context
                .Articles.GetAllData()
                .Where(x => x.ArticleId == message.Id)
                .FirstOrDefaultAsync(cancellationToken);

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
