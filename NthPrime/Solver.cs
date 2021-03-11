using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NthPrime
{
    public class Solver
    {
        readonly Memory<uint> fullSieve;
        readonly ReadOnlyMemory<uint> bitMask = GenerateBitMasks();
        readonly int maxTasks = Environment.ProcessorCount;
        readonly int minJobs = 1_000;

        // https://math.stackexchange.com/a/1259
        public static ulong UpperBound(ulong number)
        {
            if (number < 6)
                return 13;
            var ln_n = Math.Log(number);
            return (ulong)Math.Floor(number * (ln_n + Math.Log(ln_n)));
        }

        private static ReadOnlyMemory<uint> GenerateBitMasks()
        {
            Memory<uint> masks = new uint[32];
            for (int i = 0; i < 32; ++i)
                masks.Span[i] = 1u << i;
            return masks;
        }

        public Solver(int maxSize)
        {
            fullSieve = new uint[maxSize];
        }

        public static async Task<ulong?> GetNthPrimeAsync(ulong n)
        {
            if (n == 0)
                return null;
            ulong index = 1;
            var upperBound = UpperBound(n);
            if (upperBound >= int.MaxValue)
                return null;
            var solver = new Solver((int)upperBound);
            await foreach (var prime in solver.GetPrimesAsync().ConfigureAwait(false))
            {
                if (index++ == n)
                    return prime;
            }
            return null;
        }

        public async IAsyncEnumerable<ulong> GetPrimesAsync()
        {
            int startIndex = 0;
            ulong lastHighestPrime = 0;
            // get the index of sqrt of the maximum possible number inside fullSieve
            var upperSieveBound = (int)((ulong)Math.Sqrt((ulong)fullSieve.Length << 5) >> 5);
            while (startIndex <= upperSieveBound)
            {
                const int maxPrimes = 100;

                // get next possible primes. (some are multiples of others in this list)
                var possiblePrimes = NextPossiblePrimes(ref startIndex, maxPrimes);
                if (possiblePrimes.Length == 0)
                    // no more primes found
                    yield break;
                // validate primes
                possiblePrimes = FilterPrimes(possiblePrimes.Span);
                if (possiblePrimes.Length == 0)
                    continue;
                // return current primes
                for (int i = 0; i < possiblePrimes.Length; ++i)
                {
                    var prime = possiblePrimes.Span[i];
                    if (prime > lastHighestPrime)
                        yield return prime;
                }
                // fix start index. We only need to sieve after x² of the smallest prime
                startIndex = (int)(possiblePrimes.Span[0] * possiblePrimes.Span[0] >> 5);
                // sieve next primes
                await SieveAllAsync(startIndex, possiblePrimes).ConfigureAwait(false);
            }
            // the rest of the numbers are primes. Just return them
            foreach (var prime in EnumeratePrimes(startIndex))
                yield return prime;
        }

        private static ReadOnlyMemory<ulong> FilterPrimes(ReadOnlySpan<ulong> possiblePrimes)
        {
            Memory<ulong> buffer = new ulong[possiblePrimes.Length];
            int filled = 0;
            for (int i = 0; i < possiblePrimes.Length; ++i)
            {
                // check if prime is 1
                if (possiblePrimes[i] <= 1)
                    continue;
                // check if this value is a multiple of another prime
                for (int j = 0; j < filled; ++j)
                    if ((possiblePrimes[i] % buffer.Span[j]) == 0)
                        // invalid prime found
                        goto invalid_prime;
                
                // add valid prime
                buffer.Span[filled++] = possiblePrimes[i];
                // continue with next
                invalid_prime:;
            }
            return buffer[..filled];
        }

        private IEnumerable<ulong> EnumeratePrimes(int startIndex)
        {
            for (; startIndex < fullSieve.Length; ++startIndex)
            {
                // check if all bits are set. If so there is no prime in this range
                var mask = fullSieve.Span[startIndex];
                if (mask == uint.MaxValue)
                    continue;
                // check each bit if it is not set
                for (int j = 0; j < 32; ++j)
                {
                    if ((mask & bitMask.Span[j]) != 0)
                        continue;
                    // a prime is found
                    yield return ((ulong)startIndex << 5) + (ulong)j;
                }
            }
        }

        private ReadOnlyMemory<ulong> NextPossiblePrimes(ref int startIndex, int maxPrimes)
        {
            Memory<ulong> buffer = new ulong[maxPrimes];
            int filled = 0;
            var bitMask = this.bitMask.Span;
            for (; startIndex < fullSieve.Length; ++startIndex)
            {
                // check if all bits are set. If so there is no prime in this range
                var mask = fullSieve.Span[startIndex];
                if (mask == uint.MaxValue)
                    continue;
                // check each bit if it is not set
                for (int j = 0; j < 32; ++j)
                {
                    if ((mask & bitMask[j]) != 0)
                        continue;
                    // a possible prime is found, now add it
                    buffer.Span[filled++] = ((ulong)startIndex << 5) + (ulong)j;
                    if (filled >= maxPrimes)
                        return buffer;
                }
            }
            return buffer[..filled];
        }

        /// <summary>
        /// Sieve all numbers in <see cref="fullSieve"/> with all the given primes in 
        /// <paramref name="numbers"/>.
        /// </summary>
        /// <param name="startIndex">Start index in <see cref="fullSieve"/> to start sieve</param>
        /// <param name="numbers">the prime numbers to sieve in this iteration</param>
        /// <returns>execution task</returns>
        private async Task SieveAllAsync(int startIndex, ReadOnlyMemory<ulong> numbers)
        {
            var jobs = (fullSieve.Length - startIndex) / maxTasks;
            if (jobs < minJobs)
            {
                jobs = minJobs;
            }
            Memory<Task> tasks = new Task[maxTasks];
            int i = 0;
            for (; i < maxTasks && startIndex < fullSieve.Length; ++i)
            {
                var endIndex = i == maxTasks - 1 
                    ? fullSieve.Length 
                    : Math.Min(fullSieve.Length, startIndex + jobs);
                tasks.Span[i] = SieveAsync(startIndex, endIndex, numbers);
                startIndex += jobs;
            }
            tasks = tasks[..i];
            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        private Task SieveAsync(int startIndex, int endIndex, ReadOnlyMemory<ulong> numbers)
        {
            return Task.Run(() => Sieve(startIndex, endIndex, numbers));
        }

        private void Sieve(int startIndex, int endIndex, ReadOnlyMemory<ulong> numbers)
        {
            for (int i = 0; i < numbers.Length; ++i)
            {
                Sieve(startIndex, endIndex, numbers.Span[i]);
            }
        }

        /// <summary>
        /// sieve with a specific number
        /// </summary>
        /// <param name="startIndex">The start index inside the sieve</param>
        /// <param name="endIndex">The end index inside the sieve</param>
        /// <param name="number">The number to sieve.</param>
        private void Sieve(int startIndex, int endIndex, ulong number)
        {
            // get the lowest multiple in range
            var lowest = (ulong)Math.Floor((ulong)startIndex * 32 / (double)number);
            // get the highest multiple in range
            var highest = (ulong)Math.Floor((ulong)endIndex * 32 / (double)number);
            // loop all multiples in range
            for (ulong i = lowest; i < highest; ++i)
            {
                var num = i * number;
                fullSieve.Span[(int)(num >> 5)] |= bitMask.Span[(int)(num & 31)];
            }
        }
    }
}