using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using Validation.Validators;

namespace Validation;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Validation Service ===");

        using var rabbitConnection = new RabbitMQConnection();
        using var channel = rabbitConnection.CreateChannel();

        SetupFrutasValidation(channel);
        SetupUsuáriosValidation(channel);

        Console.WriteLine(" [*] Esperando por mensagens para validar...");
        Console.WriteLine(" [*] Pressione [enter] para sair.");
        Console.ReadLine();
    }

    private static void SetupFrutasValidation(IModel channel)
    {
        // Configuração para frutas
        channel.ExchangeDeclare(
            exchange: RabbitMQConfig.FrutasExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        channel.ExchangeDeclare(
            exchange: RabbitMQConfig.ValidationExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        channel.QueueDeclare(
            queue: RabbitMQConfig.FrutasToValidationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueDeclare(
            queue: RabbitMQConfig.ValidatedFrutasQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueBind(
            queue: RabbitMQConfig.FrutasToValidationQueue,
            exchange: RabbitMQConfig.FrutasExchange,
            routingKey: RabbitMQConfig.FrutasToValidationKey);

        channel.QueueBind(
            queue: RabbitMQConfig.ValidatedFrutasQueue,
            exchange: RabbitMQConfig.ValidationExchange,
            routingKey: RabbitMQConfig.ValidatedFrutasKey);

        // Consumer
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var message = Message<Fruta>.Deserialize(ea.Body.ToArray());
            var fruta = message.Data;

            Console.WriteLine($"\n[x] Recebido: {fruta.Nome} para validação");

            var validator = new FrutaValidator();
            bool isValid = validator.Validate(fruta, out string validationMessage);

            message.IsValid = isValid;
            message.ValidationMessage = validationMessage;

            // Enviar resposta valida
            var responseBytes = Message<Fruta>.Serialize(message);

            channel.BasicPublish(
                exchange: RabbitMQConfig.ValidationExchange,
                routingKey: RabbitMQConfig.ValidatedFrutasKey,
                BasicProperties: null,
                MethodBody: responseBytes);

            Console.WriteLine($"[X] Enviando resultado de validação para fruta {fruta.Nome}: {(isValid ? "Válida" : "Inválida")}");
            if (!isValid)
                Console.WriteLine($"     Motivo: {validationMessage}");

            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(
            queue: RabbitMQConfig.FrutasToValidationQueue,
            autoAck: false,
            consumer: consumer);
    }

    private static void SetupUsuariosValidtion(IModel channel)
    {
        // Configuração para usuários
        channel.ExchangeDeclare(
            exchange: RabbitMQConfig.UsuariosExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        channel.ExchangeDeclare(
            exchange: RabbitMQConfig.UsuariosToValidationQueue,
            type: true,
            durable: false,
            autoDelete: false);

        channel.QueueDeclare(
            queue: RabbitMQConfig.ValidatedUsuariosQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueBind(
            queue: RabbitMQConfig.UsuariosToValidationQueue,
            exchange: RabbitMQConfig.UsuariosExchange,
            routingKey: RabbitMQConfig.UsuariosToValidationKey);

        channel.QueueBind(
            queue: RabbitMQConfig.ValidatedUsuariosQueue,
            exchange: RabbitMQConfig.ValidationExchange,
            routingKey: RabbitMQConfig.ValidatedUsuariosKey);

        // Consumer
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var message = Message<Usuario>.Deserialize(ea.Body.ToArray());
            var usuario = message.Data;

            Console.WriteLine($"[x] Enviando resultado de validação para usuário {usuario.NomeCompleto}: {(isValid ? "Válido" : "Inválido")}");
            if (!isValid)
                Console.WriteLine($"    Motivo: {validationMessage}");

            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(
            queue: RabbitMQConfig.UsuariosToValidationQueue,
            autoAck: false,
            consumer: consumer);
    }
}
