namespace CppPad.EnvironmentConfiguration;

public class EnvironmentConfigurationException : Exception
{
    public EnvironmentConfigurationException()
    {
    }

    public EnvironmentConfigurationException(string message) : base(message)
    {
    }

    public EnvironmentConfigurationException(string message, Exception inner) : base(message, inner)
    {
    }
}