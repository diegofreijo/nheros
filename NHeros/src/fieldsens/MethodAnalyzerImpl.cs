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
	using WrappedFact = heros.fieldsens.structs.WrappedFact;
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;
	using DefaultValueMap = heros.utilities.DefaultValueMap;

	public class MethodAnalyzerImpl<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : MethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private System.Reflection.MethodInfo method;
		private DefaultValueMap<Fact, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> perSourceAnalyzer = new DefaultValueMapAnonymousInnerClass();

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<Fact, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>>
		{
			protected internal override PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> createItem(Fact key)
			{
				return new PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(outerInstance.method, key, outerInstance.context, outerInstance.debugger);
			}
		}
		private Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> context;
		private Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger;

		internal MethodAnalyzerImpl(System.Reflection.MethodInfo method, Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> context, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger)
		{
			this.method = method;
			this.context = context;
			this.debugger = debugger;
		}

		public virtual void addIncomingEdge(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge)
		{
			WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> calleeSourceFact = incEdge.CalleeSourceFact;
			PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer = perSourceAnalyzer.getOrCreate(calleeSourceFact.Fact);
			analyzer.addIncomingEdge(incEdge);
		}

		public virtual void addInitialSeed(Stmt startPoint, Fact val)
		{
			perSourceAnalyzer.getOrCreate(val).addInitialSeed(startPoint);
		}

		public virtual void addUnbalancedReturnFlow(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> target, Stmt callSite)
		{
			perSourceAnalyzer.getOrCreate(context.zeroValue).scheduleUnbalancedReturnEdgeTo(target);
		}
	}

}