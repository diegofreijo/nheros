using NHeros.src.util;
using System.Runtime.Caching;
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
namespace heros
{
	public class FlowFunctionCache<N, D, M> : FlowFunctions<N, D, M>
	{
		protected internal readonly FlowFunctions<N, D, M> @delegate;

		private readonly LoadingCache<NNKey, FlowFunction<D>> normalCache;
        private readonly LoadingCache<CallKey, FlowFunction<D>> callCache;
        private readonly LoadingCache<ReturnKey, FlowFunction<D>> returnCache;
        private readonly LoadingCache<NNKey, FlowFunction<D>> callToReturnCache;

		//private readonly Logger logger = LoggerFactory.getLogger(this.GetType());

		public FlowFunctionCache(FlowFunctions<N, D, M> @delegate)
		{
			this.@delegate = @delegate;

			normalCache = new LoadingCache<NNKey, FlowFunction<D>>(
                (NNKey key) => @delegate.getNormalFlowFunction(key.Curr, key.Succ)
            );

			callCache = new LoadingCache<CallKey, FlowFunction<D>>(
                (CallKey key) => @delegate.getCallFlowFunction(key.CallStmt, key.DestinationMethod)
            );

			returnCache = new LoadingCache<ReturnKey, FlowFunction<D>>(
                (ReturnKey key) => @delegate.getReturnFlowFunction(key.CallStmt, key.DestinationMethod, key.ExitStmt, key.ReturnSite)
            );

			callToReturnCache = new LoadingCache<NNKey, FlowFunction<D>>(
                (NNKey key) => @delegate.getCallToReturnFlowFunction(key.Curr, key.Succ)
            );
		}

		public virtual FlowFunction<D> getNormalFlowFunction(N curr, N succ)
		{
			return normalCache.Get(new NNKey(this, curr, succ));
		}

		public virtual FlowFunction<D> getCallFlowFunction(N callStmt, M destinationMethod)
		{
			return callCache.Get(new CallKey(this, callStmt, destinationMethod));
		}

		public virtual FlowFunction<D> getReturnFlowFunction(N callSite, M calleeMethod, N exitStmt, N returnSite)
		{
			return returnCache.Get(new ReturnKey(this, callSite, calleeMethod, exitStmt, returnSite));
		}

		public virtual FlowFunction<D> getCallToReturnFlowFunction(N callSite, N returnSite)
		{
			return callToReturnCache.Get(new NNKey(this, callSite, returnSite));
		}

		private class NNKey
		{
            private readonly FlowFunctionCache<N, D, M> outerInstance;

			internal readonly N curr, succ;

			internal NNKey(FlowFunctionCache<N, D, M> outerInstance, N curr, N succ)
			{
				this.outerInstance = outerInstance;
				this.curr = curr;
				this.succ = succ;
			}

			public virtual N Curr
			{
				get
				{
					return curr;
				}
			}

			public virtual N Succ
			{
				get
				{
					return succ;
				}
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + (Utils.IsDefault(curr) ? 0 : curr.GetHashCode());
				result = prime * result + (Utils.IsDefault(succ) ? 0 : succ.GetHashCode());
				return result;
			}

			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (obj == null)
				{
					return false;
				}
				if (this.GetType() != obj.GetType())
				{
					return false;
				}

				NNKey other = (NNKey) obj;
				if (Utils.IsDefault(curr))
				{
					if (Utils.IsDefault(other.curr))
					{
						return false;
					}
				}
				else if (!curr.Equals(other.curr))
				{
					return false;
				}
				if (Utils.IsDefault(succ))
				{
					if (Utils.IsDefault(other.succ))
					{
						return false;
					}
				}
				else if (!succ.Equals(other.succ))
				{
					return false;
				}
				return true;
			}
		}

		private class CallKey
		{
			private readonly FlowFunctionCache<N, D, M> outerInstance;

			internal readonly N callStmt;
			internal readonly M destinationMethod;

			internal CallKey(FlowFunctionCache<N, D, M> outerInstance, N callStmt, M destinationMethod)
			{
				this.outerInstance = outerInstance;
				this.callStmt = callStmt;
				this.destinationMethod = destinationMethod;
			}

			public virtual N CallStmt
			{
				get
				{
					return callStmt;
				}
			}

			public virtual M DestinationMethod
			{
				get
				{
					return destinationMethod;
				}
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + (Utils.IsDefault(callStmt) ? 0 : callStmt.GetHashCode());
				result = prime * result + (Utils.IsDefault(destinationMethod) ? 0 : destinationMethod.GetHashCode());
				return result;
			}

			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (obj == null)
				{
					return false;
				}
				if (this.GetType() != obj.GetType())
				{
					return false;
				}
				CallKey other = (CallKey) obj;
				if (Utils.IsDefault(callStmt))
				{
					if (Utils.IsDefault(other.callStmt))
					{
						return false;
					}
				}
				else if (!callStmt.Equals(other.callStmt))
				{
					return false;
				}
				if (Utils.IsDefault(destinationMethod))
				{
					if (Utils.IsDefault(other.destinationMethod))
					{
						return false;
					}
				}
				else if (!destinationMethod.Equals(other.destinationMethod))
				{
					return false;
				}
				return true;
			}
		}

		private class ReturnKey : CallKey
		{
			private readonly FlowFunctionCache<N, D, M> outerInstance;


			internal readonly N exitStmt, returnSite;

			internal ReturnKey(FlowFunctionCache<N, D, M> outerInstance, N callStmt, M destinationMethod, N exitStmt, N returnSite) : base(outerInstance, callStmt, destinationMethod)
			{
				this.outerInstance = outerInstance;
				this.exitStmt = exitStmt;
				this.returnSite = returnSite;
			}

			public virtual N ExitStmt
			{
				get
				{
					return exitStmt;
				}
			}

			public virtual N ReturnSite
			{
				get
				{
					return returnSite;
				}
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = base.GetHashCode();
				result = prime * result + (Utils.IsDefault(exitStmt) ? 0 : exitStmt.GetHashCode());
				result = prime * result + (Utils.IsDefault(returnSite) ? 0 : returnSite.GetHashCode());
				return result;
			}

			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (!base.Equals(obj))
				{
					return false;
				}
				if (this.GetType() != obj.GetType())
				{
					return false;
				}

//ORIGINAL LINE: @SuppressWarnings("unchecked") ReturnKey other = (ReturnKey) obj;
				ReturnKey other = (ReturnKey) obj;
				if (Utils.IsDefault(exitStmt))
				{
					if (Utils.IsDefault(other.exitStmt))
					{
						return false;
					}
				}
				else if (!exitStmt.Equals(other.exitStmt))
				{
					return false;
				}
				if (Utils.IsDefault(returnSite))
				{
					if (Utils.IsDefault(other.returnSite))
					{
						return false;
					}
				}
				else if (!returnSite.Equals(other.returnSite))
				{
					return false;
				}
				return true;
			}
		}

		//public virtual void printStats()
		//{
		//	logger.debug("Stats for flow-function cache:\n" + "Normal:         {}\n" + "Call:           {}\n" + "Return:         {}\n" + "Call-to-return: {}\n", 
  //              normalCache.stats(),
  //              callCache.stats(), 
  //              returnCache.stats(), 
  //              callToReturnCache.stats()
  //          );
		//}

		//public virtual void invalidate()
		//{
		//	callCache.invalidateAll();
		//	callToReturnCache.invalidateAll();
		//	normalCache.invalidateAll();
		//	returnCache.invalidateAll();
		//}
	}

}