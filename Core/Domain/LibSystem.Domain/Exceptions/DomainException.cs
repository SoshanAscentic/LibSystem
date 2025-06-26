using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
        protected DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public override string ToString()
        {
            return $"{GetType().Name}: {Message}";
        }
    }
}
