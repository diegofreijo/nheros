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
	/// <summary>
	/// A wrapper that can be used to profile flow functions.
	/// </summary>
	public class ProfiledFlowFunctions<N, D, M> : FlowFunctions<N, D, M>
	{

		protected internal readonly FlowFunctions<N, D, M> @delegate;

		public long durationNormal, durationCall, durationReturn, durationCallReturn;

		public ProfiledFlowFunctions(FlowFunctions<N, D, M> @delegate)
		{
			this.@delegate = @delegate;
		}

		public virtual FlowFunction<D> getNormalFlowFunction(N curr, N succ)
		{
			long before = DateTimeHelper.CurrentUnixTimeMillis();
			FlowFunction<D> ret = @delegate.getNormalFlowFunction(curr, succ);
			long duration = DateTimeHelper.CurrentUnixTimeMillis() - before;
			durationNormal += duration;
			return ret;
		}

		public virtual FlowFunction<D> getCallFlowFunction(N callStmt, M destinationMethod)
		{
			long before = DateTimeHelper.CurrentUnixTimeMillis();
			FlowFunction<D> res = @delegate.getCallFlowFunction(callStmt, destinationMethod);
			long duration = DateTimeHelper.CurrentUnixTimeMillis() - before;
			durationCall += duration;
			return res;
		}

		public virtual FlowFunction<D> getReturnFlowFunction(N callSite, M calleeMethod, N exitStmt, N returnSite)
		{
			long before = DateTimeHelper.CurrentUnixTimeMillis();
			FlowFunction<D> res = @delegate.getReturnFlowFunction(callSite, calleeMethod, exitStmt, returnSite);
			long duration = DateTimeHelper.CurrentUnixTimeMillis() - before;
			durationReturn += duration;
			return res;
		}

		public virtual FlowFunction<D> getCallToReturnFlowFunction(N callSite, N returnSite)
		{
			long before = DateTimeHelper.CurrentUnixTimeMillis();
			FlowFunction<D> res = @delegate.getCallToReturnFlowFunction(callSite, returnSite);
			long duration = DateTimeHelper.CurrentUnixTimeMillis() - before;
			durationCallReturn += duration;
			return res;
		}

	}

}