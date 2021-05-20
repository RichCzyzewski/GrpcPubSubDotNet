using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcSubscriberConsole
{
    public class Subscriber : IDisposable
    {
        private readonly PubSub.PubSubClient pubSubClient;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly string subscriberId = Guid.NewGuid().ToString();
        private bool disposed = false;

        public Subscriber(PubSub.PubSubClient pubSubClient)
        {
            this.pubSubClient = pubSubClient;
        }

        ~Subscriber() => Dispose(false);

        public async Task SubscribeAndStream()
        {
            var subscribeRequest = new SubscribeRequest() { SubscriberId = subscriberId };
            using var call = pubSubClient.Subscribe(subscribeRequest);
            //Receive
            while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
            {
                Console.WriteLine("Event received: " + call.ResponseStream.Current);
            }
        }

        public void Unsubscribe()
        {
            var unsubscribeRequest = new UnsubscribeRequest() { SubscriberId = subscriberId };
            pubSubClient.Unsubscribe(unsubscribeRequest);
            cancellationTokenSource.Cancel();
        }

        public void Post(string input)
        {
            pubSubClient.Post(new PostRequest() { Value = input });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                cancellationTokenSource.Dispose();
            }

            disposed = true;
        }
    }
}
