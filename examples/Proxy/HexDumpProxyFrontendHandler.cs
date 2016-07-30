// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Proxy
{
    using System;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels.Sockets;
    using DotNetty.Buffers;

    public class HexDumpProxyFrontendHandler : ChannelHandlerAdapter
    {
        readonly string remoteHost;
        readonly int remotePort;
        IChannel outboundChannel;

        public HexDumpProxyFrontendHandler(string remoteHost, int remotePort)
        {
            this.remoteHost = remoteHost;
            this.remotePort = remotePort;
        }

        public async override void ChannelActive(IChannelHandlerContext context)
        {
            var inboundChannel = context.Channel;

            var bootstrap = new Bootstrap();
            bootstrap.Group(HexDumpProxy.EVENT_LOOP_GROUP)
                .Channel<TcpServerSocketChannel>()
                .Handler(new HexDumpProxyBackendHandler(inboundChannel))
                .Option(ChannelOption.AutoRead, false);

            try
            {
                this.outboundChannel = await bootstrap.ConnectAsync(this.remoteHost, this.remotePort);
                this.outboundChannel.Read();
            }
            catch
            {
                await inboundChannel.CloseAsync();
            }
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (this.outboundChannel != null && this.outboundChannel.Active)
            {
                try
                {
                    this.outboundChannel.WriteAndFlushAsync(message).Wait();
                    context.Channel.Read();
                }
                catch
                {
                    context.CloseAsync();
                }
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine(exception.ToString());

        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            if (this.outboundChannel != null)
            {
                this.outboundChannel.WriteAndFlushAsync(Unpooled.Empty)
                    .ContinueWith(result => this.outboundChannel.CloseAsync());
            }
            this.outboundChannel = null;
        }
    }
}
