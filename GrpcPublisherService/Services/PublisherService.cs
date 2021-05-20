using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcPublisherService
{
    public class PublisherService : PubSub.PubSubBase
    {
        private readonly ILogger<PublisherService> logger;
        private static readonly ConcurrentDictionary<string, SubscriberInfo> subscriberLookup = new ConcurrentDictionary<string, SubscriberInfo>();

        public PublisherService(ILogger<PublisherService> logger)
        {
            this.logger = logger;
        }

        public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<StreamElementResponse> responseStream, ServerCallContext context)
        {
            logger.LogInformation("Subscribe: {0}", request.SubscriberId);
            var subscriber = subscriberLookup.GetOrAdd(request.SubscriberId, (key) => new SubscriberInfo(request.SubscriberId, responseStream, context.CancellationToken));

            // Await here, once this method exits, the stream closes
            await subscriber.AsyncManualResetEvent.WaitAsync();
        }

        public override Task<Empty> Unsubscribe(UnsubscribeRequest request, ServerCallContext context)
        {
            logger.LogInformation("Unsubscribe: {0}", request.SubscriberId);
            TryRemoveSubscriber(request.SubscriberId, out _);
            return Task.FromResult(new Empty());
        }

        public override async Task<Empty> Post(PostRequest request, ServerCallContext context)
        {
            logger.LogInformation("Post: {0}", request.Value);

            var responseStreamTasks = new List<Task>();
            var message = new StreamElementResponse() { Value = request.Value };
            var subscribers = subscriberLookup.Values.ToArray();
            foreach (var subscriber in subscribers)
            {
                if (!subscriber.CancellationToken.IsCancellationRequested)
                {
                    responseStreamTasks.Add(subscriber.ResponseStream.WriteAsync(message));
                }
                else
                {
                    // If client disconnected, we should clean up.
                    TryRemoveSubscriber(subscriber.SubscriberId, out _);
                }
            }

            await Task.WhenAll(responseStreamTasks);

            return new Empty();
        }

        private bool TryRemoveSubscriber(string subscriberId, out SubscriberInfo subscriber)
        {
            if (subscriberLookup.TryRemove(subscriberId, out subscriber))
            {
                // Release the await/stream in the subscribe method.
                subscriber.AsyncManualResetEvent.Set();
                return true;
            }
            return false;
        }
    }
}
