// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Proxy
{
    using System;
    using DotNetty.Transport.Channels;
    using DotNetty.Buffers;

    public class HexDumpProxyBackendHandler : ChannelHandlerAdapter
    {
        private IChannel inboundChannel;

        public HexDumpProxyBackendHandler(IChannel inboundChannel)
        {
            this.inboundChannel = inboundChannel;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            context.Read();
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            this.inboundChannel.WriteAndFlushAsync(message)
                .ContinueWith(result => context.Read());
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("{0}", exception.StackTrace);
            context.WriteAndFlushAsync(Unpooled.Empty).Wait();
            context.CloseAsync();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            context.WriteAndFlushAsync(Unpooled.Empty).Wait();
            context.CloseAsync();
        }
    }
}
