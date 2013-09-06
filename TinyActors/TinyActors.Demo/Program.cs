using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace TinyActors.Demo
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello Actors!");

            int numThreads = 2;
            ActorSystem system = new ActorSystem(numThreads);

            int produceCount = 20;
            int numProcessors = 2;
            system.AddMailbox("/producer", () => new ProduceActor("/processor", produceCount, numProcessors), 100);
            Enumerable.Range(0, numProcessors).ToList().ForEach(i => {
                system.AddMailbox("/processor/" + i, () => new LongWorkActor("/joiner"), 2);
            });
            system.AddMailbox("/joiner", () => new JoinActor(), 1);

            Debug.Assert(system.TrySendMessage(null, "/producer", "test"));

            Thread.Sleep(15 * 1000);
            Console.WriteLine("Stopping");
            system.Stop();
        }

        class ProduceActor : UntypedActor
        {
            private string dstPath;
            private int count;
            private int numProcessors;

            public ProduceActor(string dstPath, int count, int numProcessors)
            {
                this.dstPath = dstPath;
                this.count = count;
                this.numProcessors = numProcessors;
            }

            protected override IEnumerable<Outcome> OnMessage(string srcPath, object message)
            {
                for (int i = 0; i < this.count; i++)
                {
                    Console.WriteLine("Producing " + message + " " + i);
                    yield return this.SendMessage(this.dstPath + "/" + i % this.numProcessors, message + "_" + i);
                }
            }
        }

        class LongWorkActor : UntypedActor
        {
            private string dstPath;

            public LongWorkActor(string dstPath)
            {
                this.dstPath = dstPath;
            }

            protected override IEnumerable<Outcome> OnMessage(string srcPath, object message)
            {
                Console.WriteLine("Processing " + message);

                // Simulate long running work
                Thread.Sleep(1 * 1000);

                yield return this.SendMessage(this.dstPath, message);
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
