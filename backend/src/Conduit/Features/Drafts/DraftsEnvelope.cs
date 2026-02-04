using System.Collections.Generic;
using Conduit.Domain;

namespace Conduit.Features.Drafts;

public class DraftsEnvelope
{
    public List<Article> Drafts { get; set; } = new();

    public int DraftsCount { get; set; }
}
