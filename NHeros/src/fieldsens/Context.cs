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

	public abstract class Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		public readonly InterproceduralCFG<Stmt, System.Reflection.MethodInfo> icfg;
		public readonly Scheduler scheduler;
		public readonly Fact zeroValue;
		public readonly bool followReturnsPastSeeds;
		public readonly FactMergeHandler<Fact> factHandler;
		public readonly ZeroHandler<System.Reflection.FieldInfo> zeroHandler;
		public readonly FlowFunctions<Stmt, System.Reflection.FieldInfo, Fact, System.Reflection.MethodInfo> flowFunctions;

		internal Context<T1>(IFDSTabulationProblem<T1> tabulationProblem, Scheduler scheduler, FactMergeHandler<Fact> factHandler) where T1 : heros.InterproceduralCFG<Stmt, System.Reflection.MethodInfo>
		{
			this.icfg = tabulationProblem.interproceduralCFG();
			this.flowFunctions = tabulationProblem.flowFunctions();
			this.scheduler = scheduler;
			this.zeroValue = tabulationProblem.zeroValue();
			this.followReturnsPastSeeds = tabulationProblem.followReturnsPastSeeds();
			this.factHandler = factHandler;
			this.zeroHandler = tabulationProblem.zeroHandler();
		}

		public abstract MethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> getAnalyzer(System.Reflection.MethodInfo method);
	}

}