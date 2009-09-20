﻿using System;
using System.Collections.Generic;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Receives messages from the transport layer
    /// and matches them with transactions.
    /// </summary>
    internal class TransactionManager
    {
        private readonly Dictionary<string, IClientTransaction> _clientTransactions =
            new Dictionary<string, IClientTransaction>();
        private readonly Dictionary<string, IServerTransaction> _serverTransactions =
            new Dictionary<string, IServerTransaction>();

        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (TransactionManager));

        /// <summary>
        /// Estimated round-trip time (RTT)
        /// </summary>
        public const int T1 = 500;

        /// <summary>
        /// Max retransmit time.
        /// </summary>
        public const int T2 = 4000;

        /// <summary>
        /// The amount of time the network will take to clear messages between client and
        /// server transactions
        /// </summary>
        public const int T4 = 5000;

        private readonly ISipStack _sipStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        /// <param name="sipStack">The sip stack.</param>
        public TransactionManager(ISipStack sipStack)
        {
            _sipStack = sipStack;
        }

        public IClientTransaction CreateClientTransaction(IRequest message)
        {
            IClientTransaction transaction;
            if (message.Method == SipMethod.INVITE)
                transaction = new ClientInviteTransaction(_sipStack, message);
            else
                transaction = new ClientNonInviteTransaction(_sipStack, message);

            _clientTransactions.Add(transaction.Id, transaction);
            transaction.Terminated += OnTerminated;
            return transaction;
        }

        private void OnTerminated(object sender, EventArgs e)
        {
            if (sender is IClientTransaction)
                _clientTransactions.Remove(((IClientTransaction)sender).Id);
            else
                _serverTransactions.Remove(((IServerTransaction)sender).Id);
        }

        public IServerTransaction CreateServerTransaction(IRequest request)
        {
            IServerTransaction transaction;
            if (request.Method == SipMethod.INVITE)
                transaction = new ServerInviteTransaction(_sipStack, request);
            else
                transaction = new ServerNonInviteTransaction(_sipStack, request);

            _serverTransactions.Add(transaction.Id, transaction);
            transaction.Terminated += OnTerminated;
            return transaction;
        }

        public bool Process(IResponse response)
        {
            // A response matches a client transaction under two conditions:

            //  1.  If the response has the same value of the branch parameter in
            //      the top Via header field as the branch parameter in the top
            //      Via header field of the request that created the transaction.

            //  2.  If the method parameter in the CSeq header field matches the
            //      method of the request that created the transaction.  The
            //      method is needed since a CANCEL request constitutes a
            //      different transaction, but shares the same value of the branch
            //      parameter.        
            string bransch = response.Via.First.Branch;
            string method = response.CSeq.Method;
            string transactionId = bransch + "-" + method;

            IClientTransaction transaction;
            lock (_clientTransactions)
            {
                if (!_clientTransactions.TryGetValue(transactionId, out transaction))
                {
                    _logger.Warning("Response do not match a transaction: " + response);
                    // did not match a transaction.
                    return false;
                }
            }

            return transaction.Process(response, null);
        }

        public IServerTransaction Process(IRequest request)
        {
            // The branch parameter in the topmost Via header field of the request
            // is examined.  If it is present and begins with the magic cookie
            // "z9hG4bK", the request was generated by a client transaction
            // compliant to this specification.  Therefore, the branch parameter
            // will be unique across all transactions sent by that client.  The
            // request matches a transaction if:
            // 
            //    1. the branch parameter in the request is equal to the one in the
            //       top Via header field of the request that created the
            //       transaction, and

            //    2. the sent-by value in the top Via of the request is equal to the
            //       one in the request that created the transaction, and

            //    3. the method of the request matches the one that created the
            //       transaction, except for ACK, where the method of the request
            //       that created the transaction is INVITE.

            string token = request.Via.First.Branch;
            token += request.Via.First.SentBy;

            if (request.Method == SipMethod.ACK)
                token += SipMethod.INVITE;
            else
                token += request.Method;

            IServerTransaction transaction;
            bool isNew = false;
            lock (_serverTransactions)
            {
                if (!_serverTransactions.TryGetValue(token, out transaction))
                {
                    // Failed to find it. add it.
                    if (request.Method == SipMethod.INVITE)
                        transaction = new ServerInviteTransaction(_sipStack, request);
                    else
                        transaction = new ServerNonInviteTransaction(_sipStack, request);
                    isNew = true;
                }
            }

            transaction.Process(request);
            if (isNew)
                ServerTransactionCreated(this, new TransactionEventArgs(transaction));

            return transaction;
        }

        public event EventHandler<TransactionEventArgs> ClientTransactionCreated = delegate { };
        public event EventHandler<TransactionEventArgs> ServerTransactionCreated = delegate { };
    }
}