using System.Collections.Generic;
using System.Text;

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
namespace heros.utilities
{

	using Lists = com.google.common.collect.Lists;

	public class JsonArray
	{

		private IList<string> items = new List();

		public virtual void add(string item)
		{
			items.Add(item);
		}

		public virtual void write(StringBuilder builder, int tabs)
		{
			builder.Append("[\n");
			foreach (string item in items)
			{
				JsonDocument.tabs(tabs + 1, builder);
				builder.Append("\"" + item + "\",\n");
			}

			if (items.Count > 0)
			{
				builder.Remove(builder.Length - 2, builder.Length - 1 - builder.Length - 2);
			}

			JsonDocument.tabs(tabs, builder);
			builder.Append("]");
		}
	}
}