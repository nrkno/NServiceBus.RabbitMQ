namespace NServiceBus.Transport.RabbitMQ
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Extensibility;

    class MessageDispatcher : IDispatchMessages
    {
        readonly ChannelProvider channelProvider;

        public MessageDispatcher(ChannelProvider channelProvider)
        {
            this.channelProvider = channelProvider;
        }

        public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, ContextBag context)
        {
            var channel = channelProvider.GetPublishChannel();

            try
            {
                var unicastTransportOperations = outgoingMessages.UnicastTransportOperations;
                var multicastTransportOperations = outgoingMessages.MulticastTransportOperations;

                var tasks = new List<Task>(unicastTransportOperations.Count + multicastTransportOperations.Count);

                foreach (var operation in unicastTransportOperations)
                {
                    context.TryGet(out RabbitMQMessagePriority priority);
                    tasks.Add(SendMessage(operation, channel, priority));
                }

                foreach (var operation in multicastTransportOperations)
                {
                    context.TryGet(out RabbitMQMessagePriority priority);
                    tasks.Add(PublishMessage(operation, channel, priority));
                }

                channelProvider.ReturnPublishChannel(channel);

                return tasks.Count == 1 ? tasks[0] : Task.WhenAll(tasks);
            }
            catch
            {
                channel.Dispose();
                throw;
            }
        }

        Task SendMessage(UnicastTransportOperation transportOperation, ConfirmsAwareChannel channel, RabbitMQMessagePriority priority)
        {
            var message = transportOperation.Message;

            var properties = channel.CreateBasicProperties();
            properties.Fill(message, transportOperation.DeliveryConstraints);
            if (priority != null)
            {
                properties.Priority = priority.Priority;
            }
            return channel.SendMessage(transportOperation.Destination, message, properties);
        }

        Task PublishMessage(MulticastTransportOperation transportOperation, ConfirmsAwareChannel channel, RabbitMQMessagePriority priority)
        {
            var message = transportOperation.Message;

            var properties = channel.CreateBasicProperties();
            properties.Fill(message, transportOperation.DeliveryConstraints);
            if (priority != null)
            {
                properties.Priority = priority.Priority;
            }
            return channel.PublishMessage(transportOperation.MessageType, message, properties);
        }
    }
}
