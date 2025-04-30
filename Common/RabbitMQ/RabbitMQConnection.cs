using RabbitMQ.Client;

namespace Common.RabbitMQ;

public class RabbitMQConnection : IDisposable
{
    private IConnection _connection;
    private bool _disposed;

    public RabbitMQConnection()
    {
        var factory = new ConnectionFactory()
        {
            HostName = RabbitMQConfig.HostName,
            UserName = RabbitMQConfig.UserName,
            Password = RabbitMQConfig.Password
        };

        _connection = factory.CreateConnection();
    }
    public IModel CreateChannel()
    {
        return _connection.CreateModel();
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
