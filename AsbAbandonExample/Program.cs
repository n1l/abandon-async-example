using Microsoft.Azure.ServiceBus;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace AsbAbandonExample
{
    class Program
    {
        static QueueClient _queue;

        static void Main(string[] args)
        {
            string ServiceBusConnectionString = ConfigurationManager.AppSettings["AsbConnectionString"];
            string QueueName = ConfigurationManager.AppSettings["QueueName"];

            MainAsync(ServiceBusConnectionString, QueueName).GetAwaiter().GetResult();

            Console.ReadKey();
        }

        static async Task MainAsync(string ServiceBusConnectionString, string QueueName)
        {
            _queue = new QueueClient(ServiceBusConnectionString, QueueName, ReceiveMode.PeekLock);

            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await _queue.CloseAsync();
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false
            };

            _queue.RegisterMessageHandler(
                async (message, token) =>
                {
                    try
                    {
                        throw new Exception("Test");
                    }
                    catch (Exception)
                    {
                        await _queue.AbandonAsync(message.SystemProperties.LockToken);
                        throw;
                    }
                },
                messageHandlerOptions);
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine(exceptionReceivedEventArgs.Exception);
            throw exceptionReceivedEventArgs.Exception;
        }
    }
}
