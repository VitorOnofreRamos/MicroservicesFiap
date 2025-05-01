using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;

namespace Receiver1_Frutas;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Receiver 2 - Dados de Usuário ===");

        using var rabbitConnection = new RabbitMQConnection();
        using var channel = rabbitConnection.CreateChannel();

        // Declare Exchange
        channel.ExchangeDeclare(
            exchage: RabbitMQConfig.ValidationExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        // Declare Queue
        channel.QueueDeclare(
            queue: RabbitMQConfig.ValidatedUsuariosQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind Queue to Exchange
        channel.QueueBind(
            queue: RabbitMQConfig.ValidatedUsuariosQueue,
            exchange: RabbitMQConfig.ValidationExchange,
            routingKey: RabbitMQConfig.ValidatedUsuariosKey);

        Console.WriteLine(" [*] Esperando por mensagem de usuários validades...");

        var consumer = new EvetingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var message = Message<Usuario>.Deserialize(ea.Body.ToArray());
            var usuario = message.Data;

            Console.WriteLine("\n=====================================");
            Console.WriteLine($"Recebida usuários validada: {usuario.NomeCompleto}");
            Console.WriteLine($"Status de validação: {(message.IsValid ? "Válida" : "Inválida")}");

            if (!message.IsValid)
            {
                Console.WriteLine($"Mensagem de validação: {message.ValidationMessage}");
            }
            else
            {
                Console.WriteLine($"Detalhes do usuário:");
                Console.WriteLine(usuario.ToString());
            }
            Console.WriteLine("=====================================");

            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(
            queue: RabbitMQConfig.ValidatedUsuariosQueue,
            autoAck: false,
            consumer: consumer);

        Console.WriteLine(" [*] Pressione [enter] para sair.");
        Console.ReadLine();
    }
}