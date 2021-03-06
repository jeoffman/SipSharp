﻿using System;
using System.Threading;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    internal class ServerInviteTransaction : IServerTransaction
    {
        private readonly IRequest _request;

        /// <summary>
        /// Retransmission timer.
        /// </summary>
        private readonly Timer _timerG;

        /// <summary>
        /// Timeout, the amount of time UAC tries to resend the request.
        /// </summary>
        private readonly Timer _timerH;

        /// <summary>
        /// When to switch to terminated state.
        /// </summary>
        private readonly Timer _timerI;

        private readonly ITransportLayer _transport;

        private IResponse _response;

        private int _timerGValue;


        public ServerInviteTransaction(ITransportLayer transport, IRequest request)
        {
            _transport = transport;
            _request = request;
            State = TransactionState.Proceeding;
            _timerG = new Timer(OnRetransmit);
            _timerH = new Timer(OnTimeout);
            _timerI = new Timer(OnTerminated);
            _timerGValue = TransactionManager.T1;

            if (request.Method == "ACK")
                throw new InvalidOperationException("Expected any other type than ACK and INVITE");

            // The server transaction MUST generate a 100
            // (Trying) response unless it knows that the TU will generate a
            // provisional or final response within 200 ms, in which case it MAY
            // generate a 100 (Trying) response
            // Send(request.CreateResponse(StatusCode.Trying, "We are trying here..."));
        }

        private void OnRetransmit(object state)
        {
            _timerGValue = Math.Min(_timerGValue*2, TransactionManager.T2);
            _timerG.Change(_timerGValue, Timeout.Infinite);
            _transport.Send(_response);
        }

        private void OnTerminated(object state)
        {
            Terminate(true);
        }

        private void OnTimeout(object state)
        {
            // If timer H fires while in the "Completed" state, it implies that the
            // ACK was never received.  In this case, the server transaction MUST
            // transition to the "Terminated" state, and MUST indicate to the TU
            // that a transaction failure has occurred.
            Terminate(false);
            TimedOut(this, EventArgs.Empty);
            //TODO: Should we have a TimedOut event too?
        }

        private void Terminate(bool triggerEvent)
        {
            State = TransactionState.Terminated;
            if (triggerEvent)
                Terminated(this, EventArgs.Empty);
            _timerG.Change(Timeout.Infinite, Timeout.Infinite);
            _timerH.Change(Timeout.Infinite, Timeout.Infinite);
            _timerI.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #region IServerTransaction Members

        /// <summary>
        /// The request have been retransmitted by the UA.
        /// </summary>
        /// <param name="request"></param>
        public void Process(IRequest request)
        {
            if (_response == null)
                return; //ops. maybe show an error?
            if (State == TransactionState.Terminated)
                return;

            // If an ACK is received while the server transaction is in the
            // "Completed" state, the server transaction MUST transition to the
            // "Confirmed" state.  As Timer G is ignored in this state, any
            // retransmissions of the response will cease.
            if (request.Method == "ACK")
            {
                if (State == TransactionState.Completed)
                    State = TransactionState.Confirmed;
                _timerG.Change(Timeout.Infinite, Timeout.Infinite);
                _timerI.Change(TransactionManager.T4, Timeout.Infinite);
            }

            // If a request
            // retransmission is received while in the "Proceeding" state, the most
            // recent provisional response that was received from the TU MUST be
            // passed to the transport layer for retransmission.
            if (State == TransactionState.Proceeding)
                _transport.Send(_response);

            // Furthermore,
            // while in the "Completed" state, if a request retransmission is
            // received, the server SHOULD pass the response to the transport for
            // retransmission.
            if (State == TransactionState.Completed)
                _transport.Send(_response);
        }

        /// <summary>
        /// Gets request that created the transaction.
        /// </summary>
        public IRequest Request
        {
            get { return _request; }
        }

        public void Send(IResponse response)
        {
            // Any other final responses passed by the TU to the server
            // transaction MUST be discarded while in the "Completed" state.
            if (State == TransactionState.Completed || State == TransactionState.Terminated)
                return;

            _response = response;

            // While in the "Trying" state, if the TU passes a provisional response
            // to the server transaction, the server transaction MUST enter the
            // "Proceeding" state.  
            if (State == TransactionState.Trying && StatusCodeHelper.Is1xx(response))
                State = TransactionState.Proceeding;

            // The response MUST be passed to the transport
            // layer for transmission.  Any further provisional responses that are
            // received from the TU while in the "Proceeding" state MUST be passed
            // to the transport layer for transmission.  

            // If the TU passes a final response (status
            // codes 200-699) to the server while in the "Proceeding" state, the
            // transaction MUST enter the "Completed" state, and the response MUST
            // be passed to the transport layer for transmission.
            if (State == TransactionState.Proceeding)
            {
                if (StatusCodeHelper.Is2xx(response))
                {
                    // retransmissions of 2xx
                    // responses are handled by the TU.  The server transaction MUST then
                    // transition to the "Terminated" state.
                    _transport.Send(response);
                    Terminate(true);
                }
                else if (StatusCodeHelper.Is3456xx(response))
                {
                    _transport.Send(response);
                    State = TransactionState.Completed;
                    if (!response.IsReliableProtocol)
                        _timerG.Change(TransactionManager.T1, Timeout.Infinite);
                    _timerH.Change(64*TransactionManager.T1, Timeout.Infinite);
                }
                if (!StatusCodeHelper.Is1xx(response))
                {
                    State = TransactionState.Completed;
                }
            }

            _response = response;
        }

        /// <summary>
        /// Gets dialog that the transaction belongs to
        /// </summary>
        //IDialog Dialog { get; }
        /// <summary>
        /// Gets transaction identifier.
        /// </summary>
        public string Id
        {
            get { return _request.GetTransactionId(); }
        }

        /// <summary>
        /// Gets dialog that the transaction belongs to
        /// </summary>
        //IDialog Dialog { get; }
        /// <summary>
        /// Gets current transaction state
        /// </summary>
        public TransactionState State { get; private set; }

        /// <summary>
        /// Ack was never received from client.
        /// </summary>
        public event EventHandler Terminated = delegate { };

        #endregion

        /// <summary>
        /// Transaction timed out.
        /// </summary>
        public event EventHandler TimedOut = delegate { };
    }
}