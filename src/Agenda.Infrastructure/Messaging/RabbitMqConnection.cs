using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Agenda.Infrastructure.Messaging;

public sealed class RabbitMqConnection : IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RabbitMqConnection(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IChannel> CreateChannelAsync(CancellationToken ct = default)
    {
        if (_connection is null)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                _connection ??= await OpenConnectionAsync(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    private Task<IConnection> OpenConnectionAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true,
        };
        return factory.CreateConnectionAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
