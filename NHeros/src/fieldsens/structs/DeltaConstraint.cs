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

	public class DeltaConstraint<FieldRef> : FlowFunction_Constraint<FieldRef>
	{

		private AccessPath.Delta<FieldRef> delta;

		public DeltaConstraint(AccessPath<FieldRef> accPathAtCaller, AccessPath<FieldRef> accPathAtCallee)
		{
			delta = accPathAtCaller.getDeltaTo(accPathAtCallee);
		}

		public DeltaConstraint(AccessPath.Delta<FieldRef> delta)
		{
			this.delta = delta;
		}

		public virtual AccessPath<FieldRef> applyToAccessPath(AccessPath<FieldRef> accPath)
		{
			return delta.applyTo(accPath);
		}

		public virtual bool canBeAppliedTo(AccessPath<FieldRef> accPath)
		{
			return delta.canBeAppliedTo(accPath);
		}

		public override string ToString()
		{
			return delta.ToString();
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((delta == null) ? 0 : delta.GetHashCode());
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
			DeltaConstraint other = (DeltaConstraint) obj;
			if (delta == null)
			{
				if (other.delta != null)
				{
					return false;
				}
			}
			else if (!delta.Equals(other.delta))
			{
				return false;
			}
			return true;
		}


	}

}