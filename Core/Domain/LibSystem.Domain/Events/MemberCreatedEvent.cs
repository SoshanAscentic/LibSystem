using LibSystem.Domain.Common;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Events
{
    public sealed class MemberCreatedEvent : IDomainEvent
    {
        public MemberId MemberId { get; }
        public Name MemberName { get; }
        public string MemberType { get; }
        public DateTime OccurredOn { get; }

        public MemberCreatedEvent(MemberId memberId, Name memberName, string memberType)
        {
            MemberId = memberId;
            MemberName = memberName;
            MemberType = memberType;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
