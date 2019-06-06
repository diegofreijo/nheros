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
	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class FlowFunctionCache<N, D, M> : FlowFunctions<N, D, M>
	{

		protected internal readonly FlowFunctions<N, D, M> @delegate;

		protected internal readonly LoadingCache<NNKey, FlowFunction<D>> normalCache;

		protected internal readonly LoadingCache<CallKey, FlowFunction<D>> callCache;

		protected internal readonly LoadingCache<ReturnKey, FlowFunction<D>> returnCache;

		protected internal readonly LoadingCache<NNKey, FlowFunction<D>> callToReturnCache;

		private readonly Logger logger = LoggerFactory.getLogger(this.GetType());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public FlowFunctionCache(final FlowFunctions<N, D, M> delegate, @SuppressWarnings("rawtypes") com.google.common.cache.CacheBuilder builder)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public FlowFunctionCache(FlowFunctions<N, D, M> @delegate, CacheBuilder builder)
		{
			this.@delegate = @delegate;

			normalCache = builder.build(new CacheLoaderAnonymousInnerClass(this, @delegate));

			callCache = builder.build(new CacheLoaderAnonymousInnerClass2(this, @delegate));

			returnCache = builder.build(new CacheLoaderAnonymousInnerClass3(this, @delegate));

			callToReturnCache = builder.build(new CacheLoaderAnonymousInnerClass4(this, @delegate));
		}

		private class CacheLoaderAnonymousInnerClass : CacheLoader<NNKey, FlowFunction<D>>
		{
			private readonly FlowFunctionCache<N, D, M> outerInstance;

			private heros.FlowFunctions<N, D, M> @delegate;

			public CacheLoaderAnonymousInnerClass(FlowFunctionCache<N, D, M> outerInstance, heros.FlowFunctions<N, D, M> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FlowFunction<D> load(NNKey key) throws Exception
			public FlowFunction<D> load(NNKey key)
			{
				return @delegate.getNormalFlowFunction(key.Curr, key.Succ);
			}
		}

		private class CacheLoaderAnonymousInnerClass2 : CacheLoader<CallKey, FlowFunction<D>>
		{
			private readonly FlowFunctionCache<N, D, M> outerInstance;

			private heros.FlowFunctions<N, D, M> @delegate;

			public CacheLoaderAnonymousInnerClass2(FlowFunctionCache<N, D, M> outerInstance, heros.FlowFunctions<N, D, M> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FlowFunction<D> load(CallKey key) throws Exception
			public FlowFunction<D> load(CallKey key)
			{
				return @delegate.getCallFlowFunction(key.CallStmt, key.DestinationMethod);
			}
		}

		private class CacheLoaderAnonymousInnerClass3 : CacheLoader<ReturnKey, FlowFunction<D>>
		{
			private readonly FlowFunctionCache<N, D, M> outerInstance;

			private heros.FlowFunctions<N, D, M> @delegate;

			public CacheLoaderAnonymousInnerClass3(FlowFunctionCache<N, D, M> outerInstance, heros.FlowFunctions<N, D, M> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FlowFunction<D> load(ReturnKey key) throws Exception
			public FlowFunction<D> load(ReturnKey key)
			{
				return @delegate.getReturnFlowFunction(key.CallStmt, key.DestinationMethod, key.ExitStmt, key.ReturnSite);
			}
		}

		private class CacheLoaderAnonymousInnerClass4 : CacheLoader<NNKey, FlowFunction<D>>
		{
			private readonly FlowFunctionCache<N, D, M> outerInstance;

			private heros.FlowFunctions<N, D, M> @delegate;

			public CacheLoaderAnonymousInnerClass4(FlowFunctionCache<N, D, M> outerInstance, heros.FlowFunctions<N, D, M> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FlowFunction<D> load(NNKey key) throws Exception
			public FlowFunction<D> load(NNKey key)
			{
				return @delegate.getCallToReturnFlowFunction(key.Curr, key.Succ);
			}
		}

		public virtual FlowFunction<D> getNormalFlowFunction(N curr, N succ)
		{
			return normalCache.getUnchecked(new NNKey(this, curr, succ));
		}

		public virtual FlowFunction<D> getCallFlowFunction(N callStmt, M destinationMethod)
		{
			return callCache.getUnchecked(new CallKey(this, callStmt, destinationMethod));
		}

		public virtual FlowFunction<D> getReturnFlowFunction(N callSite, M calleeMethod, N exitStmt, N returnSite)
		{
			return returnCache.getUnchecked(new ReturnKey(this, callSite, calleeMethod, exitStmt, returnSite));
		}

		public virtual FlowFunction<D> getCallToReturnFlowFunction(N callSite, N returnSite)
		{
			return callToReturnCache.getUnchecked(new NNKey(this, callSite, returnSite));
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
				result = prime * result + ((curr == default(N)) ? 0 : curr.GetHashCode());
				result = prime * result + ((succ == default(N)) ? 0 : succ.GetHashCode());
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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") NNKey other = (NNKey) obj;
				NNKey other = (NNKey) obj;
				if (curr == default(N))
				{
					if (other.curr != default(N))
					{
						return false;
					}
				}
				else if (!curr.Equals(other.curr))
				{
					return false;
				}
				if (succ == default(N))
				{
					if (other.succ != default(N))
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
				result = prime * result + ((callStmt == default(N)) ? 0 : callStmt.GetHashCode());
				result = prime * result + ((destinationMethod == default(M)) ? 0 : destinationMethod.GetHashCode());
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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") CallKey other = (CallKey) obj;
				CallKey other = (CallKey) obj;
				if (callStmt == default(N))
				{
					if (other.callStmt != default(N))
					{
						return false;
					}
				}
				else if (!callStmt.Equals(other.callStmt))
				{
					return false;
				}
				if (destinationMethod == default(M))
				{
					if (other.destinationMethod != default(M))
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
				result = prime * result + ((exitStmt == default(N)) ? 0 : exitStmt.GetHashCode());
				result = prime * result + ((returnSite == default(N)) ? 0 : returnSite.GetHashCode());
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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") ReturnKey other = (ReturnKey) obj;
				ReturnKey other = (ReturnKey) obj;
				if (exitStmt == default(N))
				{
					if (other.exitStmt != default(N))
					{
						return false;
					}
				}
				else if (!exitStmt.Equals(other.exitStmt))
				{
					return false;
				}
				if (returnSite == default(N))
				{
					if (other.returnSite != default(N))
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

		public virtual void printStats()
		{
			logger.debug("Stats for flow-function cache:\n" + "Normal:         {}\n" + "Call:           {}\n" + "Return:         {}\n" + "Call-to-return: {}\n", normalCache.stats(), callCache.stats(),returnCache.stats(),callToReturnCache.stats());
		}

		public virtual void invalidate()
		{
			callCache.invalidateAll();
			callToReturnCache.invalidateAll();
			normalCache.invalidateAll();
			returnCache.invalidateAll();

		}

	}

}