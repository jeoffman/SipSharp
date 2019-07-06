﻿using System;
using SipSharp.Messages.Headers;

namespace SipSharp.Messages
{
    /// <summary>
    /// Response to a <see cref="IRequest"/>.
    /// </summary>
    public class Response : Message, IResponse
    {
        public Response(string version, StatusCodes code, string phrase)
        {
            StatusCode = code;
            SipVersion = version;
            ReasonPhrase = phrase;
        }

        #region IResponse Members

        /// <summary>
        /// Gets or sets SIP status code.
        /// </summary>
        public StatusCodes StatusCode { get; set; }

        /// <summary>
        /// Gets or sets text describing why the status code was used.
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets where we want to be contacted
        /// </summary>
        public ContactHeader Contact { get; set; }

        /// <summary>
        /// Gets or sets all routes.
        /// </summary>
        public Route Route { get; internal set; }

        public override void Validate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}