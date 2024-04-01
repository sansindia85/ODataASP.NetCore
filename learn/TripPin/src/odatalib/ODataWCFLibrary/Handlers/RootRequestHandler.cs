﻿//---------------------------------------------------------------------
// <copyright file="RootRequestHandler.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Test.OData.Services.ODataWCFService.Handlers
{
    using System;
    using System.IO;
    using System.Net;
    using Microsoft.OData.Core;
    using Microsoft.Test.OData.Services.ODataWCFService.DataSource;

    public class RootRequestHandler : RequestHandler
    {
        public RootRequestHandler(HttpMethod httpMethod, IODataDataSource dataSource)
            : base(httpMethod, dataSource)
        {
        }

        protected override RequestHandler DispatchHandler()
        {
            switch (this.HttpMethod)
            {
                case HttpMethod.GET:
                    return new QueryHandler(this);
                case HttpMethod.POST:
                    return new CreateHandler(this);
                case HttpMethod.DELETE:
                    return new DeleteHandler(this);
                case HttpMethod.PATCH:
                case HttpMethod.PUT:
                    return new UpdateHandler(this, this.HttpMethod);
                default:
                    throw Utility.BuildException(HttpStatusCode.MethodNotAllowed);
            }
        }

        public override Stream Process(Stream requestStream)
        {
            try
            {
                RequestHandler handler = this.DispatchHandler();

                if (this.PreferenceContext.RespondAsync)
                {
                    return handler.ProcessAsynchronously(requestStream);
                }
                return handler.Process(requestStream);
            }
            catch (Exception e)
            {
                ErrorHandler handler = new ErrorHandler(this, e);
                return handler.Process(null);
            }
        }

        public override void Process(IODataRequestMessage requestMessage, IODataResponseMessage responseMessage)
        {
            throw new InvalidOperationException("The RootRequestHandler cannot be invoked by Process(IODataRequestMessage, IODataResponseMessage).");
        }
    }
}