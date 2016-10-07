﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using Channels.Networking.Libuv;
using Channels.Text.Primitives;

namespace Channels.Samples
{
    public class RawLibuvHttpServerSample
    {
        public static void Run()
        {
            var ip = IPAddress.Any;
            int port = 5000;
            var thread = new UvThread();
            var listener = new UvTcpListener(thread, new IPEndPoint(ip, port));
            listener.OnConnection(async connection =>
            {
                var httpParser = new HttpRequestParser();
                var formatter = connection.Output.GetFormatter(EncodingData.InvariantUtf8);

                try
                {
                    while (true)
                    {
                        httpParser.Reset();

                        // Wait for data
                        var result = await connection.Input.ReadAsync();
                        var input = result.Buffer;

                        try
                        {
                            if (input.IsEmpty && result.IsCompleted)
                            {
                                // No more data
                                break;
                            }

                            // Parse the input http request
                            var parseResult = httpParser.ParseRequest(ref input);

                            switch (parseResult)
                            {
                                case HttpRequestParser.ParseResult.Incomplete:
                                    if (result.IsCompleted)
                                    {
                                        // Didn't get the whole request and the connection ended
                                        throw new EndOfStreamException();
                                    }
                                    // Need more data
                                    continue;
                                case HttpRequestParser.ParseResult.Complete:
                                    break;
                                case HttpRequestParser.ParseResult.BadRequest:
                                    throw new Exception();
                                default:
                                    break;
                            }

                            formatter.Append("HTTP/1.1 200 OK");
                            formatter.Append("\r\nContent-Length: 13");
                            formatter.Append("\r\nContent-Type: text/plain");
                            formatter.Append("\r\n\r\n");
                            formatter.Append("Hello, World!");

                            await formatter.FlushAsync();

                        }
                        finally
                        {
                            // Consume the input
                            connection.Input.Advance(input.Start, input.End);
                        }
                    }
                }
                finally
                {
                    // Close the input channel, which will tell the producer to stop producing
                    connection.Input.Complete();

                    // Close the output channel, which will close the connection
                    connection.Output.Complete();
                }
            });

            listener.Start();

            Console.WriteLine($"Listening on {ip} on port {port}");
            var wh = new ManualResetEventSlim();
            Console.CancelKeyPress += (sender, e) =>
            {
                wh.Set();
            };

            wh.Wait();

            listener.Stop();
            thread.Dispose();
        }
    }
}
