using heros.fieldsens.structs;
using heros.utilities;
using NHeros.src.util;
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
	public class SourceStmtAnnotatedMethodAnalyzer<Field, Fact, Stmt, Method> : MethodAnalyzer<Field, Fact, Stmt, Method>
	{
		private readonly Method method;
        private DefaultValueMap<Key<Fact, Stmt>, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>> perSourceAnalyzer;

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<Key<Fact, Stmt>, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>>
		{
            private SourceStmtAnnotatedMethodAnalyzer<Field, Fact, Stmt, Method> outerInstance;

            public DefaultValueMapAnonymousInnerClass(SourceStmtAnnotatedMethodAnalyzer<Field, Fact, Stmt, Method> outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            protected internal override PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> createItem(Key<Fact, Stmt> key)
			{
				return new PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>(outerInstance.method, key.fact, outerInstance.context, outerInstance.debugger);
			}
		}

		private Context<Field, Fact, Stmt, Method> context;
		private Synchronizer<Stmt> synchronizer;
		private Debugger<Field, Fact, Stmt, Method> debugger;

		public SourceStmtAnnotatedMethodAnalyzer(Method method, Context<Field, Fact, Stmt, Method> context, Synchronizer<Stmt> synchronizer, Debugger<Field, Fact, Stmt, Method> debugger)
		{
			this.method = method;
			this.context = context;
			this.synchronizer = synchronizer;
			this.debugger = debugger;
            this.perSourceAnalyzer = new DefaultValueMapAnonymousInnerClass(this);
        }

		public virtual void addIncomingEdge(CallEdge<Field, Fact, Stmt, Method> incEdge)
		{
			var calleeSourceFact = incEdge.CalleeSourceFact;
			Key<Fact, Stmt> key = new Key<Fact, Stmt>(calleeSourceFact.Fact, default);
			PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer = perSourceAnalyzer.getOrCreate(key);
			analyzer.addIncomingEdge(incEdge);
		}

		public virtual void addInitialSeed(Stmt startPoint, Fact val)
		{
			Key<Fact, Stmt> key = new Key<Fact, Stmt>(val, startPoint);
			perSourceAnalyzer.getOrCreate(key).addInitialSeed(startPoint);
		}

        public virtual void addUnbalancedReturnFlow(WrappedFactAtStatement<Field, Fact, Stmt, Method> target, Stmt callSite)
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
				result = prime * result + (Utils.IsDefault(fact) ? 0 : fact.GetHashCode());
				result = prime * result + (Utils.IsDefault(stmt) ? 0 : stmt.GetHashCode());
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

				var other = obj as Key<Fact, Stmt>;
				if (Utils.IsDefault(fact))
				{
					if (!Utils.IsDefault(other.fact))
					{
						return false;
					}
				}
				else if (!fact.Equals(other.fact))
				{
					return false;
				}
                if (Utils.IsDefault(stmt))
                {
                    if (!Utils.IsDefault(other.stmt))
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