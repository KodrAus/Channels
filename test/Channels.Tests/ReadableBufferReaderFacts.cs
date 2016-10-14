﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Channels.Tests
{
    public class ReadableBufferReaderFacts
    {
        [Fact]
        public void PeekReturnsByteWithoutMoving()
        {
            var reader = new ReadableBufferReader(ReadableBuffer.Create(new byte[] { 1, 2 }, 0, 2));
            Assert.Equal(1, reader.Peek());
            Assert.Equal(1, reader.Peek());
        }

        [Fact]
        public void TakeReturnsByteAndMoves()
        {
            var reader = new ReadableBufferReader(ReadableBuffer.Create(new byte[] { 1, 2 }, 0, 2));
            Assert.Equal(1, reader.Take());
            Assert.Equal(2, reader.Take());
            Assert.Equal(-1, reader.Take());
        }

        [Fact]
        public void PeekReturnsMinuOneByteInTheEnd()
        {
            var reader = new ReadableBufferReader(ReadableBuffer.Create(new byte[] { 1, 2 }, 0, 2));
            Assert.Equal(1, reader.Take());
            Assert.Equal(2, reader.Take());
            Assert.Equal(-1, reader.Peek());
        }

        [Fact]
        public async Task TakeTraversesSegments()
        {
            using (var channelFactory = new ChannelFactory())
            {
                var channel = channelFactory.CreateChannel();
                var w = channel.Alloc();
                w.Append(ReadableBuffer.Create(new byte[] { 1 }, 0, 1));
                w.Append(ReadableBuffer.Create(new byte[] { 2 }, 0, 1));
                w.Append(ReadableBuffer.Create(new byte[] { 3 }, 0, 1));
                await w.FlushAsync();

                var result = await channel.ReadAsync();
                var buffer = result.Buffer;
                var reader = new ReadableBufferReader(buffer);

                Assert.Equal(1, reader.Take());
                Assert.Equal(2, reader.Take());
                Assert.Equal(3, reader.Take());
                Assert.Equal(-1, reader.Take());
            }
        }

        [Fact]
        public async Task PeekTraversesSegments()
        {
            using (var channelFactory = new ChannelFactory())
            {
                var channel = channelFactory.CreateChannel();
                var w = channel.Alloc();
                w.Append(ReadableBuffer.Create(new byte[] { 1 }, 0, 1));
                w.Append(ReadableBuffer.Create(new byte[] { 2 }, 0, 1));
                await w.FlushAsync();

                var result = await channel.ReadAsync();
                var buffer = result.Buffer;
                var reader = new ReadableBufferReader(buffer);

                Assert.Equal(1, reader.Take());
                Assert.Equal(2, reader.Peek());
                Assert.Equal(2, reader.Take());
                Assert.Equal(-1, reader.Peek());
                Assert.Equal(-1, reader.Take());
            }
        }

        [Fact]
        public async Task PeekWorkesWithEmptySegments()
        {
            using (var channelFactory = new ChannelFactory())
            {
                var channel = channelFactory.CreateChannel();
                var w = channel.Alloc();
                w.Append(ReadableBuffer.Create(new byte[] { 0 }, 0, 0));
                w.Append(ReadableBuffer.Create(new byte[] { 1 }, 0, 1));
                await w.FlushAsync();

                var result = await channel.ReadAsync();
                var buffer = result.Buffer;
                var reader = new ReadableBufferReader(buffer);

                Assert.Equal(1, reader.Peek());
                Assert.Equal(1, reader.Take());
                Assert.Equal(-1, reader.Peek());
                Assert.Equal(-1, reader.Take());
            }
        }

        [Fact]
        public void WorkesWithEmptyBuffer()
        {
            var reader = new ReadableBufferReader(ReadableBuffer.Create(new byte[] { 0 }, 0, 0));

            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Take());
        }
    }
}
