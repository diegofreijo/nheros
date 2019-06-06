using System.Collections.Generic;

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

	public class ZeroedFlowFunctions<N, D, M> : FlowFunctions<N, D, M>
	{

		protected internal readonly FlowFunctions<N, D, M> @delegate;
		protected internal readonly D zeroValue;

		public ZeroedFlowFunctions(FlowFunctions<N, D, M> @delegate, D zeroValue)
		{
			this.@delegate = @delegate;
			this.zeroValue = zeroValue;
		}

		public virtual FlowFunction<D> getNormalFlowFunction(N curr, N succ)
		{
			return new ZeroedFlowFunction(this, @delegate.getNormalFlowFunction(curr, succ));
		}

		public virtual FlowFunction<D> getCallFlowFunction(N callStmt, M destinationMethod)
		{
			return new ZeroedFlowFunction(this, @delegate.getCallFlowFunction(callStmt, destinationMethod));
		}

		public virtual FlowFunction<D> getReturnFlowFunction(N callSite, M calleeMethod, N exitStmt, N returnSite)
		{
			return new ZeroedFlowFunction(this, @delegate.getReturnFlowFunction(callSite, calleeMethod, exitStmt, returnSite));
		}

		public virtual FlowFunction<D> getCallToReturnFlowFunction(N callSite, N returnSite)
		{
			return new ZeroedFlowFunction(this, @delegate.getCallToReturnFlowFunction(callSite, returnSite));
		}

		protected internal class ZeroedFlowFunction : FlowFunction<D>
		{
			private readonly ZeroedFlowFunctions<N, D, M> outerInstance;


			protected internal FlowFunction<D> del;

			internal ZeroedFlowFunction(ZeroedFlowFunctions<N, D, M> outerInstance, FlowFunction<D> del)
			{
				this.outerInstance = outerInstance;
				this.del = del;
			}

			public virtual ISet<D> computeTargets(D source)
			{
				if (source == outerInstance.zeroValue)
				{
					HashSet<D> res = new LinkedHashSet<D>(del.computeTargets(source));
					res.Add(outerInstance.zeroValue);
					return res;
				}
				else
				{
					return del.computeTargets(source);
				}
			}

		}


	}

}