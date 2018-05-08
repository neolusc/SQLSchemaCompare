﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SQLCompare.Core;
using SQLCompare.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SQLCompare.UI.Middlewares
{
    /// <summary>
    /// Middleware that validate the header of each request
    /// </summary>
    public class RequestValidatorMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestValidatorMiddleware> logger;
        private readonly RequestValidatorSettings options;
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestValidatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">The RequestDelegate instance</param>
        /// <param name="logger">The injected Logger</param>
        /// <param name="options">Configuration options for the validation</param>
        /// <param name="appGlobals">The injected application global constants</param>
        public RequestValidatorMiddleware(RequestDelegate next, ILogger<RequestValidatorMiddleware> logger, IOptions<RequestValidatorSettings> options, IAppGlobals appGlobals)
        {
            this.next = next;
            this.logger = logger;
            this.options = options.Value;
            this.appGlobals = appGlobals;
        }

        /// <summary>
        /// Method that gets invoked when the middleware is executed
        /// </summary>
        /// <param name="context">The HttpContext of the current request</param>
        /// <returns>The next task</returns>
        public async Task Invoke(HttpContext context)
        {
            string authToken = context.Request.Headers[this.appGlobals.AuthorizationHeaderName];
            string userAgent = context.Request.Headers["User-Agent"];

            if ((string.IsNullOrEmpty(this.options.AllowedRequestGuid) || string.Equals(authToken, this.options.AllowedRequestGuid, StringComparison.Ordinal)) &&
                (string.IsNullOrEmpty(this.options.AllowedRequestAgent) || string.Equals(userAgent, this.options.AllowedRequestAgent, StringComparison.Ordinal)))
            {
                await this.next.Invoke(context).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogError($"Request refused. Token:{authToken}; UserAgent:{userAgent}");
                context.Response.Body = new MemoryStream(0);
            }
        }
    }
}