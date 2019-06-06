using System.Threading;

namespace heros.util
{
	public class SootThreadGroup : ThreadGroup
	{

		private readonly Thread startThread;

		public SootThreadGroup() : base("Soot Threadgroup")
		{
			if (Thread.CurrentThread.ThreadGroup is SootThreadGroup)
			{
				SootThreadGroup group = (SootThreadGroup) Thread.CurrentThread.ThreadGroup;
				startThread = group.StarterThread;
			}
			else
			{
				startThread = Thread.CurrentThread;
			}
		}

		public virtual Thread StarterThread
		{
			get
			{
				return startThread;
			}
		}
	}

}