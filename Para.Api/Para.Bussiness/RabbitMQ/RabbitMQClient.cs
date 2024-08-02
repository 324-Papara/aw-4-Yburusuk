using RabbitMQ.Client;
using System.Text;

namespace Para.Bussiness.RabbitMQ
{
    public class RabbitMQClient : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;

        public RabbitMQClient(string hostname, int port, string username, string password, string queueName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _queueName = queueName;
        }

        public void SendMessage(string message)
        {
            if (!_channel.IsOpen)
                throw new InvalidOperationException("RabbitMQ channel is closed !");

            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body);
        }

        public async Task SendMessageAsync(string message)
        {
            if (!_channel.IsOpen)
                throw new InvalidOperationException("RabbitMQ channel is closed !");

            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            await Task.Run(() => _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body));
        }

        public IModel GetChannel() => _channel;
        public string GetQueueName() => _queueName;

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
