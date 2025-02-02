using FluentValidation;

namespace backend.kapace.Models.Requests.Permission;

public class V1PermissionCreateRequest
{
    public string Alias { get; init; }
    public string Description { get; init; }
    public long CreatedBy { get; init; }

    public class Validator : AbstractValidator<V1PermissionCreateRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Alias).NotNull().NotEmpty();
            RuleFor(r => r.Description).NotNull().NotEmpty();
            RuleFor(r => r.CreatedBy).GreaterThan(0);
        }
    }
}