using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace TinyActors
{
    public class Mailbox
    {
        internal BlockingCollection<Tuple<string, object>> Queue { get; private set; }

        internal ActorSystem System { get; private set; }

        private UntypedActor actor;

        internal IEnumerator<Outcome> Outcomes { get; private set; }

        internal string Path { get; private set; }

        private Func<UntypedActor> actorCreateFunc;

        public Mailbox(ActorSystem system, string path, Func<UntypedActor> actorCreateFunc, int maxCapacity)
        {
            this.System = system;
            this.Path = path;
            this.actorCreateFunc = actorCreateFunc;
            this.Queue = new BlockingCollection<Tuple<string, object>>(maxCapacity);
        }

        internal void ProcessMessages()
        {
            Debug.Assert(this.Outcomes == null);

            if (this.actor == null)
            {
                this.actor = this.actorCreateFunc();
            }

            int failCount = 0;
            while (failCount < this.Queue.Count)
            {
                failCount++;
                Tuple<string, object> message = null;
                if (this.Queue.TryTake(out message))
                {
                    if (this.Outcomes == null)
                    {
                        this.Outcomes = this.actor.ReceiveMessage(message.Item1, message.Item2).GetEnumerator();
                        if (!this.Outcomes.MoveNext())
                        {
                            this.Outcomes = null;
                        }
                    }

                    if (this.ProcessOutcomes())
                    {
                        failCount = 0;
                    }
                    else
                    {
                        // Get out to allow thread to sleep and retry
                        return;
                    }
                }
            }
        }

        internal bool ProcessOutcomes()
        {
            if (this.Outcomes == null)
            {
                return true;
            }

            do
            {
                if (!this.Outcomes.Current.Process(this))
                {
                    return false;
                }
            } while(this.Outcomes.MoveNext());

            this.Outcomes = null;
            return true;
        }

        internal bool TrySendMessage(string srcPath, object message)
        {
            return this.Queue.TryAdd(Tuple.Create(srcPath, message));
        }

        internal void Stop()
        {
            if (this.Queue.Count > 0)
            {
                Console.WriteLine("Mailbox " + this.Path + " has " + this.Queue.Count + " unprocessed messages");
            }

            if (this.Outcomes != null)
            {
                Console.WriteLine("Mailbox " + this.Path + " has unprocessed outcomes");
            }
        }
    }
}

