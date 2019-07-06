﻿using System.Text;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using Xunit;

namespace SipSharp.Test.Messages
{
    public class MessageFactoryTest
    {
        private readonly MessageFactory _factory;
        private readonly HeaderFactory _headerFactory;
        private IRequest _request;
        private IResponse _response;

        public MessageFactoryTest()
        {
            LogFactory.Assign(new ConsoleLogFactory(null));

            _headerFactory = new HeaderFactory();
            _headerFactory.AddDefaultParsers();
            _factory = new MessageFactory(_headerFactory);
            _factory.RequestReceived += OnRequest;
            _factory.ResponseReceived += OnResponse;
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            _request = e.Request;
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            _response = e.Response;
        }

        private void Parse(MessageFactoryContext context, string message)
        {
            Parse(context, message, 0);
        }

        private void Parse(MessageFactoryContext context, string message, int offset)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            if (offset == 0)
                context.Parse(bytes, 0, bytes.Length);
            else
            {
                context.Parse(bytes, 0, offset);
                context.Parse(bytes, offset, bytes.Length - offset);
            }
        }

        [Fact(Skip = "I don't think this ever worked")]
        private void TestTortousInvite()
        {
            MessageFactoryContext context = _factory.CreateNewContext(null);
            Parse(context, TestMessages.AShortTortuousINVITE);
            Assert.NotNull(_request);
            Assert.Equal("chair-dnrc.example.com", _request.Uri.Domain);
            Assert.Equal("1918181833n", _request.To.Parameters["tag"]);

            Via via = _request.Via;
            Assert.Equal(3, via.Items.Count);
            Assert.Equal("390skdjuw", via.Items[0].Branch);
            Assert.Equal("SIP/2.0", via.Items[0].SipVersion);
            Assert.Equal("192.168.255.111", via.Items[2].Domain);
        }
    }
}