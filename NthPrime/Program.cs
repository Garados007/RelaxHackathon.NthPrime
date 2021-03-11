using System;
using System.IO;
using System.Threading.Tasks;

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
