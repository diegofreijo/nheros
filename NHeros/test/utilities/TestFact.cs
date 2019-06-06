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
	public class TestFact
	{

		public readonly string baseValue;

		public TestFact(string baseValue)
		{
			this.baseValue = baseValue;
		}

		public override string ToString()
		{
			return "[Fact " + baseValue + "]";
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((string.ReferenceEquals(baseValue, null)) ? 0 : baseValue.GetHashCode());
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
			if (!(obj is TestFact))
			{
				return false;
			}
			TestFact other = (TestFact) obj;
			if (string.ReferenceEquals(baseValue, null))
			{
				if (!string.ReferenceEquals(other.baseValue, null))
				{
					return false;
				}
			}
			else if (!baseValue.Equals(other.baseValue))
			{
				return false;
			}
			return true;
		}
	}

}