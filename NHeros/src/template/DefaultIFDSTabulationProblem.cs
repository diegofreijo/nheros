using NHeros.src.util;
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
namespace heros.template
{

	/// <summary>
	/// This is a template for <seealso cref="IFDSTabulationProblem"/>s that automatically caches values
	/// that ought to be cached. This class uses the Factory Method design pattern.
	/// The <seealso cref="InterproceduralCFG"/> is passed into the constructor so that it can be conveniently
	/// reused for solving multiple different <seealso cref="IFDSTabulationProblem"/>s.
	/// This class is specific to Soot. 
	/// </summary>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	public abstract class DefaultIFDSTabulationProblem<N, D, M, I> : IFDSTabulationProblem<N, D, M, I> where I : heros.InterproceduralCFG<N,M>
	{
		public abstract IDictionary<N, ISet<D>> initialSeeds();

		private readonly I icfg;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private FlowFunctions<N, D, M> flowFunctions_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private D zeroValue_Conflict;

		public DefaultIFDSTabulationProblem(I icfg)
		{
			this.icfg = icfg;
		}

		protected internal abstract FlowFunctions<N, D, M> createFlowFunctionsFactory();

		protected internal abstract D createZeroValue();

		public FlowFunctions<N, D, M> flowFunctions()
		{
			if (flowFunctions_Conflict == null)
			{
				flowFunctions_Conflict = createFlowFunctionsFactory();
			}
			return flowFunctions_Conflict;
		}

		public virtual I interproceduralCFG()
		{
			return icfg;
		}

		public D zeroValue()
		{
			if (Utils.IsDefault(zeroValue_Conflict))
			{
				zeroValue_Conflict = createZeroValue();
			}
			return zeroValue_Conflict;
		}

		public override bool followReturnsPastSeeds()
		{
			return false;
		}

		public override bool autoAddZero()
		{
			return true;
		}

		public override int numThreads()
		{
			return Runtime.Runtime.availableProcessors();
		}

		public override bool computeValues()
		{
			return true;
		}

		public override bool recordEdges()
		{
			return false;
		}
	}

}