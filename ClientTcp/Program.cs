using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ServerTcp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string ip;
                {
                    Console.Write("Введите ip: ");
                    ip = Console.ReadLine();
                }
                int port;
                {
                    Console.Write("Введите номер порта (8000): ");
                    port = Convert.ToInt32(Console.ReadLine());
                }
                //const string ip = "192.168.1.58"; // local host //127.0.0.1
                //const int port = 8000; // 8080

                var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSocket.Connect(tcpEndPoint);
                Console.WriteLine("Подключение установленно");
                while (true)
                {
                    try
                    {
                        byte codeOperation;
                        {
                            Console.WriteLine("1 - Найти среднее арифметическое чисел файла");
                            Console.WriteLine("2 - Написать сообщение");
                            Console.WriteLine("3 - Выйти");
                            Console.Write("Введите код операции: ");
                            codeOperation = Convert.ToByte(Console.ReadLine());
                        }

                        if (codeOperation == 1 || codeOperation == 2 || codeOperation == 3)
                        {
                            tcpSocket.Send(new byte[] { codeOperation }); // отправка коды операции

                            if (codeOperation == 1)
                            {
                                byte[] data;
                                {
                                    Console.Write("Путь к файлу: ");
                                    var path = Console.ReadLine();
                                    data = Encoding.UTF8.GetBytes(path);
                                }
                                tcpSocket.Send(data);

                                StringBuilder fileText = GetMessage(tcpSocket);
                                Console.WriteLine("Среднее арифметическое чисел файла = " + ArithmeticMean(fileText));
                            }
                            else if (codeOperation == 2)
                            {
                                byte[] data;
                                {
                                    Console.Write("Введите сообщение: ");
                                    string message = Console.ReadLine();
                                    data = Encoding.UTF8.GetBytes(message);
                                }
                                tcpSocket.Send(data);
                            }
                            else if (codeOperation == 3)
                            {
                                tcpSocket.Shutdown(SocketShutdown.Both);
                                tcpSocket.Close();
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
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

        private static double ArithmeticMean(StringBuilder fileText)
        {
            fileText.Append("\n");

            Regex regex = new Regex(@"(.*?)\n");
            MatchCollection matchCollection = regex.Matches(fileText.ToString());

            double arithmeticMean = default(double);
            if (matchCollection.Count > 0)
            {
                try
                {
                    for (int i = 0; i < matchCollection.Count; i++)
                    {
                        arithmeticMean += Convert.ToDouble(matchCollection[i].Groups[1].Value.Trim());
                    }
                    arithmeticMean /= matchCollection.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return arithmeticMean;
        }
    }
}