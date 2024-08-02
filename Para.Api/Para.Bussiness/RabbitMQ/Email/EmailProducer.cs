using Microsoft.Extensions.Configuration;

namespace Para.Bussiness.RabbitMQ.Email
{
    public class EmailProducer
    {
        private readonly RabbitMQClient _rabbitMQClient;

        public EmailProducer(IConfiguration configuration)
        {
            var rabbitMqSettings = configuration.GetSection("RabbitMQ");
            _rabbitMQClient = new RabbitMQClient(
                rabbitMqSettings["Hostname"],
                int.Parse(rabbitMqSettings["Port"]),
                rabbitMqSettings["UserName"],
                rabbitMqSettings["Password"],
                rabbitMqSettings["QueueName"]
            );
        }

        public void QueueEmail(string subject, string email, string content)
        {
            var message = $"{subject}|{email}|{content}";
            _rabbitMQClient.SendMessage(message);
        }
    }
}
