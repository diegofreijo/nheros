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

	/// <summary>
	/// A synchronization aid similar to <seealso cref="System.Threading.CountdownEvent"/> but with the ability
	/// to also count up. This is useful to wait until a variable number of tasks
	/// have completed. <seealso cref="awaitZero()"/> will block until the count reaches zero.
	/// </summary>
	public class CountLatch
	{


//ORIGINAL LINE: @SuppressWarnings("serial") private static final class Sync extends java.util.concurrent.locks.AbstractQueuedSynchronizer
		private sealed class Sync : AbstractQueuedSynchronizer
		{

			internal Sync(int count)
			{
				State = count;
			}

			internal int Count
			{
				get
				{
					return State;
				}
			}

			internal void reset()
			{
				State = 0;
			}

			protected internal override int tryAcquireShared(int acquires)
			{
				return (State == 0) ? 1 : -1;
			}

			protected internal int acquireNonBlocking(int acquires)
			{
				// increment count
				for (;;)
				{
					int c = State;
					int nextc = c + 1;
					if (compareAndSetState(c, nextc))
					{
						return 1;
					}
				}
			}

			protected internal override bool tryReleaseShared(int releases)
			{
				// Decrement count; signal when transition to zero
				for (;;)
				{
					int c = State;
					if (c == 0)
					{
						return false;
					}
					int nextc = c - 1;
					if (compareAndSetState(c, nextc))
					{
						return nextc == 0;
					}
				}
			}
		}

		private readonly Sync sync;

		public CountLatch(int count)
		{
			this.sync = new Sync(count);
		}


//ORIGINAL LINE: public void awaitZero() throws InterruptedException
		public virtual void awaitZero()
		{
			sync.acquireShared(1);
		}


//ORIGINAL LINE: public boolean awaitZero(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException
		public virtual bool awaitZero(long timeout, TimeUnit unit)
		{
			return sync.tryAcquireSharedNanos(1, unit.toNanos(timeout));
		}

		public virtual void increment()
		{
			sync.acquireNonBlocking(1);
		}

		public virtual void decrement()
		{
			sync.releaseShared(1);
		}

		/// <summary>
		/// Resets the counter to zero. But waiting threads won't be released somehow.
		/// So this interrupts the threads so that they escape from their waiting state.
		/// </summary>
		public virtual void resetAndInterrupt()
		{
			sync.reset();
			for (int i = 0; i < 3; i++) //Because it is a best effort thing, do it three times and hope for the best.
			{
				foreach (Thread t in sync.QueuedThreads)
				{
					t.Interrupt();
				}
			}
			sync.reset(); //Just in case a thread would've incremented the counter again.
		}

		public override string ToString()
		{
			return base.ToString() + "[Count = " + sync.Count + "]";
		}

		/// <summary>
		/// Gets whether this counting latch has arrived at zero </summary>
		/// <returns> True if this counting latch has arrived at zero, otherwise
		/// false </returns>
		public virtual bool AtZero
		{
			get
			{
				return sync.Count == 0;
			}
		}

	}

}