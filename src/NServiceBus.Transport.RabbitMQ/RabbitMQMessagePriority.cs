namespace NServiceBus.Transport.RabbitMQ
{
    /// <summary>
    /// Delivery constraint to be able to set priority on a RabbitMQ message.
    /// To be able to use this, queues will have to be created with a max priority.
    /// RabbitMQ default to queues without priority since it comes as a cost.
    /// See https://www.rabbitmq.com/priority.html 
    /// </summary>
    public class RabbitMQMessagePriority
    {
        /// <summary>
        /// 
        /// </summary>
        public byte Priority { get; private set; }

        /// <summary>
        /// See https://www.rabbitmq.com/priority.html
        /// </summary>
        /// <param name="priority">Message priority 0 to 9.</param>
        public RabbitMQMessagePriority(byte priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// Set a rabbit mq message priority explicitly after a priority is already defined in a given context
        /// </summary>
        /// <param name="priority">Message priority 0 to 9.</param>
        public void SetPriority(byte priority)
        {
            Priority = priority;
        }
    }
}