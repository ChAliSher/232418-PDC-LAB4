using System;
using System.Threading;

class Program {
    const int NumThreads = 4;
    const int IncrementPerThread = 1_000_000;

    static long counter = 0;

    static void Worker() {
        for (int i = 0; i < IncrementPerThread; i++){
            counter++;
        }
    }

    static void Main(){
        Thread[] threads = new Thread[NumThreads];
        for(int i = 0; i < NumThreads; i++){
            threads[i] = new Thread(Worker);
            threads[i].Start();
        }
        for(int i = 0; i < NumThreads; i++){
            threads[i].Join();
        }

        long expected = (long)NumThreads * IncrementPerThread;
        Console.WriteLine($"Expected: {expected}");
        Console.WriteLine($"Actual: {counter}");
        Console.WriteLine($"Lost Updates: {expected - counter}");
        Console.WriteLine($"Logical Processors: {Environment.ProcessorCount}");
    }
}