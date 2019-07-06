using Microsoft.Extensions.Primitives;
using SipSharp.Sip.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SipSharp.Sip.Exceptions
{
    public sealed class BadSipRequestException : IOException
    {
        private BadSipRequestException(string message, int statusCode, RequestRejectionReason reason)
            : this(message, statusCode, reason, null)
        { }

        private BadSipRequestException(string message, int statusCode, RequestRejectionReason reason, SipMethod? requiredMethod)
            : base(message)
        {
            StatusCode = statusCode;
            Reason = reason;

            if (requiredMethod.HasValue)
            {
                //AllowedHeader = SipUtilities.MethodToString(requiredMethod.Value);
            }
        }

        public int StatusCode { get; }

        internal StringValues AllowedHeader { get; }

        internal RequestRejectionReason Reason { get; }

        //[StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason)
        {
            throw GetException(reason);
        }

        //[StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason, SipMethod method)
            => throw GetException(reason, method.ToString().ToUpperInvariant());

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static BadSipRequestException GetException(RequestRejectionReason reason)
        {
            BadSipRequestException ex;
            switch (reason)
            {
                //case RequestRejectionReason.InvalidRequestHeadersNoCRLF:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_InvalidRequestHeadersNoCRLF, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.InvalidRequestLine:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_InvalidRequestLine, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.MalformedRequestInvalidHeaders:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_MalformedRequestInvalidHeaders, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.MultipleContentLengths:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_MultipleContentLengths, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.UnexpectedEndOfRequestContent:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_UnexpectedEndOfRequestContent, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.BadChunkSuffix:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_BadChunkSuffix, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.BadChunkSizeData:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_BadChunkSizeData, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.ChunkedRequestIncomplete:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_ChunkedRequestIncomplete, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.InvalidCharactersInHeaderName:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_InvalidCharactersInHeaderName, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.RequestLineTooLong:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_RequestLineTooLong, StatusCodes.Status414UriTooLong, reason);
                //    break;
                //case RequestRejectionReason.HeadersExceedMaxTotalSize:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_HeadersExceedMaxTotalSize, StatusCodes.Status431RequestHeaderFieldsTooLarge, reason);
                //    break;
                //case RequestRejectionReason.TooManyHeaders:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_TooManyHeaders, StatusCodes.Status431RequestHeaderFieldsTooLarge, reason);
                //    break;
                //case RequestRejectionReason.RequestBodyTooLarge:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_RequestBodyTooLarge, StatusCodes.Status413PayloadTooLarge, reason);
                //    break;
                //case RequestRejectionReason.RequestHeadersTimeout:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_RequestHeadersTimeout, StatusCodes.Status408RequestTimeout, reason);
                //    break;
                //case RequestRejectionReason.RequestBodyTimeout:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_RequestBodyTimeout, StatusCodes.Status408RequestTimeout, reason);
                //    break;
                //case RequestRejectionReason.OptionsMethodRequired:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_MethodNotAllowed, StatusCodes.Status405MethodNotAllowed, reason, SipMethod.Options);
                //    break;
                //case RequestRejectionReason.ConnectMethodRequired:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_MethodNotAllowed, StatusCodes.Status405MethodNotAllowed, reason, SipMethod.Connect);
                //    break;
                //case RequestRejectionReason.MissingHostHeader:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_MissingHostHeader, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.MultipleHostHeaders:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_MultipleHostHeaders, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.InvalidHostHeader:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_InvalidHostHeader, StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.UpgradeRequestCannotHavePayload:
                //    ex = new BadSipRequestException(CoreStrings.BadRequest_UpgradeRequestCannotHavePayload, StatusCodes.Status400BadRequest, reason);
                //    break;
                default:
                    ex = new BadSipRequestException(CoreStrings.BadRequest, (int)StatusCodes.NotFound, reason);
                    break;
            }
            return ex;
        }

        //[StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason, string detail)
        {
            throw GetException(reason, detail);
        }

        //[StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason, StringValues detail)
        {
            throw GetException(reason, detail.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static BadSipRequestException GetException(RequestRejectionReason reason, string detail)
        {
            BadSipRequestException ex;
            switch (reason)
            {
                //case RequestRejectionReason.InvalidRequestLine:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_InvalidRequestLine_Detail(detail), StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.InvalidRequestTarget:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_InvalidRequestTarget_Detail(detail), StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.InvalidRequestHeader:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_InvalidRequestHeader_Detail(detail), StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.InvalidContentLength:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_InvalidContentLength_Detail(detail), StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.UnrecognizedSipVersion:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_UnrecognizedSipVersion(detail), StatusCodes.Status505SipVersionNotsupported, reason);
                //    break;
                //case RequestRejectionReason.FinalTransferCodingNotChunked:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_FinalTransferCodingNotChunked(detail), StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.LengthRequired:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_LengthRequired(detail), StatusCodes.Status411LengthRequired, reason);
                //    break;
                //case RequestRejectionReason.LengthRequiredSip10:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_LengthRequiredSip10(detail), StatusCodes.Status400BadRequest, reason);
                //    break;
                //case RequestRejectionReason.InvalidHostHeader:
                //    ex = new BadSipRequestException(CoreStrings.FormatBadRequest_InvalidHostHeader_Detail(detail), StatusCodes.Status400BadRequest, reason);
                //    break;
                default:
                    ex = new BadSipRequestException(CoreStrings.BadRequest, (int)StatusCodes.NotFound, reason);
                    break;
            }
            return ex;
        }
    }
}
