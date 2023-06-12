#pragma warning disable 1591

namespace NServiceBus
{
    using System;
    using Transport.RabbitMQ;

    public static partial class RabbitMQTransportSettingsExtensions
    {
        public static DelayedDeliverySettings DelayedDelivery(this TransportExtensions<RabbitMQTransport> transportExtensions)
        {
            throw new NotImplementedException();
        }
    }

    public class DelayedDeliverySettings
    {
        public DelayedDeliverySettings EnableTimeoutManager()
        {
            throw new NotImplementedException();
        }
    }

    public partial class RabbitMQTransport
    {
        internal string LegacyApiConnectionString { get; set; }

        internal Func<bool, IRoutingTopology> TopologyFactory { get; set; }

        internal bool UseDurableExchangesAndQueues { get; set; } = true;

        bool legacyMode;

        internal RabbitMQTransport() : base(TransportTransactionMode.ReceiveOnly, true, true, true)
        {
            legacyMode = true;
        }

        void ValidateAndApplyLegacyConfiguration()
        {
            if (!legacyMode)
            {
                return;
            }

            if (TopologyFactory == null)
            {
                throw new Exception("A routing topology must be configured with one of the 'EndpointConfiguration.UseTransport<RabbitMQTransport>().UseXXXXRoutingTopology()` methods. Most new projects should use the Conventional routing topology.");
            }

            RoutingTopology = TopologyFactory(UseDurableExchangesAndQueues);

            if (string.IsNullOrEmpty(LegacyApiConnectionString))
            {
                throw new Exception("A connection string must be configured with 'EndpointConfiguration.UseTransport<RabbitMQTransport>().ConnectionString()` method.");
            }

            ConnectionConfiguration = ConnectionConfiguration.Create(LegacyApiConnectionString);
        }
    }
}

#pragma warning restore 1591
