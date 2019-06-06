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
///     John Toman - adds recordEdge option
/// *****************************************************************************
/// </summary>
namespace heros
{
	using IDESolver = heros.solver.IDESolver;

	/// <summary>
	/// Configuration parameters for <seealso cref="IDESolver"/>.
	/// </summary>
	public interface SolverConfiguration
	{


		/// <summary>
		/// If true, the analysis will compute a partially unbalanced analysis problem in which
		/// function returns are followed also further up the call stack than where the initial seeds
		/// started.
		/// 
		/// If this is enabled, when reaching the exit of a method that is <i>nowhere</i> called, in order
		/// to avoid not at all processing the exit statement, the <seealso cref="IDESolver"/> will call
		/// the <i>return</i> flow function with a <code>null</code> call site and return site.
		/// </summary>
		bool followReturnsPastSeeds();

		/// <summary>
		/// If true, the solver will automatically add the zero value to each flow-function call's result set. </summary>
		/// <seealso cref= #zeroValue() </seealso>
		bool autoAddZero();

		/// <summary>
		/// Returns the number of threads to be used by the solver. 
		/// </summary>
		int numThreads();

		/// <summary>
		/// If false, then the solver will only compute the exploded super graph but not propagate values.
		/// This can save time for IFDS problems where all of the interesting results are collected already
		/// during the computation of the super graph.
		/// </summary>
		bool computeValues();

		/// <summary>
		/// Returns true if the solver should record the intermediate flow edges
		/// created by calling the methods on <seealso cref="IFDSTabulationProblem.flowFunctions()"/>.
		/// These nodes are not used by the solver, but may be useful for debugging the solver
		/// or flow functions (see <seealso cref="FlowFunctionDotExport"/>). 
		/// </summary>
		bool recordEdges();
	}

}