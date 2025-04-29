using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receiver1_Frutas;

class Program
{
	static async Task Main(string[] args)
	{
		Console.WriteLine("=== Receiver 1 - Frutas de Época ===");

		// Use the async factory method and await it
		var rabbitConnection = await RabbitMQConnection.CreateAsync();
		// Create channel asynchronously
		var channel = await rabbitConnection.CreateChannelAsync();

		// Declare Exchange
		await channel.ExchangeDeclareAsync(
			exchange: RabbitMQConfig.ValidationExchange,
			type: ExchangeType.Direct,
			durable: true,
			autoDelete: false);

		// Declare Queue
		await channel.QueueDeclareAsync(
			queue: RabbitMQConfig.ValidatedFrutasQueue,
			durable: true,
			exclusive: false,
			autoDelete: false);

		// Bind Queue to Exchange
		await channel.QueueBindAsync(
			queue: RabbitMQConfig.ValidatedFrutasQueue,
			exchange: RabbitMQConfig.ValidationExchange,
			routingKey: RabbitMQConfig.ValidatedFrutasKey);

		Console.WriteLine(" [*] Esperando por mensagem de frutas validadas...");

		// Use AsyncEventingBasicConsumer instead of AsyncDefaultBasicConsumer
		var consumer = new AsyncEventingBasicConsumer(channel);

		// Use Received event instead of ConsumedAsync
		consumer.ReceivedAsync += async (sender, ea) =>
		{
			var message = Message<Fruta>.Deserialize(ea.Body.ToArray());
			var fruta = message.Data;

			Console.WriteLine("\n=====================================");
			Console.WriteLine($"Recebido fruta validada: {fruta.Nome}");
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

			// Acknowledge message asynchronously
			await channel.BasicAckAsync(ea.DeliveryTag, false);
		};

		// Start consuming asynchronously
		var consumerTag = await channel.BasicConsumeAsync(
			queue: RabbitMQConfig.ValidatedFrutasQueue,
			autoAck: false,
			consumer: consumer);

		Console.WriteLine(" [*] Pressione [enter] para sair.");
		Console.ReadLine();

		// Dispose of resources asynchronously
		await rabbitConnection.DisposeAsync();
	}
}