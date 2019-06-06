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
	/// Defines an IDE tabulation problem as presented in the Sagiv, Reps, Horwitz 1996 
	/// (SRH96) paper. An IDE tabulation problem extends an <seealso cref="IFDSTabulationProblem"/>
	/// by allowing additional values to be computed along flow functions: each domain value
	/// of type D maps at any program point to a value of type V. The functions describe how
	/// values are transformed when moving from one statement to another.
	/// 
	/// The problem further defines a <seealso cref="JoinLattice"/>, which describes how values of
	/// type V are joined (merged) when multiple values are possible.
	/// </summary>
	/// @param <N> The type of nodes in the interprocedural control-flow graph. Typically <seealso cref="Unit"/>. </param>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	/// @param <M> The type of objects used to represent methods. Typically <seealso cref="SootMethod"/>. </param>
	/// @param <V> The type of values to be computed along flow edges. </param>
	/// @param <I> The type of inter-procedural control-flow graph being used. </param>
	public interface IDETabulationProblem<N, D, M, V, I> : IFDSTabulationProblem<N, D, M, I> where I : InterproceduralCFG<N,M>
	{

		/// <summary>
		/// Returns the edge functions that describe how V-values are transformed along
		/// flow function edges.
		/// </summary>
		EdgeFunctions<N, D, M, V> edgeFunctions();

		/// <summary>
		/// Returns the lattice describing how values of type V need to be joined.
		/// </summary>
		JoinLattice<V> joinLattice();

		/// <summary>
		/// Returns a function mapping everything to top.
		/// </summary>
		EdgeFunction<V> allTopFunction();
	}

}