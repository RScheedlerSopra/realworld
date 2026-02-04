using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Drafts;

public class EditDraft
{
    public record DraftData(string? Title, string? Description, string? Body, string[]? TagList);

    public record Command(Model Model, int Id) : IRequest<DraftEnvelope>;

    public record Model(DraftData Draft);

    public class DraftDataValidator : AbstractValidator<DraftData>
    {
        public DraftDataValidator()
        {
            // Only require title to be non-empty if provided
            RuleFor(x => x.Title).NotEmpty().When(x => x.Title != null);
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Model.Draft).NotNull().SetValidator(new DraftDataValidator());
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class Handler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Command, DraftEnvelope>
    {
        public async Task<DraftEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var article = await context
                .Articles.Include(x => x.ArticleTags)
                .Include(x => x.Author)
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
                    new { Draft = "You are not authorized to edit this draft" }
                );
            }

            article.Description = message.Model.Draft.Description ?? article.Description;
            article.Body = message.Model.Draft.Body ?? article.Body;
            article.Title = message.Model.Draft.Title ?? article.Title;
            // No slug generation for drafts

            // list of currently saved article tags for the given article
            var articleTagList = message.Model.Draft.TagList ?? Enumerable.Empty<string>();

            var articleTagsToCreate = GetArticleTagsToCreate(article, articleTagList);
            var articleTagsToDelete = GetArticleTagsToDelete(article, articleTagList);

            if (
                context.ChangeTracker.Entries().First(x => x.Entity == article).State
                    == EntityState.Modified
                || articleTagsToCreate.Count != 0
                || articleTagsToDelete.Count != 0
            )
            {
                article.UpdatedAt = DateTime.UtcNow;
            }

            // ensure context is tracking any tags that are about to be created so that it won't attempt to insert a duplicate
            context.Tags.AttachRange(
                [.. articleTagsToCreate.Where(x => x.Tag is not null).Select(a => a.Tag!)]
            );

            // add the new article tags
            await context.ArticleTags.AddRangeAsync(articleTagsToCreate, cancellationToken);

            // delete the tags that do not exist anymore
            context.ArticleTags.RemoveRange(articleTagsToDelete);

            await context.SaveChangesAsync(cancellationToken);

            article = await context
                .Articles.Include(x => x.Author)
                .Include(x => x.ArticleFavorites)
                .Include(x => x.ArticleTags)
                .Where(x => x.ArticleId == message.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (article is null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { Draft = Constants.NOT_FOUND }
                );
            }

            return new DraftEnvelope(article);
        }

        /// <summary>
        /// check which article tags need to be added
        /// </summary>
        private static List<ArticleTag> GetArticleTagsToCreate(
            Article article,
            IEnumerable<string> articleTagList
        )
        {
            var articleTagsToCreate = new List<ArticleTag>();
            foreach (var tag in articleTagList)
            {
                var at = article.ArticleTags?.FirstOrDefault(t => t.TagId == tag);
                if (at == null)
                {
                    at = new ArticleTag
                    {
                        Article = article,
                        ArticleId = article.ArticleId,
                        Tag = new Tag { TagId = tag },
                        TagId = tag,
                    };
                    articleTagsToCreate.Add(at);
                }
            }

            return articleTagsToCreate;
        }

        /// <summary>
        /// check which article tags need to be deleted
        /// </summary>
        private static List<ArticleTag> GetArticleTagsToDelete(
            Article article,
            IEnumerable<string> articleTagList
        )
        {
            var articleTagsToDelete = new List<ArticleTag>();
            foreach (var tag in article.ArticleTags)
            {
                var at = articleTagList.FirstOrDefault(t => t == tag.TagId);
                if (at == null)
                {
                    articleTagsToDelete.Add(tag);
                }
            }

            return articleTagsToDelete;
        }
    }
}
