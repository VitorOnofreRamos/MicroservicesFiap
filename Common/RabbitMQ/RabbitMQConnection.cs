using RabbitMQ.Client;

namespace Common.RabbitMQ;

public class RabbitMQConnection : IDisposable
{
    private IConnection _connection;
    private IChannel _channel;
    private bool _disposed;

    public RabbitMQConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = RabbitMQConfig.HostName,
            UserName = RabbitMQConfig.UserName,
            Password = RabbitMQConfig.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public void Dispose() 
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _connection?.Dispose();

        _disposed = true;
    }
}
