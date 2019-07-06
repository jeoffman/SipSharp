using System;

namespace SipSharp
{
    /// <summary>
    /// Exception which can be sent back to client.
    /// </summary>
    public class SipException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SipException"/> class.
        /// </summary>
        /// <param name="code">SIP status code.</param>
        /// <param name="errMsg">Why exception was thrown.</param>
        public SipException(StatusCodes code, string errMsg) : base(errMsg)
        {
            StatusCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SipException"/> class.
        /// </summary>
        /// <param name="code">SIP status code.</param>
        /// <param name="errMsg">Why exception was thrown.</param>
        /// <param name="inner">Inner exception.</param>
        public SipException(StatusCodes code, string errMsg, Exception inner) : base(errMsg, inner)
        {
            StatusCode = code;
        }

        /// <summary>
        /// Gets sip status code to return
        /// </summary>
        public StatusCodes StatusCode { get; private set; }
    }
}