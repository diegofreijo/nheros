using System;
using System.Threading;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2012 Eric Bodden.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Eric Bodden - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.solver
{
	using SootThreadGroup = heros.util.SootThreadGroup;


	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// A <seealso cref="ThreadPoolExecutor"/> which keeps track of the number of spawned
	/// tasks to allow clients to await their completion. 
	/// </summary>
	public class CountingThreadPoolExecutor : ThreadPoolExecutor
	{

		protected internal static readonly Logger logger = LoggerFactory.getLogger(typeof(CountingThreadPoolExecutor));

		protected internal readonly CountLatch numRunningTasks = new CountLatch(0);

		protected internal volatile Exception exception = null;

		public CountingThreadPoolExecutor(int corePoolSize, int maximumPoolSize, long keepAliveTime, TimeUnit unit, BlockingQueue<ThreadStart> workQueue) : base(corePoolSize, maximumPoolSize, keepAliveTime, unit, workQueue, new ThreadFactoryAnonymousInnerClass())
		{
		}

		private class ThreadFactoryAnonymousInnerClass : ThreadFactory
		{

			public override Thread newThread(ThreadStart r)
			{
				return new Thread(new SootThreadGroup(), r);
			}
		}

		public override void execute(ThreadStart command)
		{
			try
			{
				numRunningTasks.increment();
				base.execute(command);
			}
			catch (RejectedExecutionException ex)
			{
				// If we were unable to submit the task, we may not count it!
				numRunningTasks.decrement();
				throw ex;
			}
		}

		protected internal override void afterExecute(ThreadStart r, Exception t)
		{
			if (t != null)
			{
				exception = t;
				logger.error("Worker thread execution failed: " + t.Message, t);

				shutdownNow();
				numRunningTasks.resetAndInterrupt();
			}
			else
			{
				numRunningTasks.decrement();
			}
			base.afterExecute(r, t);
		}

		/// <summary>
		/// Awaits the completion of all spawned tasks.
		/// </summary>

//ORIGINAL LINE: public void awaitCompletion() throws InterruptedException
		public virtual void awaitCompletion()
		{
			numRunningTasks.awaitZero();
		}

		/// <summary>
		/// Awaits the completion of all spawned tasks.
		/// </summary>

//ORIGINAL LINE: public void awaitCompletion(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException
		public virtual void awaitCompletion(long timeout, TimeUnit unit)
		{
			numRunningTasks.awaitZero(timeout, unit);
		}

		/// <summary>
		/// Returns the exception thrown during task execution (if any).
		/// </summary>
		public virtual Exception Exception
		{
			get
			{
				return exception;
			}
		}

	}
}