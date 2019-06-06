using System.Text;

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

	/// <summary>
	/// A path edge as described in the IFDS/IDE algorithms.
	/// The source node is implicit: it can be computed from the target by using the <seealso cref="InterproceduralCFG"/>.
	/// Hence, we don't store it.
	/// </summary>
	/// @param <N> The type of nodes in the interprocedural control-flow graph. Typically <seealso cref="Unit"/>. </param>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	public class PathEdge<N, D>
	{

		protected internal readonly N target;
		protected internal readonly D dSource, dTarget;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly int hashCode_Conflict;

		/// <param name="dSource"> The fact at the source. </param>
		/// <param name="target"> The target statement. </param>
		/// <param name="dTarget"> The fact at the target. </param>
		public PathEdge(D dSource, N target, D dTarget) : base()
		{
			this.target = target;
			this.dSource = dSource;
			this.dTarget = dTarget;

			const int prime = 31;
			int result = 1;
			result = prime * result + ((dSource == default(D)) ? 0 : dSource.GetHashCode());
			result = prime * result + ((dTarget == default(D)) ? 0 : dTarget.GetHashCode());
			result = prime * result + ((target == default(N)) ? 0 : target.GetHashCode());
			this.hashCode_Conflict = result;
		}

		public virtual N Target
		{
			get
			{
				return target;
			}
		}

		public virtual D factAtSource()
		{
			return dSource;
		}

		public virtual D factAtTarget()
		{
			return dTarget;
		}

		public override int GetHashCode()
		{
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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") PathEdge other = (PathEdge) obj;
			PathEdge other = (PathEdge) obj;
			if (dSource == default(D))
			{
				if (other.dSource != default(D))
				{
					return false;
				}
			}
			else if (!dSource.Equals(other.dSource))
			{
				return false;
			}
			if (dTarget == default(D))
			{
				if (other.dTarget != default(D))
				{
					return false;
				}
			}
			else if (!dTarget.Equals(other.dTarget))
			{
				return false;
			}
			if (target == default(N))
			{
				if (other.target != default(N))
				{
					return false;
				}
			}
			else if (!target.Equals(other.target))
			{
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			result.Append("<");
			result.Append(dSource);
			result.Append("> -> <");
			result.Append(target.ToString());
			result.Append(",");
			result.Append(dTarget);
			result.Append(">");
			return result.ToString();
		}

	}

}