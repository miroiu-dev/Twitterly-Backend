using FluentValidation;
using FluentValidation.Results;
using Twitter.ApiErrors;

namespace Twitter.Filters
{
    public class ValidateEndpointFilter<TValidator, TEntity> : IEndpointFilter where TValidator : AbstractValidator<TEntity>, new() where TEntity : notnull
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {

            var entity = context.Arguments.OfType<TEntity>().FirstOrDefault(a => a?.GetType() == typeof(TEntity));

            if (entity is null) return Results.Problem("Could not find a type to validate");
            
            TValidator validator = new();

            ValidationResult validationResult = await validator.ValidateAsync(entity);

            if (!validationResult.IsValid)
            {
                var dic = validationResult.ToDictionary();
                var error = dic.Take(1).Select(d => d.Value).First().First();
                return Results.BadRequest(new AuthenticationError(error));
            }

            var result = await next(context);

            return result;
        }
    }
}
