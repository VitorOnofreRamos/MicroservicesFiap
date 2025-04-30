using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;
using System.Reflection;

namespace Sender2_Usuario;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Sender 2 -Dados de Usuário ===");

        using var rabbitConnection = new RabbitMQConnection();
        using var channel = rabbitConnection.CreateChannel();

        // Declare Exchange
        channel.ExchangeDeclare(
            exchange: RabbitMQConfig.UsuariosExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        // Declare Queue
        channel.QueueDeclare(
            queue: RabbitMQConfig.UsuariosToValidationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind Queue to Exchage
        channel.QueueBind(
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
            channel.BasicPublich(
                exchange: RabbitMQConfig.UsuariosExchange,
                routingKey: RabbitMQConfig.UsuariosToValidationKey,
                BasicProperties: null,
                MethodBody: messageBytes);

            Console.WriteLine($"[x] Enviando usuário {usuario.NomeCompleto} para validação em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
    }
}
