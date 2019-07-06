namespace SipSharp.Sip.Interfaces
{
    /// <summary>Loosely modeled after  AspNetCore/src/Servers/Kestrel/Core/src/Internal/Http/IHttpHeadersHandler.cs </summary>
    public interface ISipHeadersHandler
    {
        void OnHeader(string name, string value);
        void OnHeadersComplete();
    }
}
