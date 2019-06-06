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


	internal class CallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>>
	{

		public CallEdgeResolver(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger) : this(analyzer, debugger, null)
		{
		}

		public CallEdgeResolver(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger, CallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> parent) : base(analyzer, analyzer.AccessPath, parent, debugger)
		{
		}

		protected internal override AccessPath<System.Reflection.FieldInfo> getAccessPathOf(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> inc)
		{
			return inc.CalleeSourceFact.AccessPath;
		}

		protected internal override void processIncomingGuaranteedPrefix(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> inc)
		{
			analyzer.applySummaries(inc);
		}

		protected internal override void processIncomingPotentialPrefix(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> inc)
		{
			@lock();
			inc.registerInterestCallback(analyzer);
			unlock();
		}

		protected internal override ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> createNestedResolver(AccessPath<System.Reflection.FieldInfo> newAccPath)
		{
			return analyzer.createWithAccessPath(newAccPath).CallEdgeResolver;
		}

		public virtual void applySummaries(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			foreach (CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge in Lists.newLinkedList(incomingEdges))
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