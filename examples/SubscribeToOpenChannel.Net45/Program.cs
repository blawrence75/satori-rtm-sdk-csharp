﻿using System;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json.Linq;
using Satori.Rtm;
using Satori.Rtm.Client;

namespace SubscribeToOpenChannel
{
    class Program
    {
        const string endpoint = "wss://open-data.api.satori.com";
        const string appkey = "YOUR_APPKEY";
        const string channel = "OPEN_CHANNEL";

        static void Main()
        {
            // Log messages from SDK to the console
            Trace.Listeners.Add(new ConsoleTraceListener());

            IRtmClient client = new RtmClientBuilder(endpoint, appkey).Build();

            client.OnEnterConnected += cn => Console.WriteLine("Connected to RTM");
            client.OnError += ex => Console.Error.WriteLine("Error occurred: " + ex.Message);

            client.Start();

            // Create subscription observer to observe channel subscription events 
            var observer = new SubscriptionObserver();

            observer.OnEnterSubscribed += (ISubscription sub) => 
                Console.WriteLine("Subscribed to: " + sub.SubscriptionId);
            
            observer.OnSubscriptionData += (ISubscription sub, RtmSubscriptionData data) =>
            {
                foreach(JToken msg in data.Messages) 
                {
                    Console.WriteLine("Got message: " + msg);
                }
            };

            observer.OnSubscribeError += (ISubscription sub, Exception err) => 
            {
                var rtmEx = err as SubscribeException;
                if (rtmEx != null) 
                    Console.WriteLine("Subscribing failed because RTM replied with the error {0}: {1}", rtmEx.Error.Code, rtmEx.Error.Reason);
                else 
                    Console.WriteLine("Subscribing failed: " + err.Message);
            };

            observer.OnSubscriptionError += (ISubscription sub, RtmSubscriptionError err) => 
                Console.WriteLine("Subscription failed because RTM sent the error {0}: {1}", err.Code, err.Reason);

            client.CreateSubscription(channel, SubscriptionModes.Simple, observer);

            // Do not exit the program
            new ManualResetEvent(false).WaitOne();
        }
    }
}