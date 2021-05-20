using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace GrpcSubscriberConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            using var subscriber = new Subscriber(new PubSub.PubSubClient(channel));
            var streamTask = subscriber.SubscribeAndStream();

            Console.WriteLine("Enter a message to post. Hit q key to quit");
            var input = Console.ReadLine();
            while (string.Compare(input, "q", true) != 0)
            {
                subscriber.Post(input);
                Console.WriteLine("Message posted.");
                Console.WriteLine("Enter a message to post. Hit q key to unsubscribe");
                input = Console.ReadLine();
            }

            subscriber.Unsubscribe();
        }
    }
}
