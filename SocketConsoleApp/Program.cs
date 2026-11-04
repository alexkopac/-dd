using System;
using System.Net.Sockets;
using System.Text;

namespace SocketHttpApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== Мережеве програмування: HTTP-запит через Socket ===");
            Console.Write("Введіть URL або адресу сайту (наприклад, example.com або http://example.com): ");

            string? input = Console.ReadLine()?.Trim();

            // Якщо користувач ничего не ввів, використовуємо example.com за замовчуванням
            if (string.IsNullOrWhiteSpace(input))
            {
                input = "http://example.com";
                Console.WriteLine($"Адресу не вказано. Використовується за замовчуванням: {input}");
            }

            // Додаємо протокол http://, якщо користувач ввів лише домен
            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                input = "http://" + input;
            }

            // Валідація та створення об'єкта Uri
            if (!Uri.TryCreate(input, UriKind.Absolute, out Uri? targetUri))
            {
                Console.WriteLine("Помилка: Некоректний формат URL.");
                return;
            }

            int port = targetUri.Port > 0 ? targetUri.Port : 80;

            Console.WriteLine($"\n[+] Підключення до {targetUri.Host}:{port}...");
            Console.WriteLine(new string('-', 60));

            try
            {
                SendHttpRequest(targetUri, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[!] Помилка підключення/передачі даних: {ex.Message}");
            }

            Console.WriteLine("\n" + new string('-', 60));
            Console.WriteLine("Запит завершено. Натисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }

        /// <summary>
        /// Реалізація прикладу з документації Microsoft
        /// </summary>
        private static void SendHttpRequest(Uri uri, int port = 80)
        {
            // Формування мінімалістичного HTTP/1.0 GET-запиту
            string path = string.IsNullOrEmpty(uri.PathAndQuery) ? "/" : uri.PathAndQuery;
            byte[] requestBytes = Encoding.ASCII.GetBytes(
                $"GET {path} HTTP/1.0\r\n" +
                $"Host: {uri.Host}\r\n" +
                $"Connection: Close\r\n\r\n"
            );

            // Створення та підключення сокета (Dual-stack IPv4/IPv6)
            using Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(uri.Host, port);

            // Відправка HTTP-запиту в сокет
            int bytesSent = 0;
            while (bytesSent < requestBytes.Length)
            {
                bytesSent += socket.Send(requestBytes, bytesSent, requestBytes.Length - bytesSent, SocketFlags.None);
            }

            // Буферизація та читання ASCII-відповіді від сервера
            byte[] responseBytes = new byte[1024];
            char[] responseChars = new char[1024];

            while (true)
            {
                int bytesReceived = socket.Receive(responseBytes);

                // Отримання 0 байт означає кінець потоку (EOF)
                if (bytesReceived == 0) break;

                // Конвертація байтів у символи
                int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);

                // Вивід отриманої HTML-сторінки та заголовків у консоль
                Console.Out.Write(responseChars, 0, charCount);
            }
        }
    }
}