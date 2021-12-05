using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerTcp
{
    class Program
    {
        static void Main(string[] args)
        {
            //const string ip = "127.0.0.1"; // local host
            const int port = 8000; // 8080
            IPAddress ip = GetLocalIP();
            var tcpEndPoint = new IPEndPoint(ip, port);// end point - точка подключения  // IPAddress.Parse(ip)

            // создание сокета ожидающего приема сообщения
            var tcpSocet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// socet - через него устанавливается соединение

            try
            {
                tcpSocet.Bind(tcpEndPoint); // перевод сокета в ожидание подключения

                tcpSocet.Listen(5); // запуск сокета на простушивание (ожидание подключения)
                                    // перегрузка (максимум клиентов в очереди подключения)

                WriteWithTime("Сервер запущен. Ожидание подключений...");

                var listener = tcpSocet.Accept(); // новый подсокет для обрабатывания клиента
                                                  // под каждого клиента свой 

                WriteWithTime("Подключение установленно");
                while (true) // ожидание
                {
                    byte codeOperation;
                    {
                        byte[] buffer = new byte[1];
                        int size = listener.Receive(buffer);
                        codeOperation = buffer[0];
                    }
                    if (codeOperation == 1 || codeOperation == 2 || codeOperation == 3)
                    {
                        if (codeOperation == 1)
                        {
                            string filePath = GetMessage(listener).ToString();

                            WriteWithTime("Запрос на обработку файла: " + filePath);

                            byte[] array;
                            using (FileStream fStriam = new FileStream(filePath, FileMode.Open))
                            {
                                array = new byte[fStriam.Length];
                                fStriam.Read(array, 0, array.Length);
                            }
                            listener.Send(array);
                        }
                        else if (codeOperation == 2)
                        {
                            WriteWithTime(GetMessage(listener).ToString());
                        }
                        else if (codeOperation == 3)
                        {
                            WriteWithTime("Клиент отключен");
                            listener.Shutdown(SocketShutdown.Both);
                            listener.Close();

                            WriteWithTime("Ожидание подключений...");
                            listener = tcpSocet.Accept(); // ожидание нового подключения
                            WriteWithTime("Подключение установленно");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static StringBuilder GetMessage(Socket listener)
        {
            byte[] buffer = new byte[256];
            int messageSize;
            StringBuilder data = new StringBuilder();
            do
            {
                messageSize = listener.Receive(buffer);
                data.Append(Encoding.UTF8.GetString(buffer, 0, messageSize));
            }
            while (listener.Available > 0);
            return data;
        }

        private static IPAddress GetLocalIP()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

        private static void WriteWithTime(string str)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + str);
        }
    }
}

