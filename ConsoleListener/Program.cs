// See https://aka.ms/new-console-template for more information

using Grpc.Core;
using Grpc.Net.Client;
using GrpcGreeterProto;

const string MY_ID = "ConsoleListener";

using var channel = GrpcChannel.ForAddress("https://localhost:7261/");
var client = new Greeter.GreeterClient(channel);

var subscription = client.ListenToNotifications(new RequesterId { Id = MY_ID });

await foreach(var notification in subscription.ResponseStream.ReadAllAsync())
{
  Console.WriteLine($"[{notification.Sent.ToDateTime().ToLocalTime().ToLongTimeString()}][{DateTime.Now.ToLongTimeString()}] Notification from {notification.From}: {notification.Message}");
}
