namespace NServiceBus.Transport.RabbitMQ
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    class MessageDispatcher : IMessageDispatcher
    {
        readonly ChannelProvider channelProvider;

        public MessageDispatcher(ChannelProvider channelProvider)
        {
            this.channelProvider = channelProvider;
        }

        public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, CancellationToken cancellationToken = default)
        {
            var channel = channelProvider.GetPublishChannel();

            try
            {
                var unicastTransportOperations = outgoingMessages.UnicastTransportOperations;
                var multicastTransportOperations = outgoingMessages.MulticastTransportOperations;

                var tasks = new List<Task>(unicastTransportOperations.Count + multicastTransportOperations.Count);

                transaction.TryGet(out RabbitMQMessagePriority priority);
                foreach (var operation in unicastTransportOperations)
                {
                    tasks.Add(SendMessage(operation, channel, priority, cancellationToken));
                }

                foreach (var operation in multicastTransportOperations)
                {
                    tasks.Add(PublishMessage(operation, channel, priority, cancellationToken));
                }

                channelProvider.ReturnPublishChannel(channel);

                return tasks.Count == 1 ? tasks[0] : Task.WhenAll(tasks);
            }
#pragma warning disable PS0019 // When catching System.Exception, cancellation needs to be properly accounted for - justification:
            // the same action is appropriate when an operation was canceled
            catch
#pragma warning restore PS0019 // When catching System.Exception, cancellation needs to be properly accounted for
            {
                channel.Dispose();
                throw;
            }
        }

        Task SendMessage(UnicastTransportOperation transportOperation, ConfirmsAwareChannel channel, RabbitMQMessagePriority priority, CancellationToken cancellationToken)
        {
            var message = transportOperation.Message;

            var properties = channel.CreateBasicProperties();
            if (priority != null)
            {
                properties.Priority = priority.Priority;
            }
            properties.Fill(message, transportOperation.Properties);

            return channel.SendMessage(transportOperation.Destination, message, properties, cancellationToken);
        }

        Task PublishMessage(MulticastTransportOperation transportOperation, ConfirmsAwareChannel channel, RabbitMQMessagePriority priority, CancellationToken cancellationToken)
        {
            var message = transportOperation.Message;

            var properties = channel.CreateBasicProperties();
            if (priority != null)
            {
                properties.Priority = priority.Priority;
            }
            properties.Fill(message, transportOperation.Properties);

            return channel.PublishMessage(transportOperation.MessageType, message, properties, cancellationToken);
        }

    }
}
