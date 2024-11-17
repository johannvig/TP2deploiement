using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace ServiceBusApp
{
    class Program
    {
        private const string connectionString = "Endpoint=sb://azuresb-demo.servicebus.windows.net/;SharedAccessKeyName=AccessPolicy;SharedAccessKey=W8+tkazvT4L3D8Z9qRP7D1faJyQavR+ab+ASbJFalKU=;EntityPath=messagequeue";
        private const string queueName = "messagequeue";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Start : " + DateTime.Now.ToString());

            // Créer le client de la file d'attente
            ServiceBusClient client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);

            // Envoyer un message à la file d'attente
            string messageBody = "Hello, Azure Service Bus!";
            ServiceBusMessage message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
            Console.WriteLine($"Message sent: {messageBody}");

            // Recevoir le message de la file d'attente
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();
            Console.WriteLine($"Message received: {receivedMessage.Body}");

            // Compléter le message pour le supprimer de la file d'attente
            await receiver.CompleteMessageAsync(receivedMessage);
            Console.WriteLine("Message completed.");
            Console.ReadLine();
        }
    }
}