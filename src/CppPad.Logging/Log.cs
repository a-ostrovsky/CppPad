using Microsoft.Extensions.Logging;

namespace CppPad.Logging;

public static class Log
{
    private static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>
        builder.AddConsole()
    );

    public static ILogger CreateLogger<T>() => Factory.CreateLogger<T>();
}
