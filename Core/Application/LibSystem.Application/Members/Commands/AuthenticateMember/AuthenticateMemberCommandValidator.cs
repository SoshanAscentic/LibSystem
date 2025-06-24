using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Members.Commands.AuthenticateMember
{
    public class AuthenticateMemberCommandValidator : AbstractValidator<AuthenticateMemberCommand>
    {
        public AuthenticateMemberCommandValidator()
        {
            RuleFor(x => x.MemberID)
                .GreaterThan(0).WithMessage("Member ID must be positive.");
        }
    }
}
