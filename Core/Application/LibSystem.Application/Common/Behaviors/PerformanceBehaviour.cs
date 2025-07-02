// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformanceBehaviour.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Common.Behaviors
{
    using System.Diagnostics;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    {
        private readonly Stopwatch timer;
        private readonly ILogger<TRequest> logger;

        public PerformanceBehaviour(ILogger<TRequest> logger)
        {
            this.logger = logger;
            this.timer = new Stopwatch();
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            this.timer.Start();
            var response = await next();
            this.timer.Stop();
            var elapsedMilliseconds = this.timer.ElapsedMilliseconds;
            if (elapsedMilliseconds > 500) // Log if the request takes more than 500ms
            {
                var requestName = typeof(TRequest).Name;

                this.logger.LogWarning(
                    "Library System Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                    requestName,
                    elapsedMilliseconds,
                    request);
            }

            return response;
        }
    }
}
