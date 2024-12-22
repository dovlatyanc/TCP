using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TcpLib
{
    public static class Tcp
    {
        public static async Task SendInt32(this TcpClient client, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            await client.GetStream().WriteAsync(bytes, 0, bytes.Length);//отправить 4 бвайта
        }
        public static async Task SendBytes(this TcpClient client, byte[] bytes)
        {
            await client.SendInt32(bytes.Length);//сначала длину
            await client.GetStream().WriteAsync(bytes, 0, bytes.Length);// потом содержимое
        }

        public static async Task SendString(this TcpClient client, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);//превратить в массив байтов
            await client.SendBytes(bytes);// отправить массив байтов( с длиной)
        }
        public static async Task<int> ReceiveInt32(this TcpClient client)
        {
            byte[] bytes = new byte[sizeof(int)];
            await client.GetStream().ReadExactlyAsync(bytes, 0, bytes.Length);
            return BitConverter.ToInt32(bytes);

        }

        public static async Task SendJson<T>(this TcpClient client, T item)
        {
            //превратим в строку
            string json = JsonSerializer.Serialize(item);
            // отправим строку
            await client.SendString(json);

        }

        public static async Task SendFile(this TcpClient client, Stream file)
        {
            int lenght = (int)file.Length;// < 2 gb
            await client.SendInt32(lenght);

            byte[] buffer = new byte[1024];
            int sent = 0;
            while (sent < lenght)
            {
                // зачепнуть не больше чем осталось и не больше чем объем ведра
                int read = await file.ReadAsync(buffer, 0, Math.Min(buffer.Length, lenght - sent));
                await client.GetStream().WriteAsync(buffer, 0, read);//отправить ведро без префикса

            }
        }
        public static async Task<byte[]> ReceiveBytes(this TcpClient client)
        {
            int lenght = await client.ReceiveInt32();//узнать длину предстоящего блока
            byte[] bytes = new byte[lenght];
            await client.GetStream().ReadExactlyAsync(bytes, 0, bytes.Length);
            return bytes;
        }
        public static async Task<string> ReceiveString(this TcpClient client)
        {
            //получить массив байтов
            byte[] bytes = await client.ReceiveBytes();
            //декодировать и вернуть
            return Encoding.UTF8.GetString(bytes);
        }
        public static async Task<T> ReceiveJson<T>(this TcpClient client)
        {
            //получить строку json 
            string json = await client.ReceiveString();
            //десириализовать 
            T item = JsonSerializer.Deserialize<T>(json) ?? throw new NullReferenceException();

            return item;


        }
        public static async Task ReceiveFile(this TcpClient client, Stream stream)
        {
            int lenght = await client.ReceiveInt32();
            byte[] buffer = new byte[1024];
            int pos = 0;
            while (pos < lenght)
            {
                int remaining = Math.Min(buffer.Length, lenght - pos);
                int read = await client.GetStream().ReadAsync(buffer, 0, remaining);
                await stream.WriteAsync(buffer, 0, read);//сколько зачерпнуоли столько и вылили в файл                                              
                pos += read;
            }


        }
    }
}
