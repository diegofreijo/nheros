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
	public class Statement
	{

		public readonly string identifier;

		public Statement(string identifier)
		{
			this.identifier = identifier;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((string.ReferenceEquals(identifier, null)) ? 0 : identifier.GetHashCode());
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
			if (!(obj is Statement))
			{
				return false;
			}
			Statement other = (Statement) obj;
			if (string.ReferenceEquals(identifier, null))
			{
				if (!string.ReferenceEquals(other.identifier, null))
				{
					return false;
				}
			}
			else if (!identifier.Equals(other.identifier))
			{
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			return "[Statement " + identifier + "]";
		}
	}

}