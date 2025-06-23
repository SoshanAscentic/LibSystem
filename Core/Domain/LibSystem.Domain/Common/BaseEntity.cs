using LibSystem.Domain.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Common
{
    public abstract class BaseEntity
    {
        //Implements domain events pattern for rich domain model
        private readonly List<IDomainEvent> domainEvents = new();

        public int id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }


        //Gets the domain events that have been raised by this entity and used for eventual consistency and side effects
        [NotMapped]
        private IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

        //Enabling loose cupling between domain events and event handlers
        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
            domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
            domainEvents.Remove(domainEvent);
        }

        public void CleanDomainEvents()
        {
            domainEvents.Clear();
        }
    }
}
