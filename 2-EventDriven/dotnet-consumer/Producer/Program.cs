using System.Text;
using System.Text.Json;
using CodeAcademy.DotnetConsumer.Common.Config;
using RabbitMQ.Client;

Console.WriteLine("Producer starting...");

// Establish connection to RabbitMQ
using var connection = await ConnectionHelper.ConnectAsync();
Console.WriteLine("Connected to RabbitMQ");

// Implement a basic producer here.
// Start with:
// - Create a channel
// - Declare a queue
// - Publish a message to the queue (you can use a simple JSON string as the message body)


// Create a channel and declare the queue
using var channel = await connection.CreateChannelAsync();

string exchangeName = "chat";

//await channel.QueueDeclareAsync(
//    queue: "idem-events",
//    durable: true,
//    exclusive: false,
//    autoDelete: false,
//    arguments: null
//);

// Fanout Exchange
await channel.ExchangeDeclareAsync(
    exchange: exchangeName,
    type: ExchangeType.Fanout,
    durable: false,
    autoDelete: false,
    arguments: null
);

// Direct Exchange
//await channel.ExchangeDeclareAsync(
//    exchange: exchangeName,
//    type: ExchangeType.Direct,
//    durable: true,
//    autoDelete: false,
//    arguments: null
//);

// Topic Exchange
//await channel.ExchangeDeclareAsync(
//    exchange: exchangeName,
//    type: ExchangeType.Topic,
//    durable: true,
//    autoDelete: false,
//    arguments: null
//);

// Headers Exchange
//await channel.ExchangeDeclareAsync(
//    exchange: exchangeName,
//    type: ExchangeType.Headers,
//    durable: true,
//    autoDelete: false,
//    arguments: null
//);

// Publish messages to the queue with for loop to simulate multiple events
for (int i = 0; i < 10; i++)
{
    //var message = $"Idems Event {i + 1} at {DateTime.Now}";
    var message = "Jeløy Yacht og Champagne";

    var messageBody = JsonSerializer.Serialize(message);
    var body = Encoding.UTF8.GetBytes(messageBody);

    await channel.BasicPublishAsync(
        exchange: exchangeName,
        //routingKey: i % 2 == 0 ? "hello" : "hello.there",
        routingKey: string.Empty,
        mandatory: true,
        basicProperties: new BasicProperties { Persistent = true },
        body: body
    );
    Console.WriteLine($"Published event: {message}");

    await Task.Delay(2000);
}

Console.WriteLine("Producer finished.");
