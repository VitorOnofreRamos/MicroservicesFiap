using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Validation.Validators;

namespace Validation;

class Program
{
	static async Task Main(string[] args)
	{
		Console.WriteLine("=== Validation Service ===");

		// Use the async factory method and await it
		var rabbitConnection = await RabbitMQConnection.CreateAsync();
		// Create channel asynchronously
		var channel = await rabbitConnection.CreateChannelAsync();

		await SetupFrutasValidationAsync(channel);
		await SetupUsuariosValidationAsync(channel);

		Console.WriteLine(" [*] Esperando por mensagens para validar...");
		Console.WriteLine(" [*] Pressione [enter] para sair.");
		Console.ReadLine();

		// Dispose of resources asynchronously
		await rabbitConnection.DisposeAsync();
	}

	private static async Task SetupFrutasValidationAsync(IChannel channel)
	{
		// Configuração para frutas
		await channel.ExchangeDeclareAsync(
			exchange: RabbitMQConfig.FrutasExchange,
			type: ExchangeType.Direct,
			durable: true,
			autoDelete: false);

		await channel.ExchangeDeclareAsync(
			exchange: RabbitMQConfig.ValidationExchange,
			type: ExchangeType.Direct,
			durable: true,
			autoDelete: false);

		await channel.QueueDeclareAsync(
			queue: RabbitMQConfig.FrutasToValidationQueue,
			durable: true,
			exclusive: false,
			autoDelete: false);

		await channel.QueueDeclareAsync(
			queue: RabbitMQConfig.ValidatedFrutasQueue,
			durable: true,
			exclusive: false,
			autoDelete: false);

		await channel.QueueBindAsync(
			queue: RabbitMQConfig.FrutasToValidationQueue,
			exchange: RabbitMQConfig.FrutasExchange,
			routingKey: RabbitMQConfig.FrutasToValidationKey);

		await channel.QueueBindAsync(
			queue: RabbitMQConfig.ValidatedFrutasQueue,
			exchange: RabbitMQConfig.ValidationExchange,
			routingKey: RabbitMQConfig.ValidatedFrutasKey);

		// Consumer
		var consumer = new AsyncDefaultBasicConsumer(channel);
		consumer.ConsumedAsync += async (model, ea) =>
		{
			var message = Message<Fruta>.Deserialize(ea.Body.ToArray());
			var fruta = message.Data;

			Console.WriteLine($"\n[x] Recebido: {fruta.Nome} para validação");

			var validator = new FrutaValidator();
			bool isValid = validator.Validate(fruta, out string validationMessage);

			message.IsValid = isValid;
			message.ValidationMessage = validationMessage;

			// Enviar resposta validada
			var responseBytes = Message<Fruta>.Serialize(message);

			await channel.BasicPublishAsync(
				exchange: RabbitMQConfig.ValidationExchange,
				routingKey: RabbitMQConfig.ValidatedFrutasKey,
				basicProperties: null,
				body: responseBytes);

			Console.WriteLine($"[X] Enviando resultado de validação para fruta {fruta.Nome}: {(isValid ? "Válida" : "Inválida")}");
			if (!isValid)
				Console.WriteLine($"     Motivo: {validationMessage}");

			await channel.BasicAckAsync(ea.DeliveryTag, false);
		};

		var consumerTag = await channel.BasicConsumeAsync(
			queue: RabbitMQConfig.FrutasToValidationQueue,
			autoAck: false,
			consumer: consumer);
	}

	private static async Task SetupUsuariosValidationAsync(IChannel channel)
	{
		// Configuração para usuários
		await channel.ExchangeDeclareAsync(
			exchange: RabbitMQConfig.UsuariosExchange,
			type: ExchangeType.Direct,
			durable: true,
			autoDelete: false);

		await channel.ExchangeDeclareAsync(
			exchange: RabbitMQConfig.ValidationExchange,
			type: ExchangeType.Direct,
			durable: true,
			autoDelete: false);

		await channel.QueueDeclareAsync(
			queue: RabbitMQConfig.UsuariosToValidationQueue,
			durable: true,
			exclusive: false,
			autoDelete: false);

		await channel.QueueDeclareAsync(
			queue: RabbitMQConfig.ValidatedUsuariosQueue,
			durable: true,
			exclusive: false,
			autoDelete: false);

		await channel.QueueBindAsync(
			queue: RabbitMQConfig.UsuariosToValidationQueue,
			exchange: RabbitMQConfig.UsuariosExchange,
			routingKey: RabbitMQConfig.UsuariosToValidationKey);

		await channel.QueueBindAsync(
			queue: RabbitMQConfig.ValidatedUsuariosQueue,
			exchange: RabbitMQConfig.ValidationExchange,
			routingKey: RabbitMQConfig.ValidatedUsuariosKey);

		// Consumer
		var consumer = new AsyncDefaultBasicConsumer(channel);
		consumer.ConsumedAsync += async (model, ea) =>
		{
			var message = Message<Usuario>.Deserialize(ea.Body.ToArray());
			var usuario = message.Data;

			Console.WriteLine($"\n[x] Recebido: {usuario.NomeCompleto} para validação");

			var validator = new UsuariosValidator();
			bool isValid = validator.Validate(usuario, out string validationMessage);

			message.IsValid = isValid;
			message.ValidationMessage = validationMessage;

			// Enviar resposta validada
			var responseBytes = Message<Usuario>.Serialize(message);

			await channel.BasicPublishAsync(
				exchange: RabbitMQConfig.ValidationExchange,
				routingKey: RabbitMQConfig.ValidatedUsuariosKey,
				basicProperties: null,
				body: responseBytes);

			Console.WriteLine($"[x] Enviando resultado de validação para usuário {usuario.NomeCompleto}: {(isValid ? "Válido" : "Inválido")}");
			if (!isValid)
				Console.WriteLine($"    Motivo: {validationMessage}");

			await channel.BasicAckAsync(ea.DeliveryTag, false);
		};

		var consumerTag = await channel.BasicConsumeAsync(
			queue: RabbitMQConfig.UsuariosToValidationQueue,
			autoAck: false,
			consumer: consumer);
	}
}