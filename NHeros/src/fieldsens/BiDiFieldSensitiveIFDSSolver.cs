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

	using Synchronizer = heros.fieldsens.SourceStmtAnnotatedMethodAnalyzer.Synchronizer;

	using HashMultimap = com.google.common.collect.HashMultimap;
	using Sets = com.google.common.collect.Sets;


	public class BiDiFieldSensitiveIFDSSolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, I> where I : heros.InterproceduralCFG<Stmt, System.Reflection.MethodInfo>
	{

		private FieldSensitiveIFDSSolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, I> forwardSolver;
		private FieldSensitiveIFDSSolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, I> backwardSolver;
		private Scheduler scheduler;
		private SynchronizerImpl<Stmt> forwardSynchronizer;
		private SynchronizerImpl<Stmt> backwardSynchronizer;

		public BiDiFieldSensitiveIFDSSolver(IFDSTabulationProblem<Stmt, System.Reflection.FieldInfo, Fact, System.Reflection.MethodInfo, I> forwardProblem, IFDSTabulationProblem<Stmt, System.Reflection.FieldInfo, Fact, System.Reflection.MethodInfo, I> backwardProblem, FactMergeHandler<Fact> factHandler, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger, Scheduler scheduler)
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
//ORIGINAL LINE: private FieldSensitiveIFDSSolver<Field, Fact, Stmt, Method, I> createSolver(IFDSTabulationProblem<Stmt, Field, Fact, Method, I> problem, FactMergeHandler<Fact> factHandler, Debugger<Field, Fact, Stmt, Method> debugger, final SynchronizerImpl<Stmt> synchronizer)
		private FieldSensitiveIFDSSolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, I> createSolver(IFDSTabulationProblem<Stmt, System.Reflection.FieldInfo, Fact, System.Reflection.MethodInfo, I> problem, FactMergeHandler<Fact> factHandler, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger, SynchronizerImpl<Stmt> synchronizer)
		{
			return new FieldSensitiveIFDSSolverAnonymousInnerClass(this, problem, factHandler, debugger, scheduler, synchronizer);
		}

		private class FieldSensitiveIFDSSolverAnonymousInnerClass : FieldSensitiveIFDSSolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, I>
		{
			private readonly BiDiFieldSensitiveIFDSSolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, I> outerInstance;

			private new heros.fieldsens.Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger;
			private heros.fieldsens.BiDiFieldSensitiveIFDSSolver.SynchronizerImpl<Stmt> synchronizer;

			public FieldSensitiveIFDSSolverAnonymousInnerClass(BiDiFieldSensitiveIFDSSolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, I> outerInstance, heros.fieldsens.IFDSTabulationProblem<Stmt, System.Reflection.FieldInfo, Fact, System.Reflection.MethodInfo, I> problem, heros.fieldsens.FactMergeHandler<Fact> factHandler, heros.fieldsens.Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger, heros.fieldsens.Scheduler scheduler, heros.fieldsens.BiDiFieldSensitiveIFDSSolver.SynchronizerImpl<Stmt> synchronizer) : base(problem, factHandler, debugger, scheduler)
			{
				this.outerInstance = outerInstance;
				this.debugger = debugger;
				this.synchronizer = synchronizer;
			}

			protected internal override MethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> createMethodAnalyzer(System.Reflection.MethodInfo method)
			{
				return new SourceStmtAnnotatedMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(method, context, synchronizer, debugger);
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