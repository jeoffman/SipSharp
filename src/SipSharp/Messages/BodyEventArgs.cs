﻿using System;
//using Fadd;

namespace SipSharp.Transports.Parser
{
    /// <summary>
    /// Arguments used when more body bytes have come.
    /// </summary>
    public class BodyEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BodyEventArgs"/> class.
        /// </summary>
        /// <param name="buffer">buffer that contains the received bytes.</param>
        /// <param name="offset">offset in buffer where to start processing.</param>
        /// <param name="count">number of bytes from <paramref name="offset"/> that should be parsed.</param>
        public BodyEventArgs(byte[] buffer, int offset, int count)
        {
            //Check.Require(buffer, "buffer");
            if (buffer == null)
                throw new ArgumentNullException($"{nameof(buffer)}");
            //Check.Min(1, offset, "offset");
            if (offset < 1)
                throw new ArgumentOutOfRangeException($"{nameof(offset)} must be >= 1");
            //Check.Min(1, count, "count");
            if (count < 1)
                throw new ArgumentOutOfRangeException($"{nameof(count)} must be >= 1");

            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyEventArgs"/> class.
        /// </summary>
        public BodyEventArgs()
        {
        }

        /// <summary>
        /// Gets or sets buffer that contains the received bytes.
        /// </summary>
        public byte[] Buffer { get; internal set; }

        /*
        /// <summary>
        /// Gets or sets number of bytes used by the request.
        /// </summary>
        public int BytesUsed { get; set; }
        */

        /// <summary>
        /// Gets or sets number of bytes from <see cref="Offset"/> that should be parsed.
        /// </summary>
        public int Count { get; internal set; }

        /*
        /// <summary>
        /// Gets or sets whether the body is complete.
        /// </summary>
        public bool IsBodyComplete { get; set; }
        */

        /// <summary>
        /// Gets or sets offset in buffer where to start processing.
        /// </summary>
        public int Offset { get; internal set; }

        internal void AssignInternal(byte[] bytes, int offset, int count)
        {
            Buffer = bytes;
            Offset = offset;
            Count = count;
        }
    }
}