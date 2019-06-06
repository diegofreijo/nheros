﻿/// <summary>
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
	using Delta = heros.fieldsens.AccessPath.Delta;


	public class WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact;
		private Stmt stmt;

		public WrappedFactAtStatement(Stmt stmt, WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact)
		{
			this.stmt = stmt;
			this.fact = fact;
		}

		public virtual WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> WrappedFact
		{
			get
			{
				return fact;
			}
		}

		public virtual Fact Fact
		{
			get
			{
				return fact.Fact;
			}
		}

		public virtual AccessPath<System.Reflection.FieldInfo> AccessPath
		{
			get
			{
				return fact.AccessPath;
			}
		}

		public virtual Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> Resolver
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

		public virtual FactAtStatement<Fact, Stmt> AsFactAtStatement
		{
			get
			{
				return new FactAtStatement<Fact, Stmt>(fact.Fact, stmt);
			}
		}

		public virtual bool canDeltaBeApplied(AccessPath.Delta<System.Reflection.FieldInfo> delta)
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
			WrappedFactAtStatement other = (WrappedFactAtStatement) obj;
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