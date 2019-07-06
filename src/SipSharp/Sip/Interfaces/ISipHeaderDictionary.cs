﻿using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace SipSharp.Sip.Interfaces
{
    class ISipHeaderDictionary
    {
    }


        /// <summary>
        /// Represents HttpRequest and HttpResponse headers
        /// </summary>
        public interface IHeaderDictionary : IDictionary<string, StringValues>
        {
            /// <summary>
            /// IHeaderDictionary has a different indexer contract than IDictionary, where it will return StringValues.Empty for missing entries.
            /// </summary>
            /// <param name="key"></param>
            /// <returns>The stored value, or StringValues.Empty if the key is not present.</returns>
            new StringValues this[string key] { get; set; }

            /// <summary>
            /// Strongly typed access to the Content-Length header. Implementations must keep this in sync with the string representation.
            /// </summary>
            long? ContentLength { get; set; }
        }
}
