using System.Diagnostics;

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
	using Delta = heros.fieldsens.AccessPath.Delta;

	public class WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private readonly Fact fact;
		private readonly AccessPath<System.Reflection.FieldInfo> accessPath;
		private readonly Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver;

		public WrappedFact(Fact fact, AccessPath<System.Reflection.FieldInfo> accessPath, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
		{
			Debug.Assert(fact != default(Fact));
			Debug.Assert(accessPath != null);
			Debug.Assert(resolver != null);

			this.fact = fact;
			this.accessPath = accessPath;
			this.resolver = resolver;
		}

		public virtual Fact Fact
		{
			get
			{
				return fact;
			}
		}

		public virtual WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> applyDelta(AccessPath.Delta<System.Reflection.FieldInfo> delta)
		{
			return new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, delta.applyTo(accessPath), resolver); //TODO keep resolver?
		}

		public virtual AccessPath<System.Reflection.FieldInfo> AccessPath
		{
			get
			{
				return accessPath;
			}
		}

		public virtual WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> applyConstraint(FlowFunction_Constraint<System.Reflection.FieldInfo> constraint, Fact zeroValue)
		{
			if (fact.Equals(zeroValue))
			{
				return this;
			}
			else
			{
				return new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, constraint.applyToAccessPath(accessPath), resolver);
			}
		}

		public override string ToString()
		{
			string result = fact.ToString() + accessPath;
			if (resolver != null)
			{
				result += resolver.ToString();
			}
			return result;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((accessPath == null) ? 0 : accessPath.GetHashCode());
			result = prime * result + ((fact == default(Fact)) ? 0 : fact.GetHashCode());
			result = prime * result + ((resolver == null) ? 0 : resolver.GetHashCode());
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
			WrappedFact other = (WrappedFact) obj;
			if (accessPath == null)
			{
				if (other.accessPath != null)
				{
					return false;
				}
			}
			else if (!accessPath.Equals(other.accessPath))
			{
				return false;
			}
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
			if (resolver == null)
			{
				if (other.resolver != null)
				{
					return false;
				}
			}
			else if (!resolver.Equals(other.resolver))
			{
				return false;
			}
			return true;
		}

		public virtual Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> Resolver
		{
			get
			{
				return resolver;
			}
		}


	}

}