﻿using System.Threading;

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


	public class SourceStmtAnnotatedMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : MethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private System.Reflection.MethodInfo method;
		private DefaultValueMap<Key<Fact, Stmt>, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> perSourceAnalyzer = new DefaultValueMapAnonymousInnerClass();

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<Key<Fact, Stmt>, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>>
		{

			protected internal override PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> createItem(Key<Fact, Stmt> key)
			{
				return new PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(outerInstance.method, key.fact, outerInstance.context, outerInstance.debugger);
			}
		}
		private Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> context;
		private Synchronizer<Stmt> synchronizer;
		private Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger;

		public SourceStmtAnnotatedMethodAnalyzer(System.Reflection.MethodInfo method, Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> context, Synchronizer<Stmt> synchronizer, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger)
		{
			this.method = method;
			this.context = context;
			this.synchronizer = synchronizer;
			this.debugger = debugger;
		}

		public virtual void addIncomingEdge(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge)
		{
			WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> calleeSourceFact = incEdge.CalleeSourceFact;
			Key<Fact, Stmt> key = new Key<Fact, Stmt>(calleeSourceFact.Fact, null);
			PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer = perSourceAnalyzer.getOrCreate(key);
			analyzer.addIncomingEdge(incEdge);
		}

		public virtual void addInitialSeed(Stmt startPoint, Fact val)
		{
			Key<Fact, Stmt> key = new Key<Fact, Stmt>(val, startPoint);
			perSourceAnalyzer.getOrCreate(key).addInitialSeed(startPoint);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void addUnbalancedReturnFlow(final heros.fieldsens.structs.WrappedFactAtStatement<Field, Fact, Stmt, Method> target, final Stmt callSite)
		public virtual void addUnbalancedReturnFlow(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> target, Stmt callSite)
		{
			synchronizer.synchronizeOnStmt(callSite, () =>
			{
			Key<Fact, Stmt> key = new Key<Fact, Stmt>(context.zeroValue, callSite);
			perSourceAnalyzer.getOrCreate(key).scheduleUnbalancedReturnEdgeTo(target);
			});
		}

		public interface Synchronizer<Stmt>
		{
			void synchronizeOnStmt(Stmt stmt, ThreadStart job);
		}

		private class Key<Fact, Stmt>
		{
			internal Fact fact;
			internal Stmt stmt;

			internal Key(Fact fact, Stmt stmt)
			{
				this.fact = fact;
				this.stmt = stmt;
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + ((fact == default(Fact)) ? 0 : fact.GetHashCode());
				result = prime * result + ((stmt == default(Stmt)) ? 0 : stmt.GetHashCode());
				return result;
			}

			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (obj == null)
				{
					return false;
				}
				if (this.GetType() != obj.GetType())
				{
					return false;
				}
				Key other = (Key) obj;
				if (fact == default(Fact))
				{
					if (other.fact != default(Fact))
					{
						return false;
					}
				}
				else if (!fact.Equals(other.fact))
				{
					return false;
				}
				if (stmt == default(Stmt))
				{
					if (other.stmt != default(Stmt))
					{
						return false;
					}
				}
				else if (!stmt.Equals(other.stmt))
				{
					return false;
				}
				return true;
			}
		}
	}
}