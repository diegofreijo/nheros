using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using heros;
using heros.solver;

namespace NHeros.src.solver
{
    public class DummyExecutor
    {
        private int v1;
        private int numThreads;
        private int v2;
        private Queue<ThreadStart> queue;

        public bool Terminating { get; internal set; }
        public Exception Exception { get; internal set; }

        public DummyExecutor(int v1, int numThreads, int v2, Queue<ThreadStart> queue)
        {
            this.v1 = v1;
            this.numThreads = numThreads;
            this.v2 = v2;
            this.queue = queue;
        }

        internal void shutdown()
        {
            throw new NotImplementedException();
        }

        internal void awaitCompletion()
        {
            throw new NotImplementedException();
        }

        internal void execute(ITask task)
        {
            throw new NotImplementedException();
        }
    }
}
