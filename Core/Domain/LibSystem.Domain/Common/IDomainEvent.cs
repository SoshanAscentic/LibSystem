using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Common
{
    //Domain events are used to notify other parts of the system about changes in the domain model.CROSS-AGGREGATE !!!
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}
