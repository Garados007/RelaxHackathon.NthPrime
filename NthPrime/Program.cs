using System;
using System.IO;
using System.Threading.Tasks;
using MaxLib.WebServer;

namespace NthPrime
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                var index = File.Exists(args[0])
                    ? await GetIndex(args[0])
                    : ParseIndex(args[0]);
                if (index == null)
                {
                    Console.WriteLine("invalid number in file or argument");
                    return;
                }

                var result = await Solver.GetNthPrimeAsync(index.Value).ConfigureAwait(false);
                Console.WriteLine(result);
            }
            else
            {
                // init server
                var server = new Server(new WebServerSettings(8001, 5000));
                server.InitialDefault(); // init with default service collection
                // add our own services
                server.AddWebService(new PrimeService());
                server.AddWebService(new Swagger());
                // start
                server.Start();

                Console.WriteLine("Server is running");

                // wait for exit
                await Task.Delay(-1);
            }
        }

        static async Task<ulong?> GetIndex(string file)
        {
            var lines = await File.ReadAllLinesAsync(file);
            if (lines.Length == 0)
                return null;
            if (ulong.TryParse(lines[0], out ulong index))
                return index;
            return null;
        }

        static ulong? ParseIndex(string value)
        {
            return ulong.TryParse(value, out ulong index) ? index : null;
        }
    }
}
