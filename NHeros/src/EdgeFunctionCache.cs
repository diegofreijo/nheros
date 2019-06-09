using NHeros.src.util;
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
    public class EdgeFunctionCache<N, D, M, V> : EdgeFunctions<N, D, M, V>
    {

        readonly EdgeFunctions<N, D, M, V> @delegate;

        readonly LoadingCache<NDNDKey, EdgeFunction<V>> normalCache;
        readonly LoadingCache<CallKey, EdgeFunction<V>> callCache;
        readonly LoadingCache<ReturnKey, EdgeFunction<V>> returnCache;
        readonly LoadingCache<NDNDKey, EdgeFunction<V>> callToReturnCache;

        //internal Logger logger = LoggerFactory.getLogger(this.GetType());

        public EdgeFunctionCache(EdgeFunctions<N, D, M, V> @delegate)
        {
            this.@delegate = @delegate;

            normalCache = new LoadingCache<NDNDKey, EdgeFunction<V>>(
                (NDNDKey key) => @delegate.getNormalEdgeFunction(key.N1, key.D1, key.N2, key.D2)
            );

            callCache = new LoadingCache<CallKey, EdgeFunction<V>>(
                (CallKey key) => @delegate.getCallEdgeFunction(key.CallSite, key.D1, key.CalleeMethod, key.D2)
            );

            returnCache = new LoadingCache<ReturnKey, EdgeFunction<V>>(
                (ReturnKey key) => @delegate.getReturnEdgeFunction(key.CallSite, key.CalleeMethod, key.ExitStmt, key.D1, key.ReturnSite, key.D2)
            );

            callToReturnCache = new LoadingCache<NDNDKey, EdgeFunction<V>>(
                (NDNDKey key) => @delegate.getCallToReturnEdgeFunction(key.N1, key.D1, key.N2, key.D2)
            );
        }


        public virtual EdgeFunction<V> getNormalEdgeFunction(N curr, D currNode, N succ, D succNode)
        {
            return normalCache.Get(new NDNDKey(this, curr, currNode, succ, succNode));
        }

        public virtual EdgeFunction<V> getCallEdgeFunction(N callStmt, D srcNode, M destinationMethod, D destNode)
        {
            return callCache.Get(new CallKey(this, callStmt, srcNode, destinationMethod, destNode));
        }

        public virtual EdgeFunction<V> getReturnEdgeFunction(N callSite, M calleeMethod, N exitStmt, D exitNode, N returnSite, D retNode)
        {
            return returnCache.Get(new ReturnKey(this, callSite, calleeMethod, exitStmt, exitNode, returnSite, retNode));
        }

        public virtual EdgeFunction<V> getCallToReturnEdgeFunction(N callSite, D callNode, N returnSite, D returnSideNode)
        {
            return callToReturnCache.Get(new NDNDKey(this, callSite, callNode, returnSite, returnSideNode));
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
                result = prime * result + (Utils.IsDefault((d1)) ? 0 : d1.GetHashCode());
                result = prime * result + (Utils.IsDefault((d2)) ? 0 : d2.GetHashCode());
                result = prime * result + (Utils.IsDefault((n1)) ? 0 : n1.GetHashCode());
                result = prime * result + (Utils.IsDefault((n2)) ? 0 : n2.GetHashCode());
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
                
                //ORIGINAL LINE: @SuppressWarnings("unchecked") NDNDKey other = (NDNDKey) obj;
                NDNDKey other = (NDNDKey)obj;
                if (Utils.IsDefault(d1))
                {
                    if (!Utils.IsDefault(other.d1))
                    {
                        return false;
                    }
                }
                else if (!d1.Equals(other.d1))
                {
                    return false;
                }
                if (Utils.IsDefault(d2))
                {
                    if (!Utils.IsDefault(other.d2))
                    {
                        return false;
                    }
                }
                else if (!d2.Equals(other.d2))
                {
                    return false;
                }
                if (Utils.IsDefault(n1))
                {
                    if (!Utils.IsDefault(other.n1))
                    {
                        return false;
                    }
                }
                else if (!n1.Equals(other.n1))
                {
                    return false;
                }
                if (Utils.IsDefault(n2))
                {
                    if (!Utils.IsDefault(other.n2))
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
                result = prime * result + (Utils.IsDefault((d1)) ? 0 : d1.GetHashCode());
                result = prime * result + (Utils.IsDefault((d2)) ? 0 : d2.GetHashCode());
                result = prime * result + (Utils.IsDefault((callSite)) ? 0 : callSite.GetHashCode());
                result = prime * result + (Utils.IsDefault((calleeMethod)) ? 0 : calleeMethod.GetHashCode());
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
                
                //ORIGINAL LINE: @SuppressWarnings("unchecked") CallKey other = (CallKey) obj;
                CallKey other = (CallKey)obj;
                if (Utils.IsDefault(d1))
                {
                    if (!Utils.IsDefault(other.d1))
                    {
                        return false;
                    }
                }
                else if (!d1.Equals(other.d1))
                {
                    return false;
                }
                if (Utils.IsDefault(d2))
                {
                    if (!Utils.IsDefault(other.d2))
                    {
                        return false;
                    }
                }
                else if (!d2.Equals(other.d2))
                {
                    return false;
                }
                if (Utils.IsDefault(callSite))
                {
                    if (!Utils.IsDefault(other.callSite))
                    {
                        return false;
                    }
                }
                else if (!callSite.Equals(other.callSite))
                {
                    return false;
                }
                if (Utils.IsDefault(calleeMethod))
                {
                    if (!Utils.IsDefault(other.calleeMethod))
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
                result = prime * result + (Utils.IsDefault((exitStmt)) ? 0 : exitStmt.GetHashCode());
                result = prime * result + (Utils.IsDefault((returnSite)) ? 0 : returnSite.GetHashCode());
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
                ReturnKey other = (ReturnKey)obj;
                if (Utils.IsDefault(exitStmt))
                {
                    if (!Utils.IsDefault(other.exitStmt))
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
                    if (!Utils.IsDefault(other.returnSite))
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
        //    logger.debug("Stats for edge-function cache:\n" + "Normal:         {}\n" + "Call:           {}\n" + "Return:         {}\n" + "Call-to-return: {}\n", normalCache.stats(), callCache.stats(), returnCache.stats(), callToReturnCache.stats());
        //}

    }

}