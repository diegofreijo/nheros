using heros.edgefunc;
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
	/// <summary>
	/// A solver for an <seealso cref="IFDSTabulationProblem"/>. This solver in effect uses the <seealso cref="IDESolver"/>
	/// to solve the problem, as any IFDS problem can be intepreted as a special case of an IDE problem.
	/// See Section 5.4.1 of the SRH96 paper. In effect, the IFDS problem is solved by solving an IDE
	/// problem in which the environments (D to N mappings) represent the set's characteristic function.
	/// </summary>
	/// @param <N> The type of nodes in the interprocedural control-flow graph. Typically <seealso cref="Unit"/>. </param>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	/// @param <M> The type of objects used to represent methods. Typically <seealso cref="SootMethod"/>. </param>
	/// @param <I> The type of inter-procedural control-flow graph being used. </param>
	/// <seealso cref= IFDSTabulationProblem </seealso>
	public class IFDSSolver<N, D, M, I> : IDESolver<N, D, M, IFDSSolver<N, D, M, I>.BinaryDomain, I> 
        where I : InterproceduralCFG<N, M>
    {

		protected internal enum BinaryDomain
		{
			TOP,
			BOTTOM
		}

		private static readonly EdgeFunction<BinaryDomain> ALL_BOTTOM = new AllBottom<BinaryDomain>(BinaryDomain.BOTTOM);

		/// <summary>
		/// Creates a solver for the given problem. The solver must then be started by calling
		/// <seealso cref="solve()"/>.
		/// </summary>
        public IFDSSolver(IFDSTabulationProblem<N, D, M, I> ifdsProblem) : base(createIDETabulationProblem(ifdsProblem))
		{
		}

        internal static IDETabulationProblem<N, D, M, BinaryDomain, I> createIDETabulationProblem(IFDSTabulationProblem<N, D, M, I> ifdsProblem) 
        {
			return new IDETabulationProblemAnonymousInnerClass(ifdsProblem);
		}

		private class IDETabulationProblemAnonymousInnerClass : IDETabulationProblem<N, D, M, BinaryDomain, I>
		{
			private IFDSTabulationProblem<N, D, M, I> ifdsProblem;

			public IDETabulationProblemAnonymousInnerClass(IFDSTabulationProblem<N, D, M, I> ifdsProblem)
			{
				this.ifdsProblem = ifdsProblem;
			}


			public FlowFunctions<N, D, M> flowFunctions()
			{
				return ifdsProblem.flowFunctions();
			}

			public I interproceduralCFG()
			{
				return ifdsProblem.interproceduralCFG();
			}

			public IDictionary<N, ISet<D>> initialSeeds()
			{
				return ifdsProblem.initialSeeds();
			}

			public D zeroValue()
			{
				return ifdsProblem.zeroValue();
			}

			public EdgeFunctions<N, D, M, BinaryDomain> edgeFunctions()
			{
				return new IFDSEdgeFunctions(this);
			}

			public JoinLattice<BinaryDomain> joinLattice()
			{
				return new JoinLatticeAnonymousInnerClass(this);
			}

			private class JoinLatticeAnonymousInnerClass : JoinLattice<BinaryDomain>
			{
				private readonly IDETabulationProblemAnonymousInnerClass outerInstance;

				public JoinLatticeAnonymousInnerClass(IDETabulationProblemAnonymousInnerClass outerInstance)
				{
					this.outerInstance = outerInstance;
				}


				public BinaryDomain topElement()
				{
					return BinaryDomain.TOP;
				}

				public BinaryDomain bottomElement()
				{
					return BinaryDomain.BOTTOM;
				}

				public BinaryDomain join(BinaryDomain left, BinaryDomain right)
				{
					if (left == BinaryDomain.TOP && right == BinaryDomain.TOP)
					{
						return BinaryDomain.TOP;
					}
					else
					{
						return BinaryDomain.BOTTOM;
					}
				}
			}

			public EdgeFunction<BinaryDomain> allTopFunction()
			{
				return new AllTop<BinaryDomain>(BinaryDomain.TOP);
			}

			public bool followReturnsPastSeeds()
			{
				return ifdsProblem.followReturnsPastSeeds();
			}

			public bool autoAddZero()
			{
				return ifdsProblem.autoAddZero();
			}

			public int numThreads()
			{
				return ifdsProblem.numThreads();
			}

			public bool computeValues()
			{
				return ifdsProblem.computeValues();
			}

			internal class IFDSEdgeFunctions : EdgeFunctions<N, D, M, BinaryDomain>
			{
				private readonly IDETabulationProblemAnonymousInnerClass outerInstance;

				public IFDSEdgeFunctions(IDETabulationProblemAnonymousInnerClass outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual EdgeFunction<BinaryDomain> getNormalEdgeFunction(N src, D srcNode, N tgt, D tgtNode)
				{
					if (srcNode.Equals(outerInstance.ifdsProblem.zeroValue()))
					{
						return ALL_BOTTOM;
					}
					return EdgeIdentity<BinaryDomain>.v();
				}

				public virtual EdgeFunction<BinaryDomain> getCallEdgeFunction(N callStmt, D srcNode, M destinationMethod, D destNode)
				{
					if (srcNode.Equals(outerInstance.ifdsProblem.zeroValue()))
					{
						return ALL_BOTTOM;
					}
					return EdgeIdentity<BinaryDomain>.v();
				}

				public virtual EdgeFunction<BinaryDomain> getReturnEdgeFunction(N callSite, M calleeMethod, N exitStmt, D exitNode, N returnSite, D retNode)
				{
					if (exitNode.Equals(outerInstance.ifdsProblem.zeroValue()))
					{
						return ALL_BOTTOM;
					}
					return EdgeIdentity<BinaryDomain>.v();
				}

				public virtual EdgeFunction<BinaryDomain> getCallToReturnEdgeFunction(N callStmt, D callNode, N returnSite, D returnSideNode)
				{
					if (callNode.Equals(outerInstance.ifdsProblem.zeroValue()))
					{
						return ALL_BOTTOM;
					}
					return EdgeIdentity<BinaryDomain>.v();
				}
			}

			public bool recordEdges()
			{
				return ifdsProblem.recordEdges();
			}

		}

		/// <summary>
		/// Returns the set of facts that hold at the given statement.
		/// </summary>
		public virtual ICollection<D> ifdsResultsAt(N statement)
		{
			return resultsAt(statement).Keys;
		}

	}

}