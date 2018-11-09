namespace PhotoMalfunctionDetection
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    class Program
    {
        static int counter;
        static List<Guid> photoEventsTriggered;

        //ride start
        //ride end
        //lift start
        //lift end
        //photo triggered


        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            #region boilerplate
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");
            var pipeMessage = new Message(messageBytes);

            if (!string.IsNullOrEmpty(messageString))
            {
                var tevent = JsonConvert.DeserializeObject<trainevent>(messageString);
                if(tevent != null)
                {
                    if (tevent.EventType == "PhotoTriggered")
                    {
                        photoEventsTriggered.Add(tevent.CorrelationId);
                        
                    }
                    else if(tevent.EventType == "RideEnd")
                    {
                        if(photoEventsTriggered.Contains(tevent.CorrelationId))
                        {
                            photoEventsTriggered.Remove(tevent.CorrelationId);//RemoveAt(photoEventsTriggered.IndexOf(tevent.CorrelationId))
                        }
                        else
                        {
                            //await SetAlarm(tevent.TrainId, tevent.CorrelationId, tevent.DeviceTime);
                            var msgString = JsonConvert.SerializeObject(tevent);
                            //pipeMessage = mess
                            
                            await moduleClient.SendEventAsync("output1", pipeMessage);
                        }

                    }
                }
                
                //foreach (var prop in message.Properties)
                //{
                //    pipeMessage.Properties.Add(prop.Key, prop.Value);
                //}
                //await moduleClient.SendEventAsync("output1", pipeMessage);
                Console.WriteLine("Received message sent");
            }
            #endregion



            return MessageResponse.Completed;
        }

        //private static async Task SetAlarm(Guid trainId, Guid correlationId, DateTime deviceTime)
        //{
        //    await Task.FromException(new NotImplementedException());
        //    await moduleClient.SendEventAsync("output1", pipeMessage);
        //}
    }
}
