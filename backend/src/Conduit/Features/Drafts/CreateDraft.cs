using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Drafts;

public class CreateDraft
{
    public class DraftData
    {
        public string? Title { get; init; }

        public string? Description { get; init; }

        public string? Body { get; init; }

        public string[]? TagList { get; init; }
    }

    public class DraftDataValidator : AbstractValidator<DraftData>
    {
        public DraftDataValidator()
        {
            // Only title is required for drafts
            RuleFor(x => x.Title).NotNull().NotEmpty();
        }
    }

    public record Command(DraftData Draft) : IRequest<DraftEnvelope>;

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator() =>
            RuleFor(x => x.Draft).NotNull().SetValidator(new DraftDataValidator());
    }

    public class Handler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Command, DraftEnvelope>
    {
        public async Task<DraftEnvelope> Handle(
            Command message,
            CancellationToken cancellationToken
        )
        {
            var author = await context.Persons.FirstAsync(
                x => x.Username == currentUserAccessor.GetCurrentUsername(),
                cancellationToken
            );
            var tags = new List<Tag>();
            foreach (var tag in (message.Draft.TagList ?? Enumerable.Empty<string>()))
            {
                var t = await context.Tags.FindAsync(tag);
                if (t == null)
                {
                    t = new Tag { TagId = tag };
                    await context.Tags.AddAsync(t, cancellationToken);
                    //save immediately for reuse
                    await context.SaveChangesAsync(cancellationToken);
                }
                tags.Add(t);
            }

            var article = new Article
            {
                Author = author,
                Body = message.Draft.Body,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Description = message.Draft.Description,
                Title = message.Draft.Title,
                IsDraft = true,
                Slug = null, // No slug for drafts
            };
            await context.Articles.AddAsync(article, cancellationToken);

            await context.ArticleTags.AddRangeAsync(
                tags.Select(x => new ArticleTag { Article = article, Tag = x }),
                cancellationToken
            );

            await context.SaveChangesAsync(cancellationToken);

            return new DraftEnvelope(article);
        }
    }
}
