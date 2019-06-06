using heros.solver;
using JoinHandlingNode_JoinKey = heros.solver.JoinHandlingNode_JoinKey;

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
namespace heros.utilities
{
	public class JoinableFact : JoinHandlingNode<JoinableFact>
	{
		public readonly string name;

		public JoinableFact(string name)
		{
			this.name = name;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((string.ReferenceEquals(name, null)) ? 0 : name.GetHashCode());
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
			if (!(obj is JoinableFact))
			{
				return false;
			}
			JoinableFact other = (JoinableFact) obj;
			if (string.ReferenceEquals(name, null))
			{
				if (!string.ReferenceEquals(other.name, null))
				{
					return false;
				}
			}
			else if (!name.Equals(other.name))
			{
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			return "[Fact " + name + "]";
		}

		public virtual JoinableFact CallingContext
		{
			set
			{
    
			}
		}

		public virtual JoinHandlingNode_JoinKey createJoinKey()
		{
			return new TestJoinKey(this);
		}

		public virtual bool handleJoin(JoinableFact joiningNode)
		{
			return true;
		}

		private class TestJoinKey : JoinHandlingNode_JoinKey
		{
			private readonly JoinableFact outerInstance;

			public TestJoinKey(JoinableFact outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			internal virtual JoinableFact Fact
			{
				get
				{
					return outerInstance;
				}
			}

			public override bool Equals(object obj)
			{
				if (obj is TestJoinKey)
				{
					return Fact.Equals(((TestJoinKey) obj).Fact);
				}
				throw new System.ArgumentException();
			}

			public override int GetHashCode()
			{
				return outerInstance.GetHashCode();
			}
		}
	}

}