using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConnection _connection;
        private IModel _channel;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",  // This assumes RabbitMQ is running locally or in a container
                UserName = "user",       // Use RabbitMQ credentials, for testing 
                Password = "password"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "notificationQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation(message: $"Received message: {message}");


                // Here you can implement the notification logic (e.g., send email/SMS)
                SendNotification(message);
            };

            _channel.BasicConsume(queue: "notificationQueue", autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        private void SendNotification(string message)
        {
            // Placeholder for sending notification (Email/SMS)
            _logger.LogInformation($"Notification sent: {message}");
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
