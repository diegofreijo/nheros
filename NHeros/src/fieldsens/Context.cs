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
	public abstract class Context<Field, Fact, Stmt, Method>
	{
		public readonly InterproceduralCFG<Stmt, Method> icfg;
		public readonly Scheduler scheduler;
		public readonly Fact zeroValue;
		public readonly bool followReturnsPastSeeds;
		public readonly FactMergeHandler<Fact> factHandler;
		public readonly ZeroHandler<Field> zeroHandler;
		public readonly FlowFunctions<Stmt, Field, Fact, Method> flowFunctions;

		internal Context(IFDSTabulationProblem<InterproceduralCFG<Stmt, Method>> tabulationProblem, Scheduler scheduler, FactMergeHandler<Fact> factHandler) 
		{
			this.icfg = tabulationProblem.interproceduralCFG();
			this.flowFunctions = tabulationProblem.flowFunctions();
			this.scheduler = scheduler;
			this.zeroValue = tabulationProblem.zeroValue();
			this.followReturnsPastSeeds = tabulationProblem.followReturnsPastSeeds();
			this.factHandler = factHandler;
			this.zeroHandler = tabulationProblem.zeroHandler();
		}

		public abstract MethodAnalyzer<Field, Fact, Stmt, Method> getAnalyzer(Method method);
	}

}