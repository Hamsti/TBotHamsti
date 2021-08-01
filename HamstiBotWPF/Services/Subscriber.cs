using System;

namespace TBotHamsti.Services
{
    class MessageSubscriber : IDisposable
    {
        private readonly Action<MessageSubscriber> _action;

        public Type ReceiverType { get; }
        public Type MessageType { get; }

        public MessageSubscriber(Type receiverType, Type messageType, Action<MessageSubscriber> action)
        {
            ReceiverType = receiverType;
            MessageType = messageType;
            _action = action;
        }

        public void Dispose() => (_action ?? throw new ArgumentNullException(nameof(_action))).Invoke(this);
    }
}
