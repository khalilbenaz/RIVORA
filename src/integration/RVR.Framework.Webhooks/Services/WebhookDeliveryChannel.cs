using System.Threading.Channels;
using RVR.Framework.Webhooks.Models;

namespace RVR.Framework.Webhooks.Services;

/// <summary>
/// In-process channel for queuing webhook delivery tasks. Used to decouple
/// publishing from delivery so that HTTP sends happen in the background.
/// </summary>
public sealed class WebhookDeliveryChannel
{
    private readonly Channel<WebhookDeliveryTask> _channel;

    /// <summary>
    /// Initializes a new instance of <see cref="WebhookDeliveryChannel"/> with a bounded capacity.
    /// </summary>
    /// <param name="capacity">Maximum number of pending tasks. Defaults to 10 000.</param>
    public WebhookDeliveryChannel(int capacity = 10_000)
    {
        _channel = Channel.CreateBounded<WebhookDeliveryTask>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    /// <summary>Gets the writer side of the channel.</summary>
    public ChannelWriter<WebhookDeliveryTask> Writer => _channel.Writer;

    /// <summary>Gets the reader side of the channel.</summary>
    public ChannelReader<WebhookDeliveryTask> Reader => _channel.Reader;
}
