using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2014 Johannes Lerch, Johannes Spaeth.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch, Johannes Spaeth - initial API and implementation
/// *****************************************************************************
/// </summary>

namespace heros.fieldsens
{
	using DefaultValueMap = heros.utilities.DefaultValueMap;


	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class FieldSensitiveIFDSSolver<FieldRef, D, N, M, I> where I : heros.InterproceduralCFG<N, M>
	{

		protected internal static readonly Logger logger = LoggerFactory.getLogger(typeof(FieldSensitiveIFDSSolver));

		private DefaultValueMap<M, MethodAnalyzer<FieldRef, D, N, M>> methodAnalyzers = new DefaultValueMapAnonymousInnerClass();

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<M, MethodAnalyzer<FieldRef, D, N, M>>
		{
			protected internal override MethodAnalyzer<FieldRef, D, N, M> createItem(M key)
			{
				return outerInstance.createMethodAnalyzer(key);
			}
		}

		private IFDSTabulationProblem<N, FieldRef, D, M, I> tabulationProblem;
		protected internal Context<FieldRef, D, N, M> context;
		protected internal Debugger<FieldRef, D, N, M> debugger;
		private Scheduler scheduler;

		public FieldSensitiveIFDSSolver(IFDSTabulationProblem<N, FieldRef, D, M, I> tabulationProblem, FactMergeHandler<D> factHandler, Debugger<FieldRef, D, N, M> debugger, Scheduler scheduler)
		{
			this.tabulationProblem = tabulationProblem;
			this.scheduler = scheduler;
			this.debugger = debugger == null ? new Debugger_NullDebugger<FieldRef, D, N, M>() : debugger;
			this.debugger.ICFG = tabulationProblem.interproceduralCFG();
			context = initContext(tabulationProblem, factHandler);
			submitInitialSeeds();
		}

		private Context<FieldRef, D, N, M> initContext(IFDSTabulationProblem<N, FieldRef, D, M, I> tabulationProblem, FactMergeHandler<D> factHandler)
		{
			 return new ContextAnonymousInnerClass(this, tabulationProblem, scheduler, factHandler);
		}

		private class ContextAnonymousInnerClass : Context<FieldRef, D, N, M>
		{
			private readonly FieldSensitiveIFDSSolver<FieldRef, D, N, M, I> outerInstance;

			public ContextAnonymousInnerClass(FieldSensitiveIFDSSolver<FieldRef, D, N, M, I> outerInstance, heros.fieldsens.IFDSTabulationProblem<N, FieldRef, D, M, I> tabulationProblem, heros.fieldsens.Scheduler scheduler, heros.fieldsens.FactMergeHandler<D> factHandler) : base(tabulationProblem, scheduler, factHandler)
			{
				this.outerInstance = outerInstance;
			}

			public override MethodAnalyzer<FieldRef, D, N, M> getAnalyzer(M method)
			{
				if (method == default(M))
				{
					throw new System.ArgumentException("Method must be not null");
				}
				return methodAnalyzers.getOrCreate(method);
			}
		}

		protected internal virtual MethodAnalyzer<FieldRef, D, N, M> createMethodAnalyzer(M method)
		{
			return new MethodAnalyzerImpl<FieldRef, D, N, M>(method, context, debugger);
		}

		/// <summary>
		/// Schedules the processing of initial seeds, initiating the analysis.
		/// </summary>
		private void submitInitialSeeds()
		{
			foreach (KeyValuePair<N, ISet<D>> seed in tabulationProblem.initialSeeds().SetOfKeyValuePairs())
			{
				N startPoint = seed.Key;
				MethodAnalyzer<FieldRef, D, N, M> analyzer = methodAnalyzers.getOrCreate(tabulationProblem.interproceduralCFG().getMethodOf(startPoint));
				foreach (D val in seed.Value)
				{
					analyzer.addInitialSeed(startPoint, val);
					debugger.initialSeed(startPoint);
				}
			}
		}
	}

}