namespace CppPad.Common;

public interface ITimer
{
    void Change(TimeSpan dueTime, TimeSpan period);

    event EventHandler? Elapsed;
}