namespace CppPad.Common;

/// <summary>
///     Represents an asynchronous event handler
///     that returns a Task when handling the event.
/// </summary>
public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);

/// <summary>
///     Represents an asynchronous event handler
///     that returns a Task when handling the event.
/// </summary>
public delegate Task AsyncEventHandler(object sender, EventArgs e);

public static class AsyncEventHandlerExtensions
{
    /// <summary>
    ///     Invokes all async event handlers in parallel and awaits them.
    /// </summary>
    public static async Task InvokeAsync<TEventArgs>(
        this AsyncEventHandler<TEventArgs>? handler,
        object sender,
        TEventArgs e
    )
    {
        if (handler is null)
        {
            return;
        }

        // GetInvocationList() returns an array of delegates subscribed to this event
        var invocationList = handler.GetInvocationList();

        // Convert each delegate back to AsyncEventHandler<TEventArgs> and invoke it
        var handlerTasks = invocationList.Select(d =>
            ((AsyncEventHandler<TEventArgs>)d)(sender, e)
        );

        // Wait until all event handlers have finished
        await Task.WhenAll(handlerTasks);
    }

    /// <summary>
    ///     Invokes all async event handlers in parallel and awaits them.
    /// </summary>
    public static async Task InvokeAsync(
        this AsyncEventHandler? handler,
        object sender,
        EventArgs e
    )
    {
        if (handler is null)
        {
            return;
        }

        // Get all delegates subscribed to this event
        var invocationList = handler.GetInvocationList();

        // Convert each delegate back to AsyncEventHandler and invoke it
        var tasks = invocationList.Select(d => ((AsyncEventHandler)d)(sender, e));

        // Wait until all event handlers have finished
        await Task.WhenAll(tasks);
    }
}
