using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TinyActors
{
    public abstract class UntypedActor
    {
        internal Mailbox Mailbox;

        protected abstract IEnumerable<Outcome> OnMessage(string srcPath, object message);

        internal IEnumerable<Outcome> ReceiveMessage(string srcPath, object message)
        {
            return this.OnMessage(srcPath, message);
        }   

        protected Outcome SendMessage(string dstPath, object message)
        {
            return new SendMessageOutcome(dstPath, message);
        }

        protected Outcome AwaitTask<T>(Task<T> task)
        {
            return new AwaitTaskOutcome<T>(task);
        }
    }
}

