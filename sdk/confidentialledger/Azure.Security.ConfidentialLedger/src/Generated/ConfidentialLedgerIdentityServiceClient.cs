// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Security.ConfidentialLedger
{
    /// <summary> The ConfidentialLedgerIdentityService service client. </summary>
    public partial class ConfidentialLedgerIdentityServiceClient
    {
        private static readonly string[] AuthorizationScopes = { "https://confidential-ledger.azure.com/.default" };
        private readonly TokenCredential _tokenCredential;

        private readonly HttpPipeline _pipeline;
        private readonly ClientDiagnostics _clientDiagnostics;
        private readonly Uri _identityServiceUri;
        private readonly string _apiVersion;

        /// <summary> The HTTP pipeline for sending and receiving REST requests and responses. </summary>
        public virtual HttpPipeline Pipeline { get => _pipeline; }

        /// <summary> Initializes a new instance of ConfidentialLedgerIdentityServiceClient for mocking. </summary>
        protected ConfidentialLedgerIdentityServiceClient()
        {
        }

        /// <summary> Gets identity information for a Confidential Ledger instance. </summary>
        /// <param name="ledgerId"> Id of the Confidential Ledger instance to get information for. </param>
        /// <param name="options"> The request options. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="ledgerId"/> is null. </exception>
        /// <remarks>
        /// Schema for <c>Response Body</c>:
        /// <code>{
        ///   ledgerId: string,
        ///   ledgerTlsCertificate: string
        /// }
        /// </code>
        /// Schema for <c>Response Error</c>:
        /// <code>{
        ///   error: {
        ///     code: string,
        ///     message: string,
        ///     innererror: ConfidentialLedgerErrorBody
        ///   }
        /// }
        /// </code>
        /// 
        /// </remarks>
#pragma warning disable AZC0002
        public virtual async Task<Response> GetLedgerIdentityAsync(string ledgerId, RequestOptions options = null)
#pragma warning restore AZC0002
        {
            using var scope = _clientDiagnostics.CreateScope("ConfidentialLedgerIdentityServiceClient.GetLedgerIdentity");
            scope.Start();
            try
            {
                using HttpMessage message = CreateGetLedgerIdentityRequest(ledgerId);
                return await _pipeline.ProcessMessageAsync(message, _clientDiagnostics, options).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Gets identity information for a Confidential Ledger instance. </summary>
        /// <param name="ledgerId"> Id of the Confidential Ledger instance to get information for. </param>
        /// <param name="options"> The request options. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="ledgerId"/> is null. </exception>
        /// <remarks>
        /// Schema for <c>Response Body</c>:
        /// <code>{
        ///   ledgerId: string,
        ///   ledgerTlsCertificate: string
        /// }
        /// </code>
        /// Schema for <c>Response Error</c>:
        /// <code>{
        ///   error: {
        ///     code: string,
        ///     message: string,
        ///     innererror: ConfidentialLedgerErrorBody
        ///   }
        /// }
        /// </code>
        /// 
        /// </remarks>
#pragma warning disable AZC0002
        public virtual Response GetLedgerIdentity(string ledgerId, RequestOptions options = null)
#pragma warning restore AZC0002
        {
            using var scope = _clientDiagnostics.CreateScope("ConfidentialLedgerIdentityServiceClient.GetLedgerIdentity");
            scope.Start();
            try
            {
                using HttpMessage message = CreateGetLedgerIdentityRequest(ledgerId);
                return _pipeline.ProcessMessage(message, _clientDiagnostics, options);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        internal HttpMessage CreateGetLedgerIdentityRequest(string ledgerId)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Get;
            var uri = new RawRequestUriBuilder();
            uri.Reset(_identityServiceUri);
            uri.AppendPath("/ledgerIdentity/", false);
            uri.AppendPath(ledgerId, true);
            uri.AppendQuery("api-version", _apiVersion, true);
            request.Uri = uri;
            request.Headers.Add("Accept", "application/json");
            message.ResponseClassifier = ResponseClassifier200.Instance;
            return message;
        }

        private sealed class ResponseClassifier200 : ResponseClassifier
        {
            private static ResponseClassifier _instance;
            public static ResponseClassifier Instance => _instance ??= new ResponseClassifier200();
            public override bool IsErrorResponse(HttpMessage message)
            {
                return message.Response.Status switch
                {
                    200 => false,
                    _ => true
                };
            }
        }
    }
}
