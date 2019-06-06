using NHeros.src.util;
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
namespace heros.solver
{
	//copied from soot.toolkits.scalar
	public class Pair<T, U>
	{
		protected internal T o1;
		protected internal U o2;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int hashCode_Conflict = 0;

		public Pair()
		{
			o1 = default(T);
			o2 = default(U);
		}

		public Pair(T o1, U o2)
		{
			this.o1 = o1;
			this.o2 = o2;
		}

		public override int GetHashCode()
		{
			if (hashCode_Conflict != 0)
			{
				return hashCode_Conflict;
			}

			const int prime = 31;
			int result = 1;
			result = prime * result + (Utils.IsDefault((o1)) ? 0 : o1.GetHashCode());
			result = prime * result + (Utils.IsDefault((o2)) ? 0 : o2.GetHashCode());
			hashCode_Conflict = result;

			return hashCode_Conflict;
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
            Pair<T, U> other = (Pair<T, U>)obj;
			if (Utils.IsDefault(o1))
			{
				if (!Utils.IsDefault(other.o1))
				{
					return false;
				}
			}
			else if (!o1.Equals(other.o1))
			{
				return false;
			}
			if (Utils.IsDefault(o2))
			{
				if (!Utils.IsDefault(other.o2))
				{
					return false;
				}
			}
			else if (!o2.Equals(other.o2))
			{
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			return "Pair " + o1 + "," + o2;
		}

		public virtual T O1
		{
			get
			{
				return o1;
			}
			set
			{
				o1 = value;
				hashCode_Conflict = 0;
			}
		}

		public virtual U O2
		{
			get
			{
				return o2;
			}
			set
			{
				o2 = value;
				hashCode_Conflict = 0;
			}
		}



		public virtual void setPair(T no1, U no2)
		{
			o1 = no1;
			o2 = no2;
			hashCode_Conflict = 0;
		}

	}

}