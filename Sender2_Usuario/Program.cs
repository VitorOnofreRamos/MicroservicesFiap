using Common.Models;
using Common.RabbitMQ;
using RabbitMQ.Client;

namespace Sender2_Usuario;

class Program
{
	static async Task Main(string[] args)
	{
		Console.WriteLine("=== Sender 2 - Dados de Usuário ===");

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

		// Bind Queue to Exchange
		await channel.QueueBindAsync(
			queue: RabbitMQConfig.UsuariosToValidationQueue,
			exchange: RabbitMQConfig.UsuariosExchange,
			routingKey: RabbitMQConfig.UsuariosToValidationKey);

		while (true)
		{
			Console.WriteLine("\nDigite os dados do usuário");
			Console.Write("Nome completo: ");
			string nome = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(nome))
			{
				Console.WriteLine("Encerrando aplicação...");
				break;
			}

			Console.Write("Endereço: ");
			string endereco = Console.ReadLine();

			Console.Write("RG: ");
			string rg = Console.ReadLine();

			Console.Write("CPF: ");
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
				body: messageBytes);

			Console.WriteLine($"[x] Enviando usuário {usuario.NomeCompleto} para validação em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
		}

		await rabbitConnection.DisposeAsync();
	}
}