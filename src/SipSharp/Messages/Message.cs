﻿using System.IO;
using SipSharp.Headers;
using SipSharp.Messages.Headers;

namespace SipSharp.Messages
{
    public abstract class Message : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        protected Message()
        {
            Body = new MemoryStream();
            Headers = new HeaderKeyValueCollection();
        }

        protected Message(IMessage message)
        {
            Headers = new HeaderKeyValueCollection();
            foreach (var header in message.Headers)
                Headers.Add(header.Key, (IHeader) header.Value.Clone());

            SipVersion = message.SipVersion;
            To = (Contact) message.To.Clone();
            From = (Contact) message.From.Clone();
            CSeq = (CSeq) message.CSeq.Clone();
            Via = (Via) message.Via.Clone();
            IsReliableProtocol = message.IsReliableProtocol;
            ContentLength = message.ContentLength;
        }

        /// <summary>
        /// Assign a header
        /// </summary>
        /// <param name="name">Long name, in lower case.</param>
        /// <param name="header">Header to assign</param>
        internal virtual void Assign(string name, IHeader header)
        {
            switch (name)
            {
                case "to":
                    To = ((ContactHeader) header).FirstContact;
                    break;
                case "from":
                    From = ((ContactHeader) header).FirstContact;
                    break;
                case "cseq":
                    CSeq = (CSeq) header;
                    break;
                case "via":
                    var via = (Via) header;
                    if (Via != null && Via.Items.Count > 0)
                    {
                        foreach (ViaEntry entry in via)
                            Via.Add(entry);
                    }
                    else
                        Via = via;
                    break;
                case "call-id":
                    CallId = header.ToString();
                    break;
            }

            Headers.Add(name.ToLower(), header);
        }

        #region IMessage Members

        /// <summary>
        /// Validate all mandatory headers.
        /// </summary>
        /// <exception cref="BadRequestException">A header is invalid/missing.</exception>
        public abstract void Validate();

        /// <summary>
        /// Gets body stream.
        /// </summary>
        public Stream Body { get; private set; }

        /// <summary>
        /// All headers.
        /// </summary>
        public IKeyValueCollection<string, IHeader> Headers { get; private set; }

        /// <summary>
        /// Gets or sets used version of the SIP protocol.
        /// </summary>
        public string SipVersion { get; set; }

        /// <summary>
        /// Gets or sets whom the request is intended for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The To header field first and foremost specifies the desired "logical"
        /// recipient of the request, or the address-of-record of the user or resource that
        /// is the target of this request. This may or may not be the ultimate recipient of
        /// the request. The To header field MAY contain a SIP or SIPS URI, but it may also
        /// make use of other URI schemes (the tel URL (RFC 2806 [9]), for example) when
        /// appropriate. All SIP implementations MUST support the SIP URI scheme. Any
        /// implementation that supports TLS MUST support the SIPS URI scheme. The To
        /// header field allows for a display name.
        /// </para>
        /// </remarks>
        /// 
        public Contact To { get; set; }

        /// <summary>
        /// Gets or sets whom the request is from.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The From header field indicates the logical identity of the initiator of the
        /// request, possibly the user's address-of-record. Like the To header field, it
        /// contains a URI and optionally a display name. It is used by SIP elements to
        /// determine which processing rules to apply to a request (for example, automatic
        /// call rejection). As such, it is very important that the From URI not contain IP
        /// addresses or the FQDN of the host on which the UA is running, since these are
        /// not logical names.
        /// </para><para>
        /// The From header field allows for a display name. A UAC SHOULD use the display
        /// name "Anonymous", along with a syntactically correct, but otherwise meaningless
        /// URI (like sip:thisis@anonymous.invalid), if the identity of the client is to
        /// remain hidden.
        /// </para><para>
        /// Usually, the value that populates the From header field in
        /// requests generated by a particular UA is pre-provisioned by the user or by the
        /// administrators of the user's local domain. If a particular UA is used by
        /// multiple users, it might have switchable profiles that include a URI
        /// corresponding to the identity of the profiled user. Recipients of requests can
        /// authenticate the originator of a request in order to ascertain that they are
        /// who their From header field claims they are (see Section 22 for more on
        /// authentication).
        /// </para>
        /// </remarks>
        public Contact From { get; set; }

        /// <summary>
        /// Gets or sets transaction identifier.
        /// </summary>
        /// <remarks>
        /// The CSeq header field serves as a way to identify and order transactions. It
        /// consists of a sequence number and a method. The method MUST match that of the
        /// request. For non-REGISTER requests outside of a dialog, the sequence number
        /// value is arbitrary. The sequence number value MUST be expressible as a 32-bit
        /// unsigned integer and MUST be less than 2**31. As long as it follows the above
        /// guidelines, a client may use any mechanism it would like to select CSeq header
        /// field values.
        /// </remarks>
        public CSeq CSeq { get; set; }

        /// <summary>
        /// Gets proxies that the message have to pass through.
        /// </summary>
        /// <remarks>
        /// <para>The Via header field indicates the transport used for the transaction and
        /// identifies the location where the message is to be sent. A Via header field
        /// value is added only after the transport that will be used to reach the next hop
        /// has been selected.</para>
        /// </remarks>
        public Via Via { get; internal set; }

        /// <summary>
        /// Gets whether the response is sent over a reliable protocol
        /// </summary>
        public bool IsReliableProtocol { get; set; }

        /// <summary>
        /// Gets or sets call id.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Call-ID header field acts as a unique identifier to group together a series
        /// of messages
        /// </para>
        /// <para>
        /// In a new request created by a UAC outside of any dialog, the Call-ID header
        /// field MUST be selected by the UAC as a globally unique identifier over space
        /// and time unless overridden by method-specific behavior. All SIP UAs must have a
        /// means to guarantee that the Call- ID header fields they produce will not be
        /// inadvertently generated by any other UA. 
        /// <para>
        /// Note that when requests are retried
        /// after certain failure responses that solicit an amendment to a request (for
        /// example, a challenge for authentication), these retried requests are not
        /// considered new requests, and therefore do not need new Call-ID header fields;
        /// see Section 8.1.3.5 (RFC 3261).
        /// </para>
        /// </remarks>
        public string CallId { get; set; }
        //for some reason he treats CallId special and adds it to the "Headers" collection in addition to setting it here
        // this is a problem during MessageSerializer.Serialize because that guy add the call-id AGAIN
        //{
        //    get
        //    {
        //        var header = (StringHeader) Headers["call-id"];
        //        return header == null ? string.Empty : header.Value;
        //    }
        //    set
        //    {
        //        if (!Headers.Contains("call-id"))
        //            Headers.Add("call-id", new StringHeader("call-id", value));
        //        else
        //            ((StringHeader) Headers["call-id"]).Value = value;
        //    }
        //}

        /// <summary>
        /// Gets number of bytes in body.
        /// </summary>
        public int ContentLength { get; set; }

        #endregion
    }
}