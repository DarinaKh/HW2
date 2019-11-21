using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HW2
{
    public class CustomTaskFactory
    {
        public static object lo = new object();
        private readonly int _capacity;
        private int runingTasks = 0;
        private TaskFactory taskFactory;

        public CustomTaskFactory(int capacity)
        {
            _capacity = capacity;
            taskFactory = new TaskFactory();
        }

        public void Start(int param, Action<int> action)
        {
            var task = taskFactory.StartNew(s => action((int)s), param);

            task.ContinueWith(a =>
            {
                lock (lo)
                    runingTasks--;
            });

            lock (lo)
                runingTasks++;

            if (param % 100_000 == 0)
                Console.WriteLine($"processing: {param} active tasks: {runingTasks}");

            while (runingTasks >= _capacity)
                Thread.Sleep(1);
        }
    }

    class Program
    {
        static HashSet<int> hashSet = new HashSet<int>();
        static void Main(string[] args)
        {
            var fct = new CustomTaskFactory(1000);
            var st = new Stopwatch();
            st.Start();

            for (int i = 3; i <= 10_000_000; i++)
            {
                fct.Start(i, isPrime);
            }

            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);
            Console.ReadLine();
        }
        static void isPrime(int numberForCheck)
        {
            if (numberForCheck % 2 == 0) return;

            var boundary = (int)Math.Floor(Math.Sqrt(numberForCheck));

            for (int i = 3; i <= boundary; i += 2)
                if (numberForCheck % i == 0)
                    return;

            lock (hashSet)
            {
                hashSet.Add(numberForCheck);
            }
        }
    }
}
