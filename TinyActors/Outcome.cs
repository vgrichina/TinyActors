using System;
using System.Threading.Tasks;

namespace TinyActors
{
    public abstract class Outcome
    {
        internal Outcome()
        {
        }

        internal abstract bool Process(Mailbox mailbox);
    }

    public class SendMessageOutcome : Outcome
    {
        private string dstPath;
        private object message;

        internal SendMessageOutcome(string dstPath, object message)
        {
            this.dstPath = dstPath;
            this.message = message;
        }

        internal override bool Process(Mailbox mailbox)
        {
            return mailbox.System.TrySendMessage(mailbox.Path, dstPath, message);
        }
    }

    public class AwaitTaskOutcome<T> : Outcome
    {
        private Task<T> task;

        internal AwaitTaskOutcome(Task<T> task)
        {
            this.task = task;
        }

        internal override bool Process(Mailbox mailbox)
        {
            return this.task.IsCompleted;
        }
    }
}

