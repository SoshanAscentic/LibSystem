using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Common.Behaviors
{
    public class PerformanceBehaviour <TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    {
        private readonly Stopwatch timer;
        private readonly ILogger<TRequest> logger;

        public PerformanceBehaviour(ILogger<TRequest> logger)
        {
            this.logger = logger;
            timer = new Stopwatch();
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            timer.Start();
            var response = await next();
            timer.Stop();
            var elapsedMilliseconds = timer.ElapsedMilliseconds;
            if (elapsedMilliseconds > 500) // Log if the request takes more than 500ms
            {
                var requestName = typeof(TRequest).Name;

                logger.LogWarning("Library System Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                    requestName, elapsedMilliseconds, request);
            }

            return response;
        }
    }
}
