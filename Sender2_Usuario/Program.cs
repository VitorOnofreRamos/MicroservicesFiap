using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;
using System.Reflection;

namespace Sender2_Usuario;

class Program
{
    static async void Main(string[] args)
    {
        Console.WriteLine("=== Sender 2 -Dados de Usuário ===");

        var rabbitConnection = await RabbitMQConnection.CreateAsync();
        var channel = await rabbitConnection.CreateChannelAsync();

        // Declare Exchange
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConfig.UsuariosExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        // Declare Queue
        await channel.QueueDeclareAsync(
            queue: RabbitMQConfig.UsuariosToValidationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind Queue to Exchage
        await channel.QueueBindAsync(
            queue: RabbitMQConfig.UsuariosToValidationQueue,
            exchange: RabbitMQConfig.UsuariosExchange,
            routingKey: RabbitMQConfig.UsuariosToValidationKey);

        while (true)
        {
            Console.WriteLine("\nDigite os dados do usuários");
            Console.Write("Nome completo");
            string nome = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nome))
            {
                Console.WriteLine("Encerrando aplicação...");
                break;
            }

            Console.WriteLine("Endereço: ");
            string endereco = Console.ReadLine();

            Console.WriteLine("RG: ");
            string rg = Console.ReadLine();

            Console.WriteLine("CPF: ");
            string cpf = Console.ReadLine();

            var usuario = new Usuario
            {
                NomeCompleto = nome,
                Endereco = endereco,
                RG = rg,
                CPF = cpf,
                DataHoraRegistro = DateTime.Now
            };

            var message = new Message<Usuario>
            {
                Data = usuario,
                IsValid = false,
                ValidationMessage = null
            };

            var messageBytes = Message<Usuario>.Serialize(message);

            // Publish message
            await channel.BasicPublishAsync(
                exchange: RabbitMQConfig.UsuariosExchange,
                routingKey: RabbitMQConfig.UsuariosToValidationKey,
                basicProperties: null,
                body: messageBytes);

            Console.WriteLine($"[x] Enviando usuário {usuario.NomeCompleto} para validação em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        await rabbitConnection.DisposeAsync();
    }
}
