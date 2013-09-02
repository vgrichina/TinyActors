using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TinyActors
{
    public class ActorSystem
    {
        private ConcurrentQueue<Mailbox> queue;
        private ConcurrentDictionary<string, Mailbox> mailboxes;
        private int numThreads = 4; // TODO: Make configurable
        private List<Thread> threads;

        public ActorSystem()
        {
            this.queue = new ConcurrentQueue<Mailbox>();
            this.mailboxes = new ConcurrentDictionary<string, Mailbox>();
            this.threads = Enumerable.Range(0, this.numThreads).Select(i => new Thread(this.RunThread)).ToList();
        }

        private void RunThread()
        {
            int sleepInterval = 10;

            while (true)
            {
                Mailbox mailbox = null;
                if (this.queue.TryDequeue(out mailbox))
                {
                    if (mailbox.ProcessOutcomes())
                    {
                        mailbox.ProcessMessages();
                    }
                }

                Thread.Sleep(sleepInterval);

                if (mailbox != null)
                {
                    this.queue.Enqueue(mailbox);
                }
            }
        }
    }
}

