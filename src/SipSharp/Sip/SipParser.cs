using SipSharp.Sip.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SipSharp.Sip
{
    public class SipParser<TRequestHandler> : ISipParser<TRequestHandler> where TRequestHandler : ISipHeadersHandler, ISipRequestLineHandler
    {
        private bool _showErrorDetails;

        private const byte ByteCR = (byte)'\r';
        private const byte ByteLF = (byte)'\n';
        private static readonly byte[] ByteCRLR = { (byte)'\r', (byte)'\n' };

        public string RequestLine { get; set; }
        public string Body { get; set; }
        public Dictionary<string, List<string>> Headers { get; set; } = new Dictionary<string, List<string>>();

        public SipParser() : this(showErrorDetails: true)
        {
        }

        public SipParser(bool showErrorDetails)
        {
            _showErrorDetails = showErrorDetails;
        }

        /// <summary>Request line, optional headers, empty line, optional message body</summary>
        /// <param name="requestText">Complete request</param>
        /// <returns>sure</returns>
        public bool ParseRequest(string requestText)
        {
            var requestParts = requestText.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (requestParts.Length > 0)
            {   //process request line and headers
                string firstLineAndHeaders = requestParts[0];

                if (requestParts.Length > 1)
                {   //process body
                    string body = requestParts[1];
                }
            }
            return false;
        }

        /// <summary>Request line, optional headers, empty line, optional message body</summary>
        /// <param name="requestText">Complete request</param>
        /// <returns>sure</returns>
        public bool ParseRequest(byte[] requestBytes)
        {
            bool keepScanningHeaders = true;
            int scanPosition = 0;

            //Request Line
            int foundPosition = requestBytes.IndexOfSequence(ByteCRLR, scanPosition);
            if (foundPosition > 10) //minimum request line is like 12 bytes or something
            {
                //copy first (request) line for now and move on
                RequestLine = System.Text.Encoding.UTF8.GetString(requestBytes, 0, foundPosition);
                scanPosition = foundPosition + ByteCRLR.Length;

                //find headers
                while (keepScanningHeaders)
                {
                    foundPosition = requestBytes.IndexOfSequence(ByteCRLR, scanPosition);
                    if (foundPosition == scanPosition)
                    {   //we must have found the body = copy and we're done
                        foundPosition += ByteCRLR.Length;
                        Body = System.Text.Encoding.UTF8.GetString(requestBytes, foundPosition, requestBytes.Length - foundPosition);
                        keepScanningHeaders = false;
                        break;
                    }
                    else if (foundPosition > 0)
                    {
                        //found a header, now what?
                        int headerStartBytePosition = scanPosition;

                        foundPosition = scanPosition + ByteCRLR.Length;   //skip CRLF from above
                        int seekHeaderStopPosition = 0;
                        bool scanningHeader = true;
                        while (scanningHeader)
                        {
                            seekHeaderStopPosition = requestBytes.IndexOfSequence(ByteCRLR, foundPosition);
                            byte possibleEndOfHeaderByte = requestBytes[seekHeaderStopPosition + ByteCRLR.Length];
                            if (!char.IsWhiteSpace((char)possibleEndOfHeaderByte) || 
                                (requestBytes[seekHeaderStopPosition + ByteCRLR.Length] == ByteCR && requestBytes[seekHeaderStopPosition + ByteCRLR.Length + 1] == ByteLF))
                            {   //We have found the end of the Header
                                scanningHeader = false;
                            }
                            foundPosition = seekHeaderStopPosition + ByteCRLR.Length;
                        }

                        if (headerStartBytePosition < seekHeaderStopPosition)
                        {   // create header buffer text
                            string entireHeaderText = Encoding.ASCII.GetString(requestBytes, headerStartBytePosition, seekHeaderStopPosition - headerStartBytePosition);
                            int indexOfColon = entireHeaderText.IndexOf(':');
                            if (indexOfColon > 0)
                            {
                                string headerName = entireHeaderText.Substring(0, indexOfColon).Trim();
                                indexOfColon++; //skip the colon while we get the value
                                string headerValue = entireHeaderText.Substring(indexOfColon, entireHeaderText.Length - indexOfColon).Trim();

                                if (!Headers.ContainsKey(headerName))
                                    Headers[headerName] = new List<string>();
                                Headers[headerName].Add(headerValue);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Invalid SIP header {entireHeaderText} missing colon separator");
                            }
                        }
                        scanPosition = seekHeaderStopPosition + ByteCRLR.Length;
                    }
                    else
                    {
                        keepScanningHeaders = false;
                        break;
                    }
                }

            }
            return false;
        }

        public bool ParseHeaders(TRequestHandler handler, string[] lines)
        {
            throw new NotImplementedException();
        }

        public bool ParseRequestLine(TRequestHandler handler, string line)
        {
            throw new NotImplementedException();
        }


        /*
        // byte types don't have a data type annotation so we pre-cast them; to avoid in-place casts
        private const byte ByteCR = (byte)'\r';
        private const byte ByteLF = (byte)'\n';
        private const byte ByteColon = (byte)':';
        private const byte ByteSpace = (byte)' ';
        private const byte ByteTab = (byte)'\t';
        private const byte ByteQuestionMark = (byte)'?';
        private const byte BytePercentage = (byte)'%';

        //public unsafe bool ParseRequestLine(TRequestHandler handler, in ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        //{
        //    consumed = buffer.Start;
        //    examined = buffer.End;

        //    // Prepare the first span
        //    var span = buffer.First.Span;
        //    var lineIndex = span.IndexOf(ByteLF);
        //    if (lineIndex >= 0)
        //    {
        //        consumed = buffer.GetPosition(lineIndex + 1, consumed);
        //        span = span.Slice(0, lineIndex + 1);
        //    }
        //    else if (buffer.IsSingleSegment)
        //    {
        //        // No request line end
        //        return false;
        //    }
        //    else if (TryGetNewLine(buffer, out var found))
        //    {
        //        span = buffer.Slice(consumed, found).ToSpan();
        //        consumed = found;
        //    }
        //    else
        //    {
        //        // No request line end
        //        return false;
        //    }

        //    // Fix and parse the span
        //    fixed (byte* data = span)
        //    {
        //        ParseRequestLine(handler, data, span.Length);
        //    }

        //    examined = consumed;
        //    return true;
        //}


        public static SipMethod GetKnownMethod(string requestLine)
        {
            SipMethod retval;

            string[] splits = requestLine.Split(SipCharacters.RequestLineFieldSeparator);
            if (splits.Length != 3)
                RejectRequestLine(requestLine);

            retval = ConvertMethodString(splits[0]);

            return retval;
        }

        public static SipMethod ConvertMethodString(string method)
        {
            return (SipMethod)Enum.Parse(typeof(SipMethod), method, true);
        }

        private SipVersion GetKnownVersion(string requestLine)
        {
            SipVersion retval;

            string[] splits = requestLine.Split(SipCharacters.RequestLineFieldSeparator);
            if (splits.Length != 3)
                RejectRequestLine(requestLine);

            retval = ConvertVersionString(splits[2]);

            return retval;
        }

        private SipVersion ConvertVersionString(string sipVersion)
        {
            switch (sipVersion.ToUpper())
            {
                case "SIP/1.0":
                    return SipVersion.Sip10;
                case "SIP/2.0":
                    return SipVersion.Sip20;
                default:
                    throw new ArgumentException($"Invalid Version {nameof(sipVersion)}={sipVersion}");
            }
        }

        public string GetUnknownMethod(string requestLine)
        {
            string[] splits = requestLine.Split(SipCharacters.RequestLineFieldSeparator);
            if (splits.Length != 3)
                RejectRequestLine(requestLine);

            return splits[0];
        }


        //private unsafe void ParseRequestLine(TRequestHandler handler, byte* data, int length)
        public bool ParseRequestLine(TRequestHandler handler, string requestLine)
        {
            // Get Method and set the offset
            SipMethod sipMethod = GetKnownMethod(requestLine);
            SipVersion sipVersion = GetKnownVersion(requestLine);
            string targetUri = GetTargetUri(requestLine);

            //void OnStartLine(SipMethod method, SipVersion version, string target, string path, string query, string customMethod, bool pathEncoded);
            handler.OnStartLine(sipMethod, sipVersion, targetUri);
        }

        private string GetTargetUri(string requestLine)
        {
            string[] splits = requestLine.Split(SipCharacters.RequestLineFieldSeparator);
            if (splits.Length != 3)
                RejectRequestLine(requestLine);

            return splits[1];
        }

        public unsafe bool ParseHeaders(TRequestHandler handler, in ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined, out int consumedBytes)
        {
            consumed = buffer.Start;
            examined = buffer.End;
            consumedBytes = 0;

            var bufferEnd = buffer.End;

            var reader = new SequenceReader<byte>(buffer);
            var start = default(SequenceReader<byte>);
            var done = false;

            try
            {
                while (!reader.End)
                {
                    var span = reader.CurrentSpan;
                    var remaining = span.Length - reader.CurrentSpanIndex;

                    fixed (byte* pBuffer = span)
                    {
                        while (remaining > 0)
                        {
                            var index = reader.CurrentSpanIndex;
                            byte ch1;
                            byte ch2;
                            var readAhead = false;
                            var readSecond = true;

                            // Fast path, we're still looking at the same span
                            if (remaining >= 2)
                            {
                                ch1 = pBuffer[index];
                                ch2 = pBuffer[index + 1];
                            }
                            else
                            {
                                // Store the reader before we look ahead 2 bytes (probably straddling
                                // spans)
                                start = reader;

                                // Possibly split across spans
                                reader.TryRead(out ch1);
                                readSecond = reader.TryRead(out ch2);

                                readAhead = true;
                            }

                            if (ch1 == ByteCR)
                            {
                                // Check for final CRLF.
                                if (!readSecond)
                                {
                                    // Reset the reader so we don't consume anything
                                    reader = start;
                                    return false;
                                }
                                else if (ch2 == ByteLF)
                                {
                                    // If we got 2 bytes from the span directly so skip ahead 2 so that
                                    // the reader's state matches what we expect
                                    if (!readAhead)
                                    {
                                        reader.Advance(2);
                                    }

                                    done = true;
                                    handler.OnHeadersComplete();
                                    return true;
                                }

                                // Headers don't end in CRLF line.
                                BadSipRequestException.Throw(RequestRejectionReason.InvalidRequestHeadersNoCRLF);
                            }

                            // We moved the reader so look ahead 2 bytes so reset both the reader
                            // and the index
                            if (readAhead)
                            {
                                reader = start;
                                index = reader.CurrentSpanIndex;
                            }

                            var endIndex = new Span<byte>(pBuffer + index, remaining).IndexOf(ByteLF);
                            var length = 0;

                            if (endIndex != -1)
                            {
                                length = endIndex + 1;
                                var pHeader = pBuffer + index;

                                TakeSingleHeader(pHeader, length, handler);
                            }
                            else
                            {
                                var current = reader.Position;
                                var currentSlice = buffer.Slice(current, bufferEnd);

                                var lineEndPosition = currentSlice.PositionOf(ByteLF);
                                // Split buffers
                                if (lineEndPosition == null)
                                {
                                    // Not there
                                    return false;
                                }

                                var lineEnd = lineEndPosition.Value;

                                // Make sure LF is included in lineEnd
                                lineEnd = buffer.GetPosition(1, lineEnd);
                                var headerSpan = buffer.Slice(current, lineEnd).ToSpan();
                                length = headerSpan.Length;

                                fixed (byte* pHeader = headerSpan)
                                {
                                    TakeSingleHeader(pHeader, length, handler);
                                }

                                // We're going to the next span after this since we know we crossed spans here
                                // so mark the remaining as equal to the headerSpan so that we end up at 0
                                // on the next iteration
                                remaining = length;
                            }

                            // Skip the reader forward past the header line
                            reader.Advance(length);
                            remaining -= length;
                        }
                    }
                }

                return false;
            }
            finally
            {
                consumed = reader.Position;
                consumedBytes = (int)reader.Consumed;

                if (done)
                {
                    examined = consumed;
                }
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private unsafe int FindEndOfName(byte* headerLine, int length)
        //{
        //    var index = 0;
        //    var sawWhitespace = false;
        //    for (; index < length; index++)
        //    {
        //        var ch = headerLine[index];
        //        if (ch == ByteColon)
        //        {
        //            break;
        //        }
        //        if (ch == ByteTab || ch == ByteSpace || ch == ByteCR)
        //        {
        //            sawWhitespace = true;
        //        }
        //    }

        //    if (index == length || sawWhitespace)
        //    {
        //        RejectRequestHeader(headerLine, length);
        //    }

        //    return index;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void TakeSingleHeader(byte* headerLine, int length, TRequestHandler handler)
        {
            // Skip CR, LF from end position
            var valueEnd = length - 3;
            var nameEnd = FindEndOfName(headerLine, length);

            // Header name is empty
            if (nameEnd == 0)
            {
                RejectRequestHeader(headerLine, length);
            }

            if (headerLine[valueEnd + 2] != ByteLF)
            {
                RejectRequestHeader(headerLine, length);
            }
            if (headerLine[valueEnd + 1] != ByteCR)
            {
                RejectRequestHeader(headerLine, length);
            }

            // Skip colon from value start
            var valueStart = nameEnd + 1;
            // Ignore start whitespace
            for (; valueStart < valueEnd; valueStart++)
            {
                var ch = headerLine[valueStart];
                if (ch != ByteTab && ch != ByteSpace && ch != ByteCR)
                {
                    break;
                }
                else if (ch == ByteCR)
                {
                    RejectRequestHeader(headerLine, length);
                }
            }

            // Check for CR in value
            var valueBuffer = new Span<byte>(headerLine + valueStart, valueEnd - valueStart + 1);
            if (valueBuffer.Contains(ByteCR))
            {
                RejectRequestHeader(headerLine, length);
            }

            // Ignore end whitespace
            var lengthChanged = false;
            for (; valueEnd >= valueStart; valueEnd--)
            {
                var ch = headerLine[valueEnd];
                if (ch != ByteTab && ch != ByteSpace)
                {
                    break;
                }

                lengthChanged = true;
            }

            if (lengthChanged)
            {
                // Length changed
                valueBuffer = new Span<byte>(headerLine + valueStart, valueEnd - valueStart + 1);
            }

            var nameBuffer = new Span<byte>(headerLine, nameEnd);

            handler.OnHeader(nameBuffer, valueBuffer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryGetNewLine(in ReadOnlySequence<byte> buffer, out SequencePosition found)
        {
            var byteLfPosition = buffer.PositionOf(ByteLF);
            if (byteLfPosition != null)
            {
                // Move 1 byte past the \n
                found = buffer.GetPosition(1, byteLfPosition.Value);
                return true;
            }

            found = default;
            return false;
        }

        //[StackTraceHidden]
        private void RejectRequestLine(string requestLine)
            => throw GetInvalidRequestException(RequestRejectionReason.InvalidRequestLine, requestLine);

        //[StackTraceHidden]
        //private void RejectRequestHeader(byte* headerLine, int length)
        //    => throw GetInvalidRequestException(RequestRejectionReason.InvalidRequestHeader, headerLine, length);

        //[StackTraceHidden]
        //private void RejectUnknownVersion(byte* version, int length)
        //    => throw GetInvalidRequestException(RequestRejectionReason.UnrecognizedSipVersion, version, length);

        //[MethodImpl(MethodImplOptions.NoInlining)]
        private BadSipRequestException GetInvalidRequestException(RequestRejectionReason reason, string detail)
            => BadSipRequestException.GetException(
                reason,
                _showErrorDetails
                    ? detail
                    : string.Empty);*/
    }
}
