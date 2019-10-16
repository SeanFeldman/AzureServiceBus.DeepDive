namespace Receiving
{
    using System.Text;

    public static class MessageExtensions
    {
        public static string AsString(this byte[] messageBody)
        {
            return Encoding.UTF8.GetString(messageBody);
        }

        public static byte[] AsByteArray(this string body)
        {
            return Encoding.UTF8.GetBytes(body);
        }
    }
}