using System;
using System.Collections.Generic;
using System.Text;

namespace SipSharp.Sip.Exceptions
{
    internal enum RequestRejectionReason
    {
        UnrecognizedSipVersion,
        InvalidRequestLine,
        InvalidRequestHeader,
        InvalidRequestHeadersNoCRLF,
        MalformedRequestInvalidHeaders,
        InvalidContentLength,
        MultipleContentLengths,
        UnexpectedEndOfRequestContent,
        BadChunkSuffix,
        BadChunkSizeData,
        ChunkedRequestIncomplete,
        InvalidRequestTarget,
        InvalidCharactersInHeaderName,
        RequestLineTooLong,
        HeadersExceedMaxTotalSize,
        TooManyHeaders,
        RequestBodyTooLarge,
        RequestHeadersTimeout,
        RequestBodyTimeout,
        FinalTransferCodingNotChunked,
        LengthRequired,
        OptionsMethodRequired,
        ConnectMethodRequired,
        MissingHostHeader,
        MultipleHostHeaders,
        InvalidHostHeader,
        UpgradeRequestCannotHavePayload,
        RequestBodyExceedsContentLength
    }
}
