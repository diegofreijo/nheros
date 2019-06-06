using heros.fieldsens.structs;
using NHeros.src.util;
using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2012 Eric Bodden.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Eric Bodden - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.fieldsens
{
	/// <summary>
	/// A flow function computes which of the finitely many D-type values are reachable
	/// from the current source values. Typically there will be one such function
	/// associated with every possible control flow. 
	/// 
	/// <b>NOTE:</b> To be able to produce <b>deterministic benchmarking results</b>, we have found that
	/// it helps to return <seealso cref="LinkedHashSet"/>s from <seealso cref="computeTargets(object)"/>. This is
	/// because the duration of IDE's fixed point iteration may depend on the iteration order.
	/// Within the solver, we have tried to fix this order as much as possible, but the
	/// order, in general, does also depend on the order in which the result set
	/// of <seealso cref="computeTargets(object)"/> is traversed.
	/// 
	/// <b>NOTE:</b> Methods defined on this type may be called simultaneously by different threads.
	/// Hence, classes implementing this interface should synchronize accesses to
	/// any mutable shared state.
	/// </summary>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	public interface FlowFunction<FieldRef, D, Stmt, Method>
	{

		/// <summary>
		/// Returns the target values reachable from the source.
		/// </summary>
		ISet<FlowFunction_ConstrainedFact<FieldRef, D, Stmt, Method>> computeTargets(D source, AccessPathHandler<FieldRef, D, Stmt, Method> accPathHandler);

	}

	public class FlowFunction_ConstrainedFact<FieldRef, D, Stmt, Method>
	{

		internal WrappedFact<FieldRef, D, Stmt, Method> fact;
		internal FlowFunction_Constraint<FieldRef> constraint;

		internal FlowFunction_ConstrainedFact(WrappedFact<FieldRef, D, Stmt, Method> fact)
		{
			this.fact = fact;
			this.constraint = null;
		}

		internal FlowFunction_ConstrainedFact(WrappedFact<FieldRef, D, Stmt, Method> fact, FlowFunction_Constraint<FieldRef> constraint)
		{
			this.fact = fact;
			this.constraint = constraint;
		}

		public virtual WrappedFact<FieldRef, D, Stmt, Method> Fact
		{
			get
			{
				return fact;
			}
		}

		public virtual FlowFunction_Constraint<FieldRef> Constraint
		{
			get
			{
				return constraint;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((constraint == null) ? 0 : constraint.GetHashCode());
			result = prime * result + ((fact == null) ? 0 : fact.GetHashCode());
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
			if (!(obj is FlowFunction_ConstrainedFact<FieldRef, D, Stmt, Method>))
			{
				return false;
			}
			var other = (FlowFunction_ConstrainedFact<FieldRef, D, Stmt, Method>) obj;
			if (constraint == null)
			{
				if (other.constraint != null)
				{
					return false;
				}
			}
			else if (!constraint.Equals(other.constraint))
			{
				return false;
			}
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
			return true;
		}

		public override string ToString()
		{
			return fact.ToString() + "<" + constraint + ">";
		}
	}

	public interface FlowFunction_Constraint<FieldRef>
	{
		AccessPath<FieldRef> applyToAccessPath(AccessPath<FieldRef> accPath);

		bool canBeAppliedTo(AccessPath<FieldRef> accPath);
	}

	public class FlowFunction_WriteFieldConstraint<FieldRef> : FlowFunction_Constraint<FieldRef>
	{
		internal FieldRef fieldRef;

		public FlowFunction_WriteFieldConstraint(FieldRef fieldRef)
		{
			this.fieldRef = fieldRef;
		}

		public virtual AccessPath<FieldRef> applyToAccessPath(AccessPath<FieldRef> accPath)
		{
			return accPath.appendExcludedFieldReference(fieldRef);
		}

		public override string ToString()
		{
			return "^" + fieldRef.ToString();
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + (Utils.IsDefault(fieldRef) ? 0 : fieldRef.GetHashCode());
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
			if (!(obj is FlowFunction_WriteFieldConstraint<FieldRef>))
			{
				return false;
			}
			var other = (FlowFunction_WriteFieldConstraint<FieldRef>) obj;
			if (Utils.IsDefault(fieldRef))
			{
				if (Utils.IsDefault(other.fieldRef))
				{
					return false;
				}
			}
			else if (!fieldRef.Equals(other.fieldRef))
			{
				return false;
			}
			return true;
		}

		public virtual bool canBeAppliedTo(AccessPath<FieldRef> accPath)
		{
			return true;
		}
	}

	public class FlowFunction_ReadFieldConstraint<FieldRef> : FlowFunction_Constraint<FieldRef>
	{

		internal FieldRef fieldRef;

		public FlowFunction_ReadFieldConstraint(FieldRef fieldRef)
		{
			this.fieldRef = fieldRef;
		}

		public virtual AccessPath<FieldRef> applyToAccessPath(AccessPath<FieldRef> accPath)
		{
			return accPath.append(fieldRef);
		}

		public override string ToString()
		{
			return fieldRef.ToString();
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + (Utils.IsDefault(fieldRef) ? 0 : fieldRef.GetHashCode());
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
			if (!(obj is FlowFunction_ReadFieldConstraint<FieldRef>))
			{
				return false;
			}
			var other = (FlowFunction_ReadFieldConstraint<FieldRef>) obj;
			if (Utils.IsDefault(fieldRef))
			{
				if (Utils.IsDefault(other.fieldRef))
				{
					return false;
				}
			}
			else if (!fieldRef.Equals(other.fieldRef))
			{
				return false;
			}
			return true;
		}

		public virtual bool canBeAppliedTo(AccessPath<FieldRef> accPath)
		{
			return !accPath.isAccessInExclusions(fieldRef);
		}
	}

}