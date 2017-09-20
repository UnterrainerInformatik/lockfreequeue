// *************************************************************************** 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LockFreeQueue.Tests
{
    [TestFixture]
    class PerformanceComparisonVsConcurrentQueue
    {
        private const int NUMBER_OF_OPERATIONS = 10000000;
        private const int NUMBER_OF_THREADS = 4;

        private ManualResetEvent addLqDone = new ManualResetEvent(false);
        private ManualResetEvent addCqDone = new ManualResetEvent(false);
        private ManualResetEvent remLqDone = new ManualResetEvent(false);
        private ManualResetEvent remCqDone = new ManualResetEvent(false);

        private LockFreeQueue<int> lq = new LockFreeQueue<int>();
        private ConcurrentQueue<int> cq = new ConcurrentQueue<int>();
        private LockFreeQueue<int> lqDestination = new LockFreeQueue<int>();
        private ConcurrentQueue<int> cqDestination = new ConcurrentQueue<int>();

        private DateTime start;
        private bool lqConsoleOut;
        private bool cqConsoleOut;

        private readonly Random rand = new Random(1);

        [Test]
        public void LockFreeQueueVsConcurrentQueue()
        {
            lq = new LockFreeQueue<int>();
            cq = new ConcurrentQueue<int>();
            lqDestination = new LockFreeQueue<int>();
            cqDestination = new ConcurrentQueue<int>();

            addLqDone = new ManualResetEvent(false);
            addCqDone = new ManualResetEvent(false);
            remLqDone = new ManualResetEvent(false);
            remCqDone = new ManualResetEvent(false);

            // Create a thread for each adding-process.
            BackgroundWorker adderLq = new BackgroundWorker();
            adderLq.DoWork += addLq_DoWork;
            adderLq.RunWorkerCompleted += addLq_Completed;

            BackgroundWorker adderCq = new BackgroundWorker();
            adderCq.DoWork += addCq_DoWork;
            adderCq.RunWorkerCompleted += addCq_Completed;

            // Create a thread for each removal-process.
            BackgroundWorker removerLq = new BackgroundWorker();
            removerLq.DoWork += removeLq_DoWork;
            removerLq.RunWorkerCompleted += removeLq_Completed;

            BackgroundWorker removerCq = new BackgroundWorker();
            removerCq.DoWork += removeCq_DoWork;
            removerCq.RunWorkerCompleted += removeCq_Completed;

            // Start the whole shebang.
            start = DateTime.Now;
            //adderLq.RunWorkerAsync();
            adderCq.RunWorkerAsync();
            //addLqDone.WaitOne();
            addCqDone.WaitOne();
            //removerLq.RunWorkerAsync();
            removerCq.RunWorkerAsync();

            // Wait for all tests to finish.
            //addLqDone.WaitOne();
            addCqDone.WaitOne();
            //remLqDone.WaitOne();
            remCqDone.WaitOne();

            Assert.AreEqual(0, lq.Count);
            Assert.AreEqual(NUMBER_OF_OPERATIONS, lqDestination.Count);

            Assert.IsEmpty(cq);
            Assert.AreEqual(NUMBER_OF_OPERATIONS, cqDestination.Count);
        }

        private void addLq_DoWork(object sender, DoWorkEventArgs e)
        {
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = NUMBER_OF_THREADS;
            Parallel.For(0, NUMBER_OF_OPERATIONS - 900000, po, addLq_Action);
        }

        private void addCq_DoWork(object sender, DoWorkEventArgs e)
        {
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = NUMBER_OF_THREADS;
            Parallel.For(0, NUMBER_OF_OPERATIONS, po, addCq_Action);
        }

        private void removeLq_DoWork(object sender, DoWorkEventArgs e)
        {
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = NUMBER_OF_THREADS;

            while (!addLqDone.WaitOne(0) || lq.Count > 0)
            {
                Parallel.For(0, po.MaxDegreeOfParallelism, po, removeLq_Action);
                if (lq.Count == 0 && !lqConsoleOut)
                {
                    TimeSpan s = DateTime.Now.Subtract(start);
                    Console.Out.WriteLine(
                        $"LockFreeQueue finished dequeuing in {s.TotalMilliseconds}ms");
                    lqConsoleOut = true;
                }
            }
        }

        private void removeCq_DoWork(object sender, DoWorkEventArgs e)
        {
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = NUMBER_OF_THREADS;

            while (!addCqDone.WaitOne(0) || !cq.IsEmpty)
            {
                Parallel.For(0, po.MaxDegreeOfParallelism, po, removeCq_Action);
                if (cq.IsEmpty && !cqConsoleOut)
                {
                    TimeSpan s = DateTime.Now.Subtract(start);
                    Console.Out.WriteLine(
                        $"ConcurrentQueue finished dequeuing in {s.TotalMilliseconds}ms");
                    cqConsoleOut = true;
                }
            }
        }

        private void addLq_Action(int item, ParallelLoopState ls)
        {
            lq.Enqueue(rand.Next());
        }

        private void addCq_Action(int item, ParallelLoopState ls)
        {
            cq.Enqueue(rand.Next());
        }

        private void removeLq_Action(int item, ParallelLoopState ls)
        {
            int r;
            lq.TryDequeue(out r);
            lqDestination.Enqueue(r);
        }

        private void removeCq_Action(int item, ParallelLoopState ls)
        {
            int r;
            cq.TryDequeue(out r);
            cqDestination.Enqueue(r);
        }

        private void addLq_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            addLqDone.Set();
        }

        private void addCq_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            addCqDone.Set();
        }

        private void removeLq_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            remLqDone.Set();
        }

        private void removeCq_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            remCqDone.Set();
        }
    }
}