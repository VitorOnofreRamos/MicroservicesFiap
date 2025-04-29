using RabbitMQ.Client;

namespace Common.RabbitMQ;

/// <summary>
/// Async-ready wrapper for RabbitMQ connections and channels.
/// </summary>
public class RabbitMQConnection : IAsyncDisposable
{
    private IConnection _connection;
    private bool _disposed;

    private RabbitMQConnection(IConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Factory method to create and open a connection asynchornously.
    /// </summary>
    public static async Task<RabbitMQConnection> CreateAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = RabbitMQConfig.HostName,
            UserName = RabbitMQConfig.UserName,
            Password = RabbitMQConfig.Password
        };

        // Open connection asynchronously
        IConnection connection = await factory.CreateConnectionAsync().ConfigureAwait(false);
        new RabbitMQConnection(connection);
    }

    /// <summary>
    /// Create a channel asynchronously.
    /// </summary>
    public async Task<IChannel> CreateChannelAsync()
    {
        return await _connection.CreateChannelAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Releases RabbitMQ resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _connection.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
