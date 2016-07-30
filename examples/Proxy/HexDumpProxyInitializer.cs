// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Proxy
{
    using System;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using DotNetty.Handlers.Logging;

    class HexDumpProxyInitializer : ChannelInitializer<ISocketChannel>
    {
        readonly string remoteHost;
        readonly int remotePort;

        public HexDumpProxyInitializer(string remoteHost, int remotePort)
        {
            this.remoteHost = remoteHost;
            this.remotePort = remotePort;
        }

        protected override void InitChannel(ISocketChannel channel)
        {
            channel.Pipeline.AddLast(new LoggingHandler(LogLevel.INFO),
                new HexDumpProxyFrontendHandler(this.remoteHost, this.remotePort));
        }
    }
}
