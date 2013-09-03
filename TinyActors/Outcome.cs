using System;

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
}

