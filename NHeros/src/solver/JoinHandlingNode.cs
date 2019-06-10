/// <summary>
///*****************************************************************************
/// Copyright (c) 2014 Johannes Lerch.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.solver
{
    public interface JoinHandlingNode<T>
	{
		/// <param name="joiningNode"> the node abstraction that was propagated to the same target after {@code this} node. </param>
		/// <returns> true if the join could be handled and no further propagation of the {@code joiningNode} is necessary, otherwise false meaning 
		/// the node should be propagated by the solver. </returns>
		bool handleJoin(T joiningNode);

		/// <returns> a JoinKey object used to identify which node abstractions require manual join handling. 
		/// For nodes with {@code equal} JoinKey instances <seealso cref="handleJoin(JoinHandlingNode)"/> will be called. </returns>
		JoinHandlingNode_JoinKey createJoinKey();

		T CallingContext {set;}
	}

	public class JoinHandlingNode_JoinKey
	{
		internal object[] elements;

		/// 
		/// <param name="elements"> Passed elements must be immutable with respect to their hashCode and equals implementations. </param>
		public JoinHandlingNode_JoinKey(params object[] elements)
		{
			this.elements = elements;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + elements.GetHashCode();
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
			JoinHandlingNode_JoinKey other = (JoinHandlingNode_JoinKey) obj;
			if (elements.Equals(other.elements))
			{
				return false;
			}
			return true;
		}
	}

}