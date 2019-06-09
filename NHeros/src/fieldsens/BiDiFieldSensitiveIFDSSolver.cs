using System.Collections.Generic;
using System.Threading;

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
	public class BiDiFieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> where I : heros.InterproceduralCFG<Stmt, Method>
	{

		private FieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> forwardSolver;
		private FieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> backwardSolver;
		private Scheduler scheduler;
		private SynchronizerImpl<Stmt> forwardSynchronizer;
		private SynchronizerImpl<Stmt> backwardSynchronizer;

		public BiDiFieldSensitiveIFDSSolver(IFDSTabulationProblem<Stmt, Field, Fact, Method, I> forwardProblem, IFDSTabulationProblem<Stmt, Field, Fact, Method, I> backwardProblem, FactMergeHandler factHandler, Debugger<Field, Fact, Stmt, Method> debugger, Scheduler scheduler)
		{

			this.scheduler = scheduler;

			forwardSynchronizer = new SynchronizerImpl<Stmt>();
			backwardSynchronizer = new SynchronizerImpl<Stmt>();
			forwardSynchronizer.otherSynchronizer = backwardSynchronizer;
			backwardSynchronizer.otherSynchronizer = forwardSynchronizer;

			forwardSolver = createSolver(forwardProblem, factHandler, debugger, forwardSynchronizer);
			backwardSolver = createSolver(backwardProblem, factHandler, debugger, backwardSynchronizer);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private FieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> createSolver(IFDSTabulationProblem<Stmt, Field, Fact, Method, I> problem, FactMergeHandler factHandler, Debugger<Field, Fact, Stmt, Method> debugger, final SynchronizerImpl<Stmt> synchronizer)
		private FieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> createSolver(IFDSTabulationProblem<Stmt, Field, Fact, Method, I> problem, FactMergeHandler factHandler, Debugger<Field, Fact, Stmt, Method> debugger, SynchronizerImpl<Stmt> synchronizer)
		{
			return new FieldSensitiveIFDSSolverAnonymousInnerClass(this, problem, factHandler, debugger, scheduler, synchronizer);
		}

		private class FieldSensitiveIFDSSolverAnonymousInnerClass : FieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I>
		{
			private readonly BiDiFieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> outerInstance;

			private new heros.fieldsens.Debugger<Field, Fact, Stmt, Method> debugger;
			private heros.fieldsens.BiDiFieldSensitiveIFDSSolver.SynchronizerImpl<Stmt> synchronizer;

			public FieldSensitiveIFDSSolverAnonymousInnerClass(BiDiFieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> outerInstance, heros.fieldsens.IFDSTabulationProblem<Stmt, Field, Fact, Method, I> problem, heros.fieldsens.FactMergeHandler factHandler, heros.fieldsens.Debugger<Field, Fact, Stmt, Method> debugger, heros.fieldsens.Scheduler scheduler, heros.fieldsens.BiDiFieldSensitiveIFDSSolver.SynchronizerImpl<Stmt> synchronizer) : base(problem, factHandler, debugger, scheduler)
			{
				this.outerInstance = outerInstance;
				this.debugger = debugger;
				this.synchronizer = synchronizer;
			}

			protected internal override MethodAnalyzer<Field, Fact, Stmt, Method> createMethodAnalyzer(Method method)
			{
				return new SourceStmtAnnotatedMethodAnalyzer<Field, Fact, Stmt, Method>(method, context, synchronizer, debugger);
			}
		}

		private class SynchronizerImpl<Stmt> : Synchronizer<Stmt>
		{

			internal SynchronizerImpl<Stmt> otherSynchronizer;
			internal ISet<Stmt> leakedSources = Sets.newHashSet();
			internal HashMultimap<Stmt, ThreadStart> pausedJobs = HashMultimap.create();

			public virtual void synchronizeOnStmt(Stmt stmt, ThreadStart job)
			{
				leakedSources.Add(stmt);
				if (otherSynchronizer.leakedSources.Contains(stmt))
				{
					job.run();
					foreach (ThreadStart runnable in otherSynchronizer.pausedJobs.get(stmt))
					{
						runnable.run();
					}
					otherSynchronizer.pausedJobs.removeAll(stmt);
				}
				else
				{
					pausedJobs.put(stmt, job);
				}
			}
		}
	}

}