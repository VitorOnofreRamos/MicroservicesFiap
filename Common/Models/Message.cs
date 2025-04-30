using System.Text;
using System.Text.Json;

namespace Common.Models;

public class Message<T>
{
    public T Data { get; set; }
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; }

    public static byte[] Serialize(Message<T> message)
    {
        var json = JsonSerializer.Serialize(message);
        return Encoding.UTF8.GetBytes(json);
    }

    public static Message<T> Deserialize(byte[] messageBody)
    {
        var json = Encoding.UTF8.GetString(messageBody);
        return JsonSerializer.Deserialize<Message<T>>(json);
    }
}
