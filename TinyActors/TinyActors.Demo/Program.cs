using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TinyActors.Demo
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello Actors!");
           
            ActorSystem system = new ActorSystem();
            system.AddMailbox("/producer", () => new ProduceActor(), 100);
            system.AddMailbox("/joiner", () => new JoinActor(), 1);

            Debug.Assert(system.TrySendMessage(null, "/producer", "test"));

            Thread.Sleep(3 * 1000);
            Console.WriteLine("Stopping");
            system.Stop();
        }

        class ProduceActor : UntypedActor
        {
            protected override IEnumerable<Outcome> OnMessage(string srcPath, object message)
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("Producing " + message + " " + i);
                    yield return this.SendMessage("/joiner", message + "_" + i);
                }
            }
        }

        class JoinActor : UntypedActor
        {
            List<string> collectedMessages = new List<string>();

            protected override IEnumerable<Outcome> OnMessage(string srcPath, object message)
            {
                Console.WriteLine("Should join " + message);
                this.collectedMessages.Add((string) message);

                if (this.collectedMessages.Count == 2)
                {
                    Console.WriteLine("I like my messages in pairs, here they go: " + String.Join(", ", collectedMessages));
                    this.collectedMessages.Clear();
                }

                yield break;
            }
        }
    }
}
