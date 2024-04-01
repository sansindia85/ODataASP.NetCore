﻿//---------------------------------------------------------------------
// <copyright file="RequestHandler.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Test.OData.Services.ODataWCFService.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using Microsoft.OData.Core;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.Test.OData.Services.ODataWCFService.DataSource;

    /// <summary>
    /// Base class for processing and responding to a client request.
    /// </summary>
    public abstract class RequestHandler
    {
        /// <summary>
        /// Create a RequestHandler on current context.
        /// </summary>
        /// <param name="httpMethod"></param>
        protected RequestHandler(HttpMethod httpMethod, IODataDataSource dataSource)
        {
            this.HttpMethod = httpMethod;
            this.RequestUri = Utility.RebuildUri(OperationContext.Current.RequestContext.RequestMessage.Properties.Via);

            this.DataSource = dataSource;

            this.RequestAcceptHeader = WebOperationContext.Current.IncomingRequest.Accept;
            this.RequestHeaders = WebOperationContext.Current.IncomingRequest.Headers.ToDictionary();

            this.ServiceRootUri = Utility.RebuildUri(new Uri(OperationContext.Current.Host.BaseAddresses.First().AbsoluteUri.TrimEnd('/') + "/"));

            this.QueryContext = new QueryContext(this.ServiceRootUri, this.RequestUri, this.DataSource.Model);

            string preference = this.RequestHeaders.ContainsKey(ServiceConstants.HttpHeaders.Prefer) ? this.RequestHeaders[ServiceConstants.HttpHeaders.Prefer] : string.Empty;
            this.PreferenceContext = new PreferenceContext(preference);
        }

        protected RequestHandler(RequestHandler other, HttpMethod httpMethod, Uri requestUri = null, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            // TODO: [tiano] We should have a deep check in to prevent infinite loop caused by bad code.
            this.HttpMethod = httpMethod;

            if (requestUri == null)
            {
                this.RequestUri = Utility.RebuildUri(other.RequestUri);
            }
            else
            {
                this.RequestUri = Utility.RebuildUri(requestUri);
            }


            this.DataSource = other.DataSource;

            if (headers == null)
            {
                this.RequestAcceptHeader = other.RequestAcceptHeader;
                this.RequestHeaders = new Dictionary<string, string>(other.RequestHeaders);
            }
            else
            {
                this.RequestHeaders = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> kvp in headers)
                {
                    this.RequestHeaders[kvp.Key] = kvp.Value;
                }

                this.RequestAcceptHeader = this.RequestHeaders.ContainsKey("Accept") ? this.RequestHeaders["Accept"] : string.Empty;
            }

            this.ServiceRootUri = Utility.RebuildUri(other.ServiceRootUri);

            this.QueryContext = new QueryContext(this.ServiceRootUri, this.RequestUri, this.DataSource.Model);

            this.PreferenceContext = other.PreferenceContext;
        }

        public HttpMethod HttpMethod
        {
            get;
            private set;
        }

        public Uri ServiceRootUri
        {
            get;
            private set;
        }

        public IODataDataSource DataSource
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the QueryContext.
        /// </summary>
        public QueryContext QueryContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the PreferenceContext
        /// </summary>
        public PreferenceContext PreferenceContext
        {
            get;
            private set;
        }

        public Dictionary<string, string> RequestHeaders
        {
            get;
            private set;
        }

        public string RequestAcceptHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the URI for the incoming request.
        /// </summary>
        public Uri RequestUri
        {
            get;
            private set;
        }

        protected bool TryDispatch(IODataRequestMessage requestMessage, IODataResponseMessage responseMessage)
        {
            RequestHandler handler = this.DispatchHandler();

            if (handler != null)
            {
                handler.Process(requestMessage, responseMessage);
                return true;
            }

            return false;
        }

        protected virtual RequestHandler DispatchHandler()
        {
            return null;
        }

        public virtual void Process(IODataRequestMessage requestMessage, IODataResponseMessage responseMessage)
        {
            throw new NotImplementedException();
        }

        public virtual Stream Process(Stream requestStream)
        {
            RequestHandler handler = this.DispatchHandler();
            if (handler != null)
            {
                return handler.Process(requestStream);
            }

            StreamPipe pipe = new StreamPipe();
            this.Process(this.CreateRequestMessage(requestStream), this.CreateResponseMessage(pipe.WriteStream));

            return pipe.ReadStream;
        }

        public virtual Stream ProcessAsynchronously(Stream requestStream)
        {
            DateTime now = DateTime.Now;
            string asyncToken = now.Ticks.ToString(CultureInfo.InvariantCulture);

            AsyncTask asyncTask = null;
            if (requestStream == null)
            {
                asyncTask = new AsyncTask(this, this.CreateRequestMessage(null), now.AddSeconds(AsyncTask.DefaultDuration));
            }
            else
            {
                StreamPipe requestPipe = new StreamPipe();
                using (requestPipe.WriteStream)
                {
                    requestStream.CopyTo(requestPipe.WriteStream); // read the input stream to memory
                }

                var requestMessage = this.CreateRequestMessage(requestPipe.ReadStream);
                asyncTask = new AsyncTask(this, requestMessage, now.AddSeconds(AsyncTask.DefaultDuration));
            }
            AsyncTask.AddTask(asyncToken, asyncTask);

            StreamPipe responsePipe = new StreamPipe();
            var responseMessage = new ODataResponseMessage(responsePipe.WriteStream, 202); //202 Accepted
            responseMessage.PreferenceAppliedHeader().RespondAsync = true;
            ResponseWriter.WriteAsyncPendingResponse(responseMessage, asyncToken);
            return responsePipe.ReadStream;
        }


        #region Reader API

        protected virtual IODataRequestMessage CreateRequestMessage(Stream messageBody)
        {
            return new ODataRequestMessage(messageBody, this.RequestHeaders, this.RequestUri, this.HttpMethod.ToString());
        }

        protected virtual IODataResponseMessage CreateResponseMessage(Stream stream)
        {
            return new ODataResponseMessage(stream, 200);
        }

        protected virtual ODataMessageReader CreateMessageReader(IODataRequestMessage message)
        {
            return new ODataMessageReader(
                message,
                this.GetReaderSettings(),
                this.DataSource.Model);
        }

        protected virtual ODataMessageWriter CreateMessageWriter(IODataResponseMessage message)
        {
            return new ODataMessageWriter(
                message,
                this.GetWriterSettings(),
                this.DataSource.Model);
        }

        #endregion

        #region Write API

        protected virtual ODataMessageReaderSettings GetReaderSettings()
        {
            return new ODataMessageReaderSettings();
        }

        protected virtual ODataMessageWriterSettings GetWriterSettings()
        {
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                AutoComputePayloadMetadataInJson = true,
                PayloadBaseUri = this.ServiceRootUri,
                ODataUri = new ODataUri()
                {
                    RequestUri = this.RequestUri,
                    ServiceRoot = this.ServiceRootUri,
                    Path = this.QueryContext.QueryPath,
                    SelectAndExpand = this.QueryContext.QuerySelectExpandClause,
                }
            };

            // TODO: howang why here?
            if (this.QueryContext != null)
            {
                if (this.QueryContext.CanonicalUri == null)
                {
                    settings.ODataUri.RequestUri = this.QueryContext.QueryUri;
                    settings.ODataUri.Path = this.QueryContext.QueryPath;
                }
                else
                {
                    settings.ODataUri.RequestUri = this.QueryContext.CanonicalUri;
                    ODataUriParser uriParser = new ODataUriParser(this.DataSource.Model, ServiceConstants.ServiceBaseUri, this.QueryContext.CanonicalUri);
                    settings.ODataUri.Path = uriParser.ParsePath();
                }
            }

            // TODO: howang read the encoding from request.
            settings.SetContentType(string.IsNullOrEmpty(this.QueryContext.FormatOption) ? this.RequestAcceptHeader : this.QueryContext.FormatOption, Encoding.UTF8.WebName);

            return settings;
        }

        #endregion
    }
}
