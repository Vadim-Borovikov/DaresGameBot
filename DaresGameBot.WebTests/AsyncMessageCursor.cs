using System.Threading.Channels;
using TL;

namespace DaresGameBot.WebTests;

internal sealed class AsyncMessageStream
{
    public AsyncMessageStream() => _reader = _channel.Reader;

    public void Write(Message message) => _channel.Writer.TryWrite(message);

    public async Task SkipAsync(int count)
    {
        await foreach (Message _ in ReadAllAsync())
        {
            --count;
            if (count == 0)
            {
                break;
            }
        }
    }

    public async Task<Message?> ReadNextAsync()
    {
        await foreach (Message msg in ReadAllAsync())
        {
            return msg;
        }

        return null;
    }

    private async IAsyncEnumerable<Message> ReadAllAsync()
    {
        while (await _reader.WaitToReadAsync())
        {
            while (_reader.TryRead(out Message? message))
            {
                yield return message;
            }
        }
    }

    private readonly Channel<Message> _channel = System.Threading.Channels.Channel.CreateUnbounded<Message>();
    private readonly ChannelReader<Message> _reader;
}