using System;
using System.Collections.Generic;

namespace TinyActors
{
    public abstract class UntypedActor
    {
        public UntypedActor()
        {
        }

        protected abstract IEnumerable<Outcome> OnMessage(object message);

        internal IEnumerable<Outcome> SendMessage(string senderPath, object message)
        {
            return this.OnMessage(message);
        }
    }
}

