﻿using NHeros.src.util;
using System.Collections.Generic;
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
	public class WrappedFact<Field, TFact, Stmt, Method>
	{
		private readonly TFact fact;
		private readonly AccessPath<Field> accessPath;
		private readonly Resolver<Field, TFact, Stmt, Method> resolver;

		public WrappedFact(TFact fact, AccessPath<Field> accessPath, Resolver<Field, TFact, Stmt, Method> resolver)
		{
			Debug.Assert(!Utils.IsDefault(fact));
			Debug.Assert(accessPath != null);
			Debug.Assert(resolver != null);

			this.fact = fact;
			this.accessPath = accessPath;
			this.resolver = resolver;
		}

        public virtual TFact Fact => fact;
        public virtual AccessPath<Field> AccessPath => accessPath;
        public virtual Resolver<Field, TFact, Stmt, Method> Resolver => resolver;

        public virtual WrappedFact<Field, TFact, Stmt, Method> applyDelta(AccessPath<Field>.Delta<Field> delta)
		{
			return new WrappedFact<Field, TFact, Stmt, Method>(fact, delta.applyTo(accessPath), resolver); //TODO keep resolver?
		}


        public virtual WrappedFact<Field, TFact, Stmt, Method> applyConstraint(FlowFunction_Constraint<Field> constraint, TFact zeroValue)
		{
			if (fact.Equals(zeroValue))
			{
				return this;
			}
			else
			{
				return new WrappedFact<Field, TFact, Stmt, Method>(fact, constraint.applyToAccessPath(accessPath), resolver);
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
			result = prime * result + (Utils.IsDefault(fact) ? 0 : fact.GetHashCode());
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
			var other = (WrappedFact<Field, TFact, Stmt, Method>) obj;
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


    }

}