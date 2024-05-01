using BookInventory.Models;
using FluentValidation;

namespace BookInventory.Api.Validators
{
    public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
    {
        public CreateBookDtoValidator()
        {
            RuleFor(x => x.Author).NotEmpty();
            RuleFor(x => x.BookType).NotEmpty();
            RuleFor(x => x.Condition).NotEmpty();
            RuleFor(x => x.Genre).NotEmpty();
            RuleFor(x => x.ISBN).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Publisher).NotEmpty();
            RuleFor(x => x.Summary).NotEmpty();
            RuleFor(x => x.Quantity).NotEmpty();
            RuleFor(x => x.Price).NotEmpty();
        }
    }
}