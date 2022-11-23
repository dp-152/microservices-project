using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                
                Console.WriteLine("--->> Connected to Message Bus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--->> Could not connect to Message Bus: {ex.Message}");
            }
        }

        public void PublishNewPlatform(PlatformPublishDto platformData)
        {
            var message = JsonSerializer.Serialize(platformData);
            if (_connection.IsOpen)
            {
                Console.WriteLine("--->> RabbitMQ Connection open, sending message...");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine("--->> RabbitMQ Connection not open, refusing to send.");
            }
        }

        private void SendMessage(string message)
        {
            var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message));
            _channel.BasicPublish(exchange: "trigger", routingKey: null, basicProperties: null, body: body);

            Console.WriteLine($"--->> Sent message: {message}");
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--->> RabbitMQ Connection Shutdown");
        }
    }
}