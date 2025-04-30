using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;
using System.Reflection;
using System.Security.Authentication;

namespace Sender1_Frutas;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Sender 1 - Frutas de Época ===");

        using var rabbitConnection = new RabbitMQConnection();
        using var channel = rabbitConnection.CreateChannel();

        // Declare Exchange
        channel.ExchangeDeclare(
            exchange: RabbitMQConfig.FrutasExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        // Declare Queue
        channel.QueueDeclare(
            queue: RabbitMQConfig.FrutasToValidationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind Queue to Exchage
        channel.QueueBind(
            queue: RabbitMQConfig.FrutasToValidationQueue,
            exchange: RabbitMQConfig.FrutasExchange,
            routingKey: RabbitMQConfig.FrutasToValidationKey);

        while (true) 
        {
            Console.WriteLine("\nDigite as informações da fruta de época");
            Console.Write("Nome da fruta: ");
            string nome = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nome))
            {
                Console.WriteLine("Encerrando aplicação...");
                break;
            }

            Console.WriteLine("Descrição sucinta:");
            string descricao = Console.ReadLine();

            var fruta = new Fruta
            {
                Nome = nome,
                Descricao = descricao,
                DataHoraSolicitacao = DateTime.Now,
            };

            var message = new Message<Fruta>
            {
                Data = fruta,
                IsValid = false,
                ValidationMessage = null
            };

            var messageBytes = Message<Fruta>.Serialize(message);

            //Publish message
            channel.BasicPublish(
                exchange: RabbitMQConfig.FrutasExchange,
                routingKey: RabbitMQConfig.FrutasToValidationKey,
                BasicProperties: null,
                MethodBody: messageBytes);

            Console.WriteLine($"[X] Enviado {fruta.Nome} para validação em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        };
    }
}
