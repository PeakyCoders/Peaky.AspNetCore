using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Linq;

namespace Peaky.Extensions.Web.Debug
{
    internal class LoggingMiddleware : IMiddleware
    {
        private const int MaxBodyLength = 1000;

        private static readonly object ConsoleLock = new object();

        private readonly string[] _highlightedTraces;

        public LoggingMiddleware(string[] highlightedTraces)
        {
            _highlightedTraces = highlightedTraces ?? Array.Empty<string>();
        }

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                await next(ctx);
            }
            finally
            {
                watch.Stop();

                if (ShouldTracePath(ctx.Request.Path.Value))
                {
                    string requestBody = await GetRequestBody(ctx.Request);

                    lock (ConsoleLock)
                    {
                        Print(ctx, watch);
                        PrintBody(ctx, requestBody);
                        PrintException(ctx);

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }
        }

        private bool ShouldTracePath(string path)
        {
            return !path.StartsWith("/js") &&
                   !path.StartsWith("/css") &&
                   !path.StartsWith("/fonts");
        }

        private async Task<string> GetRequestBody(HttpRequest request)
        {
            // Request body stream rewind must be enabled in order to re-read it in the middleware
            request.EnableRewind();
            request.Body.Seek(0, SeekOrigin.Begin);

            using (StreamReader reader = new StreamReader(request.Body))
            {
                string requestBody = await reader.ReadToEndAsync();

                request.Body.Seek(0, SeekOrigin.Begin);

                return requestBody;
            }
            
        }

        private void Print(HttpContext ctx, Stopwatch watch)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(DateTime.Now.ToString("[HH:mm:ss]") + " ");

            Console.BackgroundColor = GetVerbColor(ctx.Request.Method);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(" " + ctx.Request.Method.PadRight(7));

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" " + ctx.Request.Path.Value.PadRight(50) + " ");

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = GetStatusColor(ctx.Response.StatusCode);
            Console.Write($"{ctx.Response.StatusCode} {((HttpStatusCode)ctx.Response.StatusCode).ToString("G").PadRight(20)} ");

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(watch.ElapsedMilliseconds.ToString().PadLeft(6) + " ms\n");
        }

        private void PrintBody(HttpContext ctx, string requestBody)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGray;

            if (ctx.Request.ContentType != null && !ctx.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"{ctx.Request.ContentType} body ({ctx.Request.ContentLength} bytes)");
            }
            else if (requestBody.Length > MaxBodyLength)
            {
                Console.WriteLine(requestBody.Substring(0, MaxBodyLength) + " ...");
            }
            else if (requestBody.Length != 0)
            {
                Console.WriteLine(requestBody);
            }
        }

        private void PrintException(HttpContext ctx)
        {
            Exception ex = ctx.Items[Constants.HttpContextDebugItem] as Exception;

            // If any exception was thrown, it will be in the Items bag thanks to DebugExceptionFilter
            if (ex != null)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(ex.GetType().FullName + ":");

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" " + ex.Message);

                Console.BackgroundColor = ConsoleColor.Black;

                foreach (string line in ex.StackTrace.Split('\n'))
                {
                    if (_highlightedTraces.Any(h => line.Contains(h, StringComparison.Ordinal)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }

                    int indexOfFile = line.IndexOf(" in ", StringComparison.Ordinal);

                    if (indexOfFile == -1)
                    {
                        Console.WriteLine(line);
                    }
                    else
                    {
                        Console.WriteLine(line.Substring(0, indexOfFile + 1));

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\t" + line.Substring(indexOfFile + 1));
                    }
                }
            }
        }

        private ConsoleColor GetVerbColor(string method)
        {
            switch (method)
            {
                case "GET":
                    return ConsoleColor.Green;

                case "POST":
                    return ConsoleColor.Cyan;

                case "PUT":
                    return ConsoleColor.Magenta;

                case "DELETE":
                    return ConsoleColor.Red;

                default:
                    return ConsoleColor.Yellow;
            }
        }

        private ConsoleColor GetStatusColor(int status)
        {
            return status >= 500 ? ConsoleColor.Red
                : status >= 400 ? ConsoleColor.Yellow
                : status >= 300 ? ConsoleColor.Magenta
                : status >= 200 ? ConsoleColor.Green
                : ConsoleColor.Cyan;
        }
    }
}
