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

	public class EdgeFunctionCache<N, D, M, V> : EdgeFunctions<N, D, M, V>
	{

		protected internal readonly EdgeFunctions<N, D, M, V> @delegate;

		protected internal readonly LoadingCache<NDNDKey, EdgeFunction<V>> normalCache;

		protected internal readonly LoadingCache<CallKey, EdgeFunction<V>> callCache;

		protected internal readonly LoadingCache<ReturnKey, EdgeFunction<V>> returnCache;

		protected internal readonly LoadingCache<NDNDKey, EdgeFunction<V>> callToReturnCache;

		internal Logger logger = LoggerFactory.getLogger(this.GetType());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public EdgeFunctionCache(final EdgeFunctions<N, D, M, V> delegate, @SuppressWarnings("rawtypes") com.google.common.cache.CacheBuilder builder)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public EdgeFunctionCache(EdgeFunctions<N, D, M, V> @delegate, CacheBuilder builder)
		{
			this.@delegate = @delegate;

			normalCache = builder.build(new CacheLoaderAnonymousInnerClass(this, @delegate));

			callCache = builder.build(new CacheLoaderAnonymousInnerClass2(this, @delegate));

			returnCache = builder.build(new CacheLoaderAnonymousInnerClass3(this, @delegate));

			callToReturnCache = builder.build(new CacheLoaderAnonymousInnerClass4(this, @delegate));
		}

		private class CacheLoaderAnonymousInnerClass : CacheLoader<NDNDKey, EdgeFunction<V>>
		{
			private readonly EdgeFunctionCache<N, D, M, V> outerInstance;

			private heros.EdgeFunctions<N, D, M, V> @delegate;

			public CacheLoaderAnonymousInnerClass(EdgeFunctionCache<N, D, M, V> outerInstance, heros.EdgeFunctions<N, D, M, V> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EdgeFunction<V> load(NDNDKey key) throws Exception
			public EdgeFunction<V> load(NDNDKey key)
			{
				return @delegate.getNormalEdgeFunction(key.N1, key.D1, key.N2, key.D2);
			}
		}

		private class CacheLoaderAnonymousInnerClass2 : CacheLoader<CallKey, EdgeFunction<V>>
		{
			private readonly EdgeFunctionCache<N, D, M, V> outerInstance;

			private heros.EdgeFunctions<N, D, M, V> @delegate;

			public CacheLoaderAnonymousInnerClass2(EdgeFunctionCache<N, D, M, V> outerInstance, heros.EdgeFunctions<N, D, M, V> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EdgeFunction<V> load(CallKey key) throws Exception
			public EdgeFunction<V> load(CallKey key)
			{
				return @delegate.getCallEdgeFunction(key.CallSite, key.D1, key.CalleeMethod, key.D2);
			}
		}

		private class CacheLoaderAnonymousInnerClass3 : CacheLoader<ReturnKey, EdgeFunction<V>>
		{
			private readonly EdgeFunctionCache<N, D, M, V> outerInstance;

			private heros.EdgeFunctions<N, D, M, V> @delegate;

			public CacheLoaderAnonymousInnerClass3(EdgeFunctionCache<N, D, M, V> outerInstance, heros.EdgeFunctions<N, D, M, V> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EdgeFunction<V> load(ReturnKey key) throws Exception
			public EdgeFunction<V> load(ReturnKey key)
			{
				return @delegate.getReturnEdgeFunction(key.CallSite, key.CalleeMethod, key.ExitStmt, key.D1, key.ReturnSite, key.D2);
			}
		}

		private class CacheLoaderAnonymousInnerClass4 : CacheLoader<NDNDKey, EdgeFunction<V>>
		{
			private readonly EdgeFunctionCache<N, D, M, V> outerInstance;

			private heros.EdgeFunctions<N, D, M, V> @delegate;

			public CacheLoaderAnonymousInnerClass4(EdgeFunctionCache<N, D, M, V> outerInstance, heros.EdgeFunctions<N, D, M, V> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EdgeFunction<V> load(NDNDKey key) throws Exception
			public EdgeFunction<V> load(NDNDKey key)
			{
				return @delegate.getCallToReturnEdgeFunction(key.N1, key.D1, key.N2, key.D2);
			}
		}

		public virtual EdgeFunction<V> getNormalEdgeFunction(N curr, D currNode, N succ, D succNode)
		{
			return normalCache.getUnchecked(new NDNDKey(this, curr, currNode, succ, succNode));
		}

		public virtual EdgeFunction<V> getCallEdgeFunction(N callStmt, D srcNode, M destinationMethod, D destNode)
		{
			return callCache.getUnchecked(new CallKey(this, callStmt, srcNode, destinationMethod, destNode));
		}

		public virtual EdgeFunction<V> getReturnEdgeFunction(N callSite, M calleeMethod, N exitStmt, D exitNode, N returnSite, D retNode)
		{
			return returnCache.getUnchecked(new ReturnKey(this, callSite, calleeMethod, exitStmt, exitNode, returnSite, retNode));
		}

		public virtual EdgeFunction<V> getCallToReturnEdgeFunction(N callSite, D callNode, N returnSite, D returnSideNode)
		{
			return callToReturnCache.getUnchecked(new NDNDKey(this, callSite, callNode, returnSite, returnSideNode));
		}


		private class NDNDKey
		{
			private readonly EdgeFunctionCache<N, D, M, V> outerInstance;

			internal readonly N n1, n2;
			internal readonly D d1, d2;

			public NDNDKey(EdgeFunctionCache<N, D, M, V> outerInstance, N n1, D d1, N n2, D d2)
			{
				this.outerInstance = outerInstance;
				this.n1 = n1;
				this.n2 = n2;
				this.d1 = d1;
				this.d2 = d2;
			}

			public virtual N N1
			{
				get
				{
					return n1;
				}
			}

			public virtual D D1
			{
				get
				{
					return d1;
				}
			}

			public virtual N N2
			{
				get
				{
					return n2;
				}
			}

			public virtual D D2
			{
				get
				{
					return d2;
				}
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + ((d1 == default(D)) ? 0 : d1.GetHashCode());
				result = prime * result + ((d2 == default(D)) ? 0 : d2.GetHashCode());
				result = prime * result + ((n1 == default(N)) ? 0 : n1.GetHashCode());
				result = prime * result + ((n2 == default(N)) ? 0 : n2.GetHashCode());
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
//ORIGINAL LINE: @SuppressWarnings("unchecked") NDNDKey other = (NDNDKey) obj;
				NDNDKey other = (NDNDKey) obj;
				if (d1 == default(D))
				{
					if (other.d1 != default(D))
					{
						return false;
					}
				}
				else if (!d1.Equals(other.d1))
				{
					return false;
				}
				if (d2 == default(D))
				{
					if (other.d2 != default(D))
					{
						return false;
					}
				}
				else if (!d2.Equals(other.d2))
				{
					return false;
				}
				if (n1 == default(N))
				{
					if (other.n1 != default(N))
					{
						return false;
					}
				}
				else if (!n1.Equals(other.n1))
				{
					return false;
				}
				if (n2 == default(N))
				{
					if (other.n2 != default(N))
					{
						return false;
					}
				}
				else if (!n2.Equals(other.n2))
				{
					return false;
				}
				return true;
			}
		}

		private class CallKey
		{
			private readonly EdgeFunctionCache<N, D, M, V> outerInstance;

			internal readonly N callSite;
			internal readonly M calleeMethod;
			internal readonly D d1, d2;

			public CallKey(EdgeFunctionCache<N, D, M, V> outerInstance, N callSite, D d1, M calleeMethod, D d2)
			{
				this.outerInstance = outerInstance;
				this.callSite = callSite;
				this.calleeMethod = calleeMethod;
				this.d1 = d1;
				this.d2 = d2;
			}

			public virtual N CallSite
			{
				get
				{
					return callSite;
				}
			}

			public virtual D D1
			{
				get
				{
					return d1;
				}
			}

			public virtual M CalleeMethod
			{
				get
				{
					return calleeMethod;
				}
			}

			public virtual D D2
			{
				get
				{
					return d2;
				}
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + ((d1 == default(D)) ? 0 : d1.GetHashCode());
				result = prime * result + ((d2 == default(D)) ? 0 : d2.GetHashCode());
				result = prime * result + ((callSite == default(N)) ? 0 : callSite.GetHashCode());
				result = prime * result + ((calleeMethod == default(M)) ? 0 : calleeMethod.GetHashCode());
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
				if (d1 == default(D))
				{
					if (other.d1 != default(D))
					{
						return false;
					}
				}
				else if (!d1.Equals(other.d1))
				{
					return false;
				}
				if (d2 == default(D))
				{
					if (other.d2 != default(D))
					{
						return false;
					}
				}
				else if (!d2.Equals(other.d2))
				{
					return false;
				}
				if (callSite == default(N))
				{
					if (other.callSite != default(N))
					{
						return false;
					}
				}
				else if (!callSite.Equals(other.callSite))
				{
					return false;
				}
				if (calleeMethod == default(M))
				{
					if (other.calleeMethod != default(M))
					{
						return false;
					}
				}
				else if (!calleeMethod.Equals(other.calleeMethod))
				{
					return false;
				}
				return true;
			}
		}


		private class ReturnKey : CallKey
		{
			private readonly EdgeFunctionCache<N, D, M, V> outerInstance;


			internal readonly N exitStmt, returnSite;

			public ReturnKey(EdgeFunctionCache<N, D, M, V> outerInstance, N callSite, M calleeMethod, N exitStmt, D exitNode, N returnSite, D retNode) : base(outerInstance, callSite, exitNode, calleeMethod, retNode)
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
			logger.debug("Stats for edge-function cache:\n" + "Normal:         {}\n" + "Call:           {}\n" + "Return:         {}\n" + "Call-to-return: {}\n", normalCache.stats(), callCache.stats(),returnCache.stats(),callToReturnCache.stats());
		}

	}

}