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

	public class MethodAnalyzerImpl<Field, Fact, Stmt, Method> : MethodAnalyzer<Field, Fact, Stmt, Method>
	{

		private Method method;
		private DefaultValueMap<Fact, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>> perSourceAnalyzer = new DefaultValueMapAnonymousInnerClass();

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<Fact, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>>
		{
			protected internal override PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> createItem(Fact key)
			{
				return new PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>(outerInstance.method, key, outerInstance.context, outerInstance.debugger);
			}
		}
		private Context<Field, Fact, Stmt, Method> context;
		private Debugger<Field, Fact, Stmt, Method> debugger;

		internal MethodAnalyzerImpl(Method method, Context<Field, Fact, Stmt, Method> context, Debugger<Field, Fact, Stmt, Method> debugger)
		{
			this.method = method;
			this.context = context;
			this.debugger = debugger;
		}

		public virtual void addIncomingEdge(CallEdge<Field, Fact, Stmt, Method> incEdge)
		{
			WrappedFact<Field, Fact, Stmt, Method> calleeSourceFact = incEdge.CalleeSourceFact;
			PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer = perSourceAnalyzer.getOrCreate(calleeSourceFact.Fact);
			analyzer.addIncomingEdge(incEdge);
		}

		public virtual void addInitialSeed(Stmt startPoint, Fact val)
		{
			perSourceAnalyzer.getOrCreate(val).addInitialSeed(startPoint);
		}

		public virtual void addUnbalancedReturnFlow(WrappedFactAtStatement<Field, Fact, Stmt, Method> target, Stmt callSite)
		{
			perSourceAnalyzer.getOrCreate(context.zeroValue).scheduleUnbalancedReturnEdgeTo(target);
		}
	}

}