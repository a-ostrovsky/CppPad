﻿namespace CppPad.Configuration.Json;


[Serializable]
public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException() { }

    public InvalidConfigurationException(string message) : base(message) { }

    public InvalidConfigurationException(string message, Exception inner) : base(message, inner) { }
}
