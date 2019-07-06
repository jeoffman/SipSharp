using SipSharp.Sip.Interfaces;

namespace SipSharp.Test
{
    internal class SipHandler : ISipHeadersHandler, ISipRequestLineHandler
    {
        public void OnStartLine(SipMethod method, SipVersion version, string targetUri)
        {
        }

        public void OnHeader(string name, string value)
        {
        }

        public void OnHeadersComplete()
        {
        }
    }
}
