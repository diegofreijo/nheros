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
	/// This is a template for <seealso cref="IDETabulationProblem"/>s that automatically caches values
	/// that ought to be cached. This class uses the Factory Method design pattern.
	/// The <seealso cref="InterproceduralCFG"/> is passed into the constructor so that it can be conveniently
	/// reused for solving multiple different <seealso cref="IDETabulationProblem"/>s.
	/// This class is specific to Soot. 
	/// </summary>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	/// @param <V> The type of values to be computed along flow edges. </param>
	/// @param <I> The type of inter-procedural control-flow graph being used. </param>
	public abstract class DefaultIDETabulationProblem<N, D, M, V, I> : DefaultIFDSTabulationProblem<N, D, M, I>, IDETabulationProblem<N, D, M, V, I> where I : heros.InterproceduralCFG<N,M>
	{

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly EdgeFunction<V> allTopFunction_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly JoinLattice<V> joinLattice_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly EdgeFunctions<N, D, M, V> edgeFunctions_Conflict;

		public DefaultIDETabulationProblem(I icfg) : base(icfg)
		{
			this.allTopFunction_Conflict = createAllTopFunction();
			this.joinLattice_Conflict = createJoinLattice();
			this.edgeFunctions_Conflict = createEdgeFunctionsFactory();
		}

		protected internal abstract EdgeFunction<V> createAllTopFunction();

		protected internal abstract JoinLattice<V> createJoinLattice();

		protected internal abstract EdgeFunctions<N, D, M, V> createEdgeFunctionsFactory();

		public EdgeFunction<V> allTopFunction()
		{
			return allTopFunction_Conflict;
		}

		public JoinLattice<V> joinLattice()
		{
			return joinLattice_Conflict;
		}

		public EdgeFunctions<N, D, M, V> edgeFunctions()
		{
			return edgeFunctions_Conflict;
		}

	}

}