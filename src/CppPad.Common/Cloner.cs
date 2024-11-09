#region

using System.Text.Json;

#endregion

namespace CppPad.Common;

public static class Cloner
{
    public static T? DeepCopy<T>(T? obj)
    {
        if (obj == null)
        {
            return default;
        }

        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json);
    }
}