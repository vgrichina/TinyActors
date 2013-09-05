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
        private bool isStopping = false;

        public ActorSystem()
        {
            this.queue = new ConcurrentQueue<Mailbox>();
            this.mailboxes = new ConcurrentDictionary<string, Mailbox>();
            this.threads = Enumerable.Range(0, this.numThreads).Select(i => new Thread(this.RunThread)).ToList();
            this.threads.ForEach(thread => thread.Start());
        }

        private void RunThread()
        {
            int sleepInterval = 100; // TODO: Make this configurable?

            while (true)
            {
                Console.Write(".");
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

        public bool TrySendMessage(string srcPath, string dstPath, object message)
        {
            Mailbox mailbox;
            if (!this.mailboxes.TryGetValue(dstPath, out mailbox))
            {
                return false;
            }

            return mailbox.TrySendMessage(srcPath, message);
        }

        public void AddMailbox(string path, Func<UntypedActor> createFunc, int maxCapacity)
        {
            // TODO: Allow to define default maxCapavity in declarative way

            Mailbox mailbox = new Mailbox(this, path, createFunc, maxCapacity);
            lock (this)
            {
                if (this.isStopping)
                {
                    throw new InvalidOperationException("Cannot add new mailbox to system which is being stopped");
                }
            }

            if (!this.mailboxes.TryAdd(path, mailbox))
            {
                throw new ArgumentException("There is already actor registered at path: " + path);
            }
            this.queue.Enqueue(mailbox);
        }

        public void Stop()
        {
            lock (this)
            {
                this.isStopping = true;
            }

            List<Mailbox> stoppedMailboxes = new List<Mailbox>();
            while (stoppedMailboxes.Count < this.mailboxes.Count)
            {
                Console.WriteLine("Stopped count: " + stoppedMailboxes.Count);

                Mailbox mailbox;
                while (!this.queue.TryDequeue(out mailbox))
                {
                    Thread.Sleep(1);
                }

                mailbox.Stop();
                stoppedMailboxes.Add(mailbox);
            }

            this.threads.ForEach(thread => thread.Interrupt());
        }
    }
}

