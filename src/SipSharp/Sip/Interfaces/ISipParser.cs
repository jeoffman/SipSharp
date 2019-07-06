namespace SipSharp.Sip.Interfaces
{
    public interface ISipParser<TRequestHandler> where TRequestHandler : ISipHeadersHandler, ISipRequestLineHandler
    {
        bool ParseRequestLine(TRequestHandler handler, string line);

        bool ParseHeaders(TRequestHandler handler, string[] lines);
    }
}
