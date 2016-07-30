// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Proxy
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Threading.Tasks;
    using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels.Sockets;
    using DotNetty.Handlers.Logging;

    class HexDumpProxy
    {
        static readonly int LOCAL_PORT = 8443;
        static readonly string REMOTE_HOST = "www.google.com.sg";
        static readonly ushort REMOTE_PORT = 443;
        public static readonly IEventLoopGroup EVENT_LOOP_GROUP = new MultithreadEventLoopGroup();

        static async Task RunServerAsync()
        {
            var eventListener = new ObservableEventListener();
            eventListener.LogToConsole();
            eventListener.EnableEvents(DefaultEventSource.Log, EventLevel.LogAlways);

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(EVENT_LOOP_GROUP, EVENT_LOOP_GROUP)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler(LogLevel.INFO))
                    .ChildHandler(new HexDumpProxyInitializer(REMOTE_HOST, REMOTE_PORT))
                    .ChildOption(ChannelOption.AutoRead, false);

                IChannel bootstrapChannel = await bootstrap.BindAsync(LOCAL_PORT);

                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                Task.WaitAll(EVENT_LOOP_GROUP.ShutdownGracefullyAsync());
                eventListener.Dispose();
            }
        }

        static void Main() => RunServerAsync().Wait();
    }
}
