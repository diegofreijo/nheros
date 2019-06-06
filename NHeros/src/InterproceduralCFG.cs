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

	/// <summary>
	/// An interprocedural control-flow graph.
	/// </summary>
	/// @param <N> Nodes in the CFG, typically <seealso cref="Unit"/> or <seealso cref="Block"/> </param>
	/// @param <M> Method representation </param>
	public interface InterproceduralCFG<N, M>
	{

		/// <summary>
		/// Returns the method containing a node. </summary>
		/// <param name="n"> The node for which to get the parent method </param>
		M getMethodOf(N n);

		IList<N> getPredsOf(N u);

		/// <summary>
		/// Returns the successor nodes.
		/// </summary>
		IList<N> getSuccsOf(N n);

		/// <summary>
		/// Returns all callee methods for a given call.
		/// </summary>
		ICollection<M> getCalleesOfCallAt(N n);

		/// <summary>
		/// Returns all caller statements/nodes of a given method.
		/// </summary>
		ICollection<N> getCallersOf(M m);

		/// <summary>
		/// Returns all call sites within a given method.
		/// </summary>
		ISet<N> getCallsFromWithin(M m);

		/// <summary>
		/// Returns all start points of a given method. There may be
		/// more than one start point in case of a backward analysis.
		/// </summary>
		ICollection<N> getStartPointsOf(M m);

		/// <summary>
		/// Returns all statements to which a call could return.
		/// In the RHS paper, for every call there is just one return site.
		/// We, however, use as return site the successor statements, of which
		/// there can be many in case of exceptional flow.
		/// </summary>
		ICollection<N> getReturnSitesOfCallAt(N n);

		/// <summary>
		/// Returns <code>true</code> if the given statement is a call site.
		/// </summary>
		bool isCallStmt(N stmt);

		/// <summary>
		/// Returns <code>true</code> if the given statement leads to a method return
		/// (exceptional or not). For backward analyses may also be start statements.
		/// </summary>
		bool isExitStmt(N stmt);

		/// <summary>
		/// Returns true is this is a method's start statement. For backward analyses
		/// those may also be return or throws statements.
		/// </summary>
		bool isStartPoint(N stmt);

		/// <summary>
		/// Returns the set of all nodes that are neither call nor start nodes.
		/// </summary>
		ISet<N> allNonCallStartNodes();

		/// <summary>
		/// Returns whether succ is the fall-through successor of stmt,
		/// i.e., the unique successor that is be reached when stmt
		/// does not branch.
		/// </summary>
		bool isFallThroughSuccessor(N stmt, N succ);

		/// <summary>
		/// Returns whether succ is a branch target of stmt. 
		/// </summary>
		bool isBranchTarget(N stmt, N succ);

	}

}