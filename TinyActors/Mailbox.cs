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

        private UntypedActor actor;

        private IEnumerator<Outcome> outcomes = null;

        private string path;
        internal string Path
        {
            get { return this.path; }
        }

        public Mailbox(string path, int maxCapacity)
        {
            this.path = path;
            this.queue = new BlockingCollection<Tuple<string, object>>(maxCapacity);
        }

        internal void ProcessMessages()
        {
            Debug.Assert(this.outcomes == null);

            int failCount = 0;
            while (failCount < this.queue.Count)
            {
                Tuple<string, object> message = null;
                if (this.queue.TryTake(out message))
                {
                    this.outcomes = this.actor.SendMessage(message.Item1, message.Item2).GetEnumerator();
                    if (this.outcomes.MoveNext() || this.ProcessOutcomes())
                    {
                        failCount = 0;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    failCount++;
                }
            }
        }

        internal bool ProcessOutcomes()
        {
            while (this.outcomes != null)
            {
                if (!this.ProcessOutcome())
                {
                    return false;
                }

                if (!this.outcomes.MoveNext())
                {
                    this.outcomes = null;
                }
            }

            return true;
        }

        bool ProcessOutcome()
        {
            throw new NotImplementedException();
        }
    }
}

