using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Members.CreateMember
{
    public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
    {
        public CreateMemberCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
                .MinimumLength(1).WithMessage("Name cannot be empty.");

            RuleFor(x => x.MemberType)
                .Must(BeAValidMemberType).WithMessage("Member type must be 0 (Member), 1 (Minor Staff), or 2 (Management Staff).");
        }

        private static bool BeAValidMemberType(int memberType)
        {
            return memberType >= 0 && memberType <= 2;
        }
    }
}
