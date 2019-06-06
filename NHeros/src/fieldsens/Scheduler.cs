using System.Collections.Generic;
using System.Threading;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2015 Johannes Lerch.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.fieldsens
{
	using Lists = com.google.common.collect.Lists;

	public class Scheduler
	{

		private LinkedList<ThreadStart> worklist = new List();

		public virtual void schedule(ThreadStart job)
		{
			worklist.AddLast(job);
		}

		public virtual void runAndAwaitCompletion()
		{
			while (worklist.Count > 0)
			{
				worklist.RemoveLast().run();
			}
		}

	}

}