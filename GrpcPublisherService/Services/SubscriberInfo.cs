using Grpc.Core;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcPublisherService
{
    public class SubscriberInfo
    {
        public string SubscriberId { get; private set; }

        public IServerStreamWriter<StreamElementResponse> ResponseStream { get; private set; }

        public AsyncManualResetEvent AsyncManualResetEvent { get; private set; }

        // Will be set if client disconnected.
        public CancellationToken CancellationToken { get; private set; }

        public SubscriberInfo(string subscriberId, IServerStreamWriter<StreamElementResponse> responseStream, CancellationToken cancellationToken)
        {
            this.SubscriberId = subscriberId;
            this.ResponseStream = responseStream;
            this.CancellationToken = cancellationToken;
            this.AsyncManualResetEvent = new AsyncManualResetEvent(false);
        }
    }
}
