namespace SipSharp.Sip.Interfaces
{
    public interface ISipRequestLineHandler
    {
        void OnStartLine(SipMethod method, SipVersion version, string targetUri);
    }
}
