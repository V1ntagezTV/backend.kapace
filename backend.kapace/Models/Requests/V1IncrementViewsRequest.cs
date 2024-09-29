using FluentValidation;

namespace backend.kapace.Models.Requests;

public record V1IncrementViewsRequest(long ContentId)
{
    public class Validator : AbstractValidator<V1IncrementViewsRequest>
    {
        public Validator()
        {
            RuleFor(r => r.ContentId).GreaterThan(0);
        }
    }
}