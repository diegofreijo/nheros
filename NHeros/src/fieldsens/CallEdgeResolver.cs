/// <summary>
///*****************************************************************************
/// Copyright (c) 2015 Johannes Lerch.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.fieldsens
{
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;

	using Lists = com.google.common.collect.Lists;


	internal class CallEdgeResolver<Field, Fact, Stmt, Method> : ResolverTemplate<Field, Fact, Stmt, Method, CallEdge<Field, Fact, Stmt, Method>>
	{

		public CallEdgeResolver(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Debugger<Field, Fact, Stmt, Method> debugger) : this(analyzer, debugger, null)
		{
		}

		public CallEdgeResolver(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Debugger<Field, Fact, Stmt, Method> debugger, CallEdgeResolver<Field, Fact, Stmt, Method> parent) : base(analyzer, analyzer.AccessPath, parent, debugger)
		{
		}

		protected internal override AccessPath<Field> getAccessPathOf(CallEdge<Field, Fact, Stmt, Method> inc)
		{
			return inc.CalleeSourceFact.AccessPath;
		}

		protected internal override void processIncomingGuaranteedPrefix(CallEdge<Field, Fact, Stmt, Method> inc)
		{
			analyzer.applySummaries(inc);
		}

		protected internal override void processIncomingPotentialPrefix(CallEdge<Field, Fact, Stmt, Method> inc)
		{
			@lock();
			inc.registerInterestCallback(analyzer);
			unlock();
		}

		protected internal override ResolverTemplate<Field, Fact, Stmt, Method, CallEdge<Field, Fact, Stmt, Method>> createNestedResolver(AccessPath<Field> newAccPath)
		{
			return analyzer.createWithAccessPath(newAccPath).CallEdgeResolver;
		}

		public virtual void applySummaries(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{
			foreach (CallEdge<Field, Fact, Stmt, Method> incEdge in Lists.newLinkedList(incomingEdges))
			{
				analyzer.applySummary(incEdge, factAtStmt);
			}
		}

		public override string ToString()
		{
			return "<" + analyzer.AccessPath + ":" + analyzer.Method + ">";
		}

		protected internal override void log(string message)
		{
			analyzer.log(message);
		}

		public virtual bool hasIncomingEdges()
		{
			return incomingEdges.Count > 0;
		}


	}
}