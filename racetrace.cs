using System;
using System.Threading;

class Program
{
    const int NumThreads = 4;
    const int IncrementsPerThread = 250_000;

    static int counter = 0;
    static readonly object gate = new object();
    static bool useLock = false;

    static int[][] seenLog = new int[NumThreads][];

    static void Worker(int id)
    {
        int[] log = seenLog[id];

        for (int i = 0; i < IncrementsPerThread; i++)
        {
            if (useLock)
            {
                lock (gate)
                {
                    int seen = counter;
                    log[i] = seen;
                    counter = seen + 1;
                }
            }
            else
            {
                int seen = counter;
                log[i] = seen;
                counter = seen + 1;
            }
        }
    }

    static void Main(string[] args)
    {
        useLock = args.Length > 0 && args[0] == "--lock";

        int total = NumThreads * IncrementsPerThread;

        Thread[] threads = new Thread[NumThreads];

        for (int t = 0; t < NumThreads; t++)
        {
            seenLog[t] = new int[IncrementsPerThread];

            int id = t;

            threads[t] = new Thread(() => Worker(id));
            threads[t].Start();
        }

        foreach (Thread th in threads)
        {
            th.Join();
        }

        // Count how many times each value was read
        int[] readCount = new int[total + 1];

        for (int t = 0; t < NumThreads; t++)
        {
            for (int i = 0; i < IncrementsPerThread; i++)
            {
                readCount[seenLog[t][i]]++;
            }
        }

        // Count collisions
        int collisions = 0;

        for (int v = 0; v <= total; v++)
        {
            if (readCount[v] > 1)
            {
                collisions += readCount[v] - 1;
            }
        }

        string mode = useLock ? "with lock" : "no synchronization";

        Console.WriteLine($"Mode: {mode}");
        Console.WriteLine($"Total increments: {total}");
        Console.WriteLine($"Final counter: {counter}");
        Console.WriteLine($"Lost updates: {total - counter}");
        Console.WriteLine($"Collisions: {collisions}");

        // Print first 5 collisions
        int shown = 0;

        for (int v = 0; v <= total && shown < 5; v++)
        {
            if (readCount[v] > 1)
            {
                Console.WriteLine($"\nCollision value: {v}");

                for (int t = 0; t < NumThreads; t++)
                {
                    for (int i = 0; i < IncrementsPerThread; i++)
                    {
                        if (seenLog[t][i] == v)
                        {
                            Console.WriteLine(
                                $"Thread {t}, loop index {i}");
                        }
                    }
                }

                shown++;
            }
        }
    }
}