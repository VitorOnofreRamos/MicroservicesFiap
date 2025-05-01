using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;

namespace Receiver1_Frutas;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Receiver 1 - Fruta de Época ===");

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
            queue: RabbitMQConfig.ValidatedFrutasQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind Queue to Exchange
        channel.QueueBind(
            queue: RabbitMQConfig.ValidatedFrutasQueue,
            exchange: RabbitMQConfig.ValidationExchange,
            routingKey: RabbitMQConfig.ValidatedFrutasKey);

        Console.WriteLine(" [*] Esperando por mensagem de frutas validades...");

        var consumer = new EvetingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var message = Message<Fruta>.Deserialize(ea.Body.ToArray());
            var fruta = message.Data;

            Console.WriteLine("\n=====================================");
            Console.WriteLine($"Recebida fruta validada: {fruta.Nome}");
            Console.WriteLine($"Status de validação: {(message.IsValid ? "Válida" : "Inválida")}");

            if (!message.IsValid)
            {
                Console.WriteLine($"Mensagem de validação: {message.ValidationMessage}");
            }
            else
            {
                Console.WriteLine($"Detalhes da fruta:");
                Console.WriteLine(fruta.ToString());
            }
            Console.WriteLine("=====================================");

            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(
            queue: RabbitMQConfig.ValidatedFrutasQueue,
            autoAck: false,
            consumer: consumer);

        Console.WriteLine(" [*] Pressione [enter] para sair.");
        Console.ReadLine();
    }
}