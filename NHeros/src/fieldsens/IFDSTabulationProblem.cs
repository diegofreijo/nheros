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
namespace heros.fieldsens
{
	using IFDSSolver = heros.solver.IFDSSolver;
	using Pair = heros.solver.Pair;




	/// <summary>
	/// A tabulation problem for solving in an <seealso cref="IFDSSolver"/> as described
	/// by the Reps, Horwitz, Sagiv 1995 (RHS95) paper.
	/// </summary>
	/// @param <N> The type of nodes in the interprocedural control-flow graph. Typically <seealso cref="Unit"/>. </param>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	/// @param <M> The type of objects used to represent methods. Typically <seealso cref="SootMethod"/>. </param>
	/// @param <I> The type of inter-procedural control-flow graph being used. </param>
	public interface IFDSTabulationProblem<N, FieldRef, D, M, I> : SolverConfiguration where I : heros.InterproceduralCFG<N,M>
	{

		/// <summary>
		/// Returns a set of flow functions. Those functions are used to compute data-flow facts
		/// along the various kinds of control flows.
		///    
		/// <b>NOTE:</b> this method could be called many times. Implementations of this
		/// interface should therefore cache the return value! 
		/// </summary>
		FlowFunctions<N, FieldRef, D, M> flowFunctions();

		/// <summary>
		/// Returns the interprocedural control-flow graph which this problem is computed over.
		/// 
		/// <b>NOTE:</b> this method could be called many times. Implementations of this
		/// interface should therefore cache the return value! 
		/// </summary>
		I interproceduralCFG();

		/// <summary>
		/// Returns initial seeds to be used for the analysis. This is a mapping of statements to initial analysis facts.
		/// </summary>
		IDictionary<N, ISet<D>> initialSeeds();

		/// <summary>
		/// This must be a data-flow fact of type <seealso cref="D"/>, but must <i>not</i>
		/// be part of the domain of data-flow facts. Typically this will be a
		/// singleton object of type <seealso cref="D"/> that is used for nothing else.
		/// It must holds that this object does not equals any object 
		/// within the domain.
		/// 
		/// <b>NOTE:</b> this method could be called many times. Implementations of this
		/// interface should therefore cache the return value! 
		/// </summary>
		D zeroValue();

		ZeroHandler<FieldRef> zeroHandler();

	}

}