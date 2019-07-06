using SipSharp.Sip;
using SipSharp.Test.Messages;
using System.Text;
using Xunit;

namespace SipSharp.Test
{
    public class SipParserTests
    {
        [Fact]
        public void MyVeryFirstSipParserTest()
        {
            SipParser<SipHandler> parser = new SipParser<SipHandler>();

            byte[] messageBytes = Encoding.ASCII.GetBytes(TestMessages.AShortTortuousINVITE);

            parser.ParseRequest(messageBytes);
        }
    }
}