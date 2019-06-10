using NHeros.src.util;
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
namespace heros.fieldsens.structs
{
	public class WrappedFactAtStatement<Field, TFact, Stmt, Method>
	{

		private WrappedFact<Field, TFact, Stmt, Method> fact;
		private Stmt stmt;

		public WrappedFactAtStatement(Stmt stmt, WrappedFact<Field, TFact, Stmt, Method> fact)
		{
			this.stmt = stmt;
			this.fact = fact;
		}

		public virtual WrappedFact<Field, TFact, Stmt, Method> WrappedFact
		{
			get
			{
				return fact;
			}
		}

		public virtual TFact Fact
		{
			get
			{
				return fact.Fact;
			}
		}

		public virtual AccessPath<Field> AccessPath
		{
			get
			{
				return fact.AccessPath;
			}
		}

		public virtual Resolver<Field, TFact, Stmt, Method> Resolver
		{
			get
			{
				return fact.Resolver;
			}
		}

		public virtual Stmt Statement
		{
			get
			{
				return stmt;
			}
		}

		public virtual FactAtStatement<TFact, Stmt> AsFactAtStatement
		{
			get
			{
				return new FactAtStatement<TFact, Stmt>(fact.Fact, stmt);
			}
		}

		public virtual bool canDeltaBeApplied(AccessPath<Field>.Delta<Field> delta)
		{
			return delta.canBeAppliedTo(fact.AccessPath);
		}

		public override string ToString()
		{
			return fact + " @ " + stmt;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((fact == null) ? 0 : fact.GetHashCode());
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

            var other = obj as WrappedFactAtStatement<Field, TFact, Stmt, Method>;
			if (fact == null)
			{
				if (other.fact != null)
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