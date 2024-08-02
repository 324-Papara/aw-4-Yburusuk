using Microsoft.Extensions.Configuration;
using Para.Bussiness.Notification;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Para.Bussiness.RabbitMQ.Email
{
    public class EmailConsumer
    {
        private readonly RabbitMQClient _rabbitMQClient;
        private readonly INotificationService _notificationService;

        public EmailConsumer(IConfiguration configuration, INotificationService notificationService)
        {
            var rabbitMqSettings = configuration.GetSection("RabbitMQ");
            _notificationService = notificationService;
            _rabbitMQClient = new RabbitMQClient(
                rabbitMqSettings["Hostname"],
                int.Parse(rabbitMqSettings["Port"]),
                rabbitMqSettings["UserName"],
                rabbitMqSettings["Password"],
                rabbitMqSettings["QueueName"]
            );
        }

        public void StartListening()
        {
            var channel = _rabbitMQClient.GetChannel();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var parts = message.Split('|');
                var subject = parts[0];
                var email = parts[1];
                var content = parts[2];

                _notificationService.SendEmail(subject, email, content);
            };

            channel.BasicConsume(queue: _rabbitMQClient.GetQueueName(),
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
