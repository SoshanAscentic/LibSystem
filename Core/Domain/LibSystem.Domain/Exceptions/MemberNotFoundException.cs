using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Exceptions
{
    public sealed class MemberNotFoundException : DomainException
    {
        public MemberId MemberId { get; }

        public MemberNotFoundException(MemberId memberId)
            : base($"Member with ID {memberId.Value} was not found.")
        {
            MemberId = memberId;
        }

        public MemberNotFoundException(int memberId)
            : this(MemberId.Create(memberId))
        {
        }
    }
}
