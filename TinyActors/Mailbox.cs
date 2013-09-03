using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace TinyActors
{
    public class Mailbox
    {
        private BlockingCollection<Tuple<string, object>> queue;
        internal BlockingCollection<Tuple<string, object>> Queue
        {
            get { return this.queue; }
        }

        private ActorSystem system;
        internal ActorSystem System
        {
            get { return this.system; }
        }

        private UntypedActor actor;

        private IEnumerator<Outcome> outcomes = null;
        internal IEnumerator<Outcome> Outcomes
        {
            get { return this.outcomes; }
        }

        private string path;
        internal string Path
        {
            get { return this.path; }
        }

        private Func<UntypedActor> actorCreateFunc;

        public Mailbox(ActorSystem system, string path, Func<UntypedActor> actorCreateFunc, int maxCapacity)
        {
            this.system = system;
            this.path = path;
            this.actorCreateFunc = actorCreateFunc;
            this.queue = new BlockingCollection<Tuple<string, object>>(maxCapacity);
        }

        internal void ProcessMessages()
        {
            Debug.Assert(this.outcomes == null);

            if (this.actor == null)
            {
                this.actor = this.actorCreateFunc();
            }

            int failCount = 0;
            while (failCount < this.queue.Count)
            {
                failCount++;
                Tuple<string, object> message = null;
                if (this.queue.TryTake(out message))
                {
                    this.outcomes = this.actor.ReceiveMessage(message.Item1, message.Item2).GetEnumerator();
                    if (this.ProcessOutcomes())
                    {
                        failCount = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        internal bool ProcessOutcomes()
        {
            if (this.outcomes == null)
            {
                return true;
            }

            while (this.outcomes.MoveNext())
            {
                if (!this.outcomes.Current.Process(this))
                {
                    return false;
                }
            }

            return true;
        }

        internal bool TrySendMessage(string srcPath, object message)
        {
            return this.queue.TryAdd(Tuple.Create(srcPath, message));
        }
    }
}

