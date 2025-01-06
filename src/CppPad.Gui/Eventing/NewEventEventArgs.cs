namespace CppPad.Gui.Eventing;

public class NewEventEventArgs(IEvent @event) : EventBus
{
    public IEvent Event { get; } = @event;
}
