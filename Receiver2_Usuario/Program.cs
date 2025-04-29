using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receiver2_Usuario;

class Program
{
	static async Task Main(string[] args)
	{
		Console.WriteLine("=== Receiver 2 - Dados de Usuário ===");

		// Use the async factory method and await it
		var rabbitConnection = await RabbitMQConnection.CreateAsync();
		// Create channel asynchronously
		var channel = await rabbitConnection.CreateChannelAsync();

		// Declare Exchange (fixed the typo in parameter name)
		await channel.ExchangeDeclareAsync(
			exchange: RabbitMQConfig.ValidationExchange,
			type: ExchangeType.Direct,
			durable: true,
			autoDelete: false);

		// Declare Queue
		await channel.QueueDeclareAsync(
			queue: RabbitMQConfig.ValidatedUsuariosQueue,
			durable: true,
			exclusive: false,
			autoDelete: false);

		// Bind Queue to Exchange
		await channel.QueueBindAsync(
			queue: RabbitMQConfig.ValidatedUsuariosQueue,
			exchange: RabbitMQConfig.ValidationExchange,
			routingKey: RabbitMQConfig.ValidatedUsuariosKey);

		Console.WriteLine(" [*] Esperando por mensagem de usuários validados...");

		// Usando o consumidor básico para processamento assíncrono
		var consumer = new AsyncDefaultBasicConsumer(channel);
		consumer.ConsumedAsync += async (model, ea) =>
		{
			var message = Message<Usuario>.Deserialize(ea.Body.ToArray());
			var usuario = message.Data;

			Console.WriteLine("\n=====================================");
			Console.WriteLine($"Recebido usuário validado: {usuario.NomeCompleto}");
			Console.WriteLine($"Status de validação: {(message.IsValid ? "Válido" : "Inválido")}");

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

			// Acknowledge message asynchronously
			await channel.BasicAckAsync(ea.DeliveryTag, false);
		};

		// Start consuming asynchronously
		var consumerTag = await channel.BasicConsumeAsync(
			queue: RabbitMQConfig.ValidatedUsuariosQueue,
			autoAck: false,
			consumer: consumer);

		Console.WriteLine(" [*] Pressione [enter] para sair.");
		Console.ReadLine();

		// Dispose of resources asynchronously
		await rabbitConnection.DisposeAsync();
	}
}