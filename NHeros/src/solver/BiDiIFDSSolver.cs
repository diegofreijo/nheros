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
namespace heros.solver
{
	using BinaryDomain = heros.solver.IFDSSolver.BinaryDomain;


	using Maps = com.google.common.collect.Maps;

	/// <summary>
	/// This is a special IFDS solver that solves the analysis problem inside out, i.e., from further down the call stack to
	/// further up the call stack. This can be useful, for instance, for taint analysis problems that track flows in two directions.
	/// 
	/// The solver is instantiated with two analyses, one to be computed forward and one to be computed backward. Both analysis problems
	/// must be unbalanced, i.e., must return <code>true</code> for <seealso cref="IFDSTabulationProblem.followReturnsPastSeeds()"/>.
	/// The solver then executes both analyses in lockstep, i.e., when one of the analyses reaches an unbalanced return edge (signified
	/// by a ZERO source value) then the solver pauses this analysis until the other analysis reaches the same unbalanced return (if ever).
	/// The result is that the analyses will never diverge, i.e., will ultimately always only propagate into contexts in which both their
	/// computed paths are realizable at the same time.
	/// 
	/// This solver requires data-flow abstractions that implement the <seealso cref="LinkedNode"/> interface such that data-flow values can be linked to form
	/// reportable paths.  
	/// </summary>
	/// @param <N> see <seealso cref="IFDSSolver"/> </param>
	/// @param <D> A data-flow abstraction that must implement the <seealso cref="LinkedNode"/> interface such that data-flow values can be linked to form
	/// 				reportable paths. </param>
	/// @param <M> see <seealso cref="IFDSSolver"/> </param>
	/// @param <I> see <seealso cref="IFDSSolver"/> </param>
	public class BiDiIFDSSolver<N, D, M, I> : BiDiIDESolver<N, D, M, BinaryDomain, I> 
        where D : JoinHandlingNode<D> 
        where I : heros.InterproceduralCFG<N, M>
	{


		/// <summary>
		/// Instantiates a <seealso cref="BiDiIFDSSolver"/> with the associated forward and backward problem.
		/// </summary>
		public BiDiIFDSSolver(IFDSTabulationProblem<N, D, M, I> forwardProblem, IFDSTabulationProblem<N, D, M, I> backwardProblem) 
            : base(IFDSSolver.createIDETabulationProblem(forwardProblem), IFDSSolver.createIDETabulationProblem(backwardProblem))
		{
		}

		public virtual ISet<D> fwIFDSResultAt(N stmt)
		{
			return extractResults(fwSolver.resultsAt(stmt).Keys);
		}

		public virtual ISet<D> bwIFDSResultAt(N stmt)
		{
			return extractResults(bwSolver.resultsAt(stmt).Keys);
		}

		private ISet<D> extractResults(ISet<AbstractionWithSourceStmt> annotatedResults)
		{
			ISet<D> res = new HashSet<D>();
			foreach (AbstractionWithSourceStmt abstractionWithSourceStmt in annotatedResults)
			{
				res.Add(abstractionWithSourceStmt.Abstraction);
			}
			return res;
		}

	}

}