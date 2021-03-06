﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Tools;

namespace SipSharp.Transports
{
    /// <summary>
    /// Transports messages over UDP.
    /// </summary>
    public class UdpTransport : ITransport
    {
        public IPAddress Address { get; set; }
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (UdpTransport));
        private readonly IPEndPoint _listeningPoint;
        private readonly MessageFactory _parsers;
        private Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpTransport"/> class.
        /// </summary>
        /// <param name="listeningPoint">End point to accept new connections on.</param>
        /// <param name="parsers">The parsers.</param>
        public UdpTransport(IPEndPoint listeningPoint, MessageFactory parsers)
        {
            _listeningPoint = listeningPoint;
            _parsers = parsers;
        }

        /// <summary>
        /// Create a new UDP socket.
        /// </summary>
        /// <returns>Created socket</returns>
        protected virtual Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        /// <summary>
        /// Received a message from remote end point.
        /// </summary>
        /// <param name="ar"></param>
        private void OnRead(IAsyncResult ar)
        {
            var buffer = (byte[]) ar.AsyncState;
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 3929);
            int bytesRead = 0;

            bool isKeepAlive = true;

            try
            {
                _logger.Trace("_socket.EndReceiveFrom");
                bytesRead = _socket.EndReceiveFrom(ar, ref endPoint);
                if (bytesRead <= 4)
                {
                    for (int i = 0; i < bytesRead; ++i)
                        if (buffer[i] != '\r' && buffer[i] != '\n')
                        {
                            isKeepAlive = false;
                            break;
                        }
                }
                else
                    isKeepAlive = false;

                if (!isKeepAlive)
                    _logger.Debug("Received " + bytesRead + " bytes from " + endPoint + ":\r\n" +
                                  Encoding.ASCII.GetString(buffer, 0, bytesRead));
            }
            catch (Exception err)
            {
                _logger.Warning("EndReceiveFrom failed: " + err);
            }


            // begin receiving another packet before starting to process this one
            byte[] newBuffer = BufferPool.Dequeue();

            try
            {
                EndPoint localEndPoint = new IPEndPoint(_listeningPoint.Address, _listeningPoint.Port);
                _socket.BeginReceiveFrom(newBuffer, 0, newBuffer.Length, SocketFlags.None,
                                         ref localEndPoint, OnRead, newBuffer);
            }
            catch (Exception err)
            {
                _logger.Warning("BeginReceiveFrom failed, closing socket. Exception: " + err);
                BufferPool.Enqueue(newBuffer);
                BufferPool.Enqueue(buffer);
                _socket.Close();
                return;
            }

            if (bytesRead == 0 || isKeepAlive)
            {
                BufferPool.Enqueue(buffer);
                return;
            }

            // Parse buffer.
            MessageFactoryContext factoryContext = _parsers.CreateNewContext(endPoint);
            try
            {
                int offset = factoryContext.Parse(buffer, 0, bytesRead);
                if (offset != bytesRead)
                    _logger.Error("Failed to parse complete message");
            }
            finally
            {
                BufferPool.Enqueue(buffer);
                _parsers.Release(factoryContext);
            }
        }


        private void OnSendComplete(IAsyncResult ar)
        {
            var context = (SendContext) ar.AsyncState;
            int bytesSent = _socket.EndSendTo(ar);
            BufferPool.Enqueue(context.buffer);
            _logger.Trace("OnSendComplete, " + bytesSent + " sent ");
        }

        #region ITransport Members

        /// <summary>
        /// Start transport.
        /// </summary>
        /// <exception cref="ArgumentException"><see cref="EndPoint"/> is not of the type expected by the transport implementation</exception>
        /// <exception cref="ArgumentNullException"><c>endPoint</c> is null.</exception>
        public void Start()
        {
            if (BufferPool == null)
                BufferPool = new ObjectPool<byte[]>(() => new byte[65535]);

            byte[] buffer = BufferPool.Dequeue();
            _socket = CreateSocket();
            _socket.Bind(_listeningPoint);

            _logger.Trace("BeginReceiveFrom");
            EndPoint ep = new IPEndPoint(_listeningPoint.Address, _listeningPoint.Port);
            _socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref ep, OnRead, buffer);
        }

        public void Send(EndPoint endPoint, byte[] buffer, int offset, int count)
        {
            _logger.Debug("Sending " + count + " bytes to " + endPoint + ":\r\n" +
                          Encoding.ASCII.GetString(buffer, 0, count));
            var context = new SendContext(endPoint, buffer);
            _socket.BeginSendTo(buffer, 0, count, SocketFlags.None, endPoint, OnSendComplete,
                                context);
        }

        /// <summary>
        /// Gets protocol used by this transporter.
        /// </summary>
        public string Protocol
        {
            get { return "UDP"; }
        }

        /// <summary>
        /// Gets or sets listening end point.
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get { return _listeningPoint; }
        }

        /// <summary>
        /// Gets port that the point is listening on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets of protocol is message based.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Message based protocols like UDP should only receive one (and a complete) message
        /// in each receive. While packet based protocols like TCP can receive partial, complete or multiple
        /// messages in one packet.
        /// </para>
        /// <para>This property should be used to </para>
        /// </remarks>
        //string IsMessageBasedProtocl{ get;}
        public ObjectPool<byte[]> BufferPool { set; private get; }

        public event UnhandledExceptionEventHandler UnhandledException = delegate { };

        #endregion

        #region Nested type: SendContext

        private struct SendContext
        {
            public readonly byte[] buffer;
            public readonly EndPoint endPoint;

            public SendContext(EndPoint endPoint, byte[] buffer)
            {
                this.endPoint = endPoint;
                this.buffer = buffer;
            }
        }

        #endregion
    }
}