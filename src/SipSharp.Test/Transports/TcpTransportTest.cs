using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Tools;
using SipSharp.Transports;
using Xunit;

namespace SipSharp.Test.Transports
{
    public class TcpTransportTestFixture
    {
        public TcpTransportTestFixture()
        {
            LogFactory.Assign(new ConsoleLogFactory(null));
        }
    }

    public class TcpTransportTest: IClassFixture<TcpTransportTestFixture>
    {
        TcpTransportTestFixture _fixture;

        private readonly ObjectPool<byte[]> _bufferPool = new ObjectPool<byte[]>(() => new byte[4096]);
        private readonly ManualResetEvent _manualEvent = new ManualResetEvent(false);
        private readonly MessageFactory _messageFactory;
        private readonly TcpTransport _transport;
        private int _receivedLength;
        private byte[] _recievedBuffer;
        private IRequest _request;
        private IResponse _response;
        public int ServerPort { get; set; }

        public TcpTransportTest(TcpTransportTestFixture fixture)
        {
            _fixture = fixture;

            ServerPort = FreeTcpPort();

            var headerFactory = new HeaderFactory();
            headerFactory.AddDefaultParsers();
            _messageFactory = new MessageFactory(headerFactory);
            _messageFactory.RequestReceived += OnRequest;
            _messageFactory.ResponseReceived += OnResponse;
            var pool = new ObjectPool<byte[]>(CreateBuffer);
            _transport = new TcpTransport(new IPEndPoint(IPAddress.Any, ServerPort), _messageFactory)
            { BufferPool = _bufferPool };
            _transport.Start();
            _transport.UnhandledException += OnUnhandledException;
        }

        private static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private byte[] CreateBuffer()
        {
            return new byte[4196];
        }

        private void OnNewSocket(IAsyncResult ar)
        {
            var listener = (TcpListener) ar.AsyncState;
            Socket socket = listener.EndAcceptSocket(ar);

            _recievedBuffer = new byte[65535];
            _receivedLength = socket.Receive(_recievedBuffer, 0, 65535, SocketFlags.None);
            _manualEvent.Set();
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            Assert.Equal(_request.To, e.Request.To);
            Assert.Equal(_request.From, e.Request.From);
            Assert.Equal(_request.Contact, e.Request.Contact);
            Assert.Equal(_request.CallId, e.Request.CallId);
            Assert.Equal(_request.CSeq, e.Request.CSeq);
            Assert.Equal(_request.MaxForwards, e.Request.MaxForwards);
            _manualEvent.Set();
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            Assert.False(true);
        }

        [Fact]
        private void ReceiveRequest()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Loopback, ServerPort);

            _request = new Request("INVITE", "sips:jonas@gauffin.com", "SIP/2.0")
                           {
                               From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060)),
                               To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com")),
                               CallId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                               CSeq = new CSeq(1, "_request.Method"),
                               MaxForwards = 60
                           };
            _request.Contact = _request.To;


            byte[] buffer = _bufferPool.Dequeue();
            var serializer = new MessageSerializer();
            int length = serializer.Serialize(_request, buffer);
            socket.Send(buffer, 0, length, SocketFlags.None);
            Assert.True(_manualEvent.WaitOne(1000));
        }

        [Fact]
        private void SendRequest()
        {
            _request = new Request("INVITE", "sips:jonas@gauffin.com", "SIP/2.0")
                           {
                               From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060)),
                               To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com")),
                               CallId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                               MaxForwards = 60,
                           };
            _request.CSeq = new CSeq(1, _request.Method);
            _request.Contact = _request.To;

            int somePort = FreeTcpPort();
            var listener = new TcpListener(IPAddress.Loopback, somePort);
            listener.Start();
            listener.BeginAcceptSocket(OnNewSocket, listener);

            byte[] buffer = _bufferPool.Dequeue();
            var serializer = new MessageSerializer();
            int length = serializer.Serialize(_request, buffer);
            _transport.Send(new IPEndPoint(IPAddress.Loopback, somePort), buffer, 0, length);

            Assert.True(_manualEvent.WaitOne(1000));
        }

        [Fact]
        private void SendResponse()
        {
            _response = new Response("SIP/2.0", StatusCodes.OK, "OK!")
                            {
                                From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060)),
                                To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com")),
                                CallId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                            };
            _response.CSeq = new CSeq(1, "INVITE");
            //_response.Contact = _request.To;

            int somePort = FreeTcpPort();
            var listener = new TcpListener(IPAddress.Loopback, somePort);
            listener.Start();
            listener.BeginAcceptSocket(OnNewSocket, listener);

            byte[] buffer = _bufferPool.Dequeue();
            var serializer = new MessageSerializer();
            int length = serializer.Serialize(_response, buffer);
            _transport.Send(new IPEndPoint(IPAddress.Loopback, somePort), buffer, 0, length);

            Assert.True(_manualEvent.WaitOne(1000));
        }
    }
}