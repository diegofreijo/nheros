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

	using Maps = com.google.common.collect.Maps;

	public class JsonDocument
	{

		private DefaultValueMap<string, JsonDocument> documents = new DefaultValueMapAnonymousInnerClass();

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<string, JsonDocument>
		{
			protected internal override JsonDocument createItem(string key)
			{
				return new JsonDocument();
			}
		}
		private DefaultValueMap<string, JsonArray> arrays = new DefaultValueMapAnonymousInnerClass2();

		private class DefaultValueMapAnonymousInnerClass2 : DefaultValueMap<string, JsonArray>
		{
			protected internal override JsonArray createItem(string key)
			{
				return new JsonArray();
			}
		}
		private IDictionary<string, string> keyValuePairs = new Dictionary();

		public virtual JsonDocument doc(string key)
		{
			return documents.getOrCreate(key);
		}

		public virtual JsonDocument doc(string key, JsonDocument doc)
		{
			if (documents.containsKey(key))
			{
				throw new System.ArgumentException("There is already a document registered for key: " + key);
			}
			documents.put(key, doc);
			return doc;
		}

		public virtual JsonArray array(string key)
		{
			return arrays.getOrCreate(key);
		}

		public virtual void keyValue(string key, string value)
		{
			keyValuePairs[key] = value;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			write(builder, 0);
			return builder.ToString();
		}

		public virtual void write(StringBuilder builder, int tabs)
		{
			builder.Append("{\n");

			foreach (KeyValuePair<string, string> entry in keyValuePairs.SetOfKeyValuePairs())
			{
				tabs(tabs + 1, builder);
				builder.Append("\"" + entry.Key + "\": \"" + entry.Value + "\",\n");
			}

			foreach (KeyValuePair<string, JsonArray> entry in arrays.entrySet())
			{
				tabs(tabs + 1, builder);
				builder.Append("\"" + entry.Key + "\": ");
				entry.Value.write(builder, tabs + 1);
				builder.Append(",\n");
			}

			foreach (KeyValuePair<string, JsonDocument> entry in documents.entrySet())
			{
				tabs(tabs + 1, builder);
				builder.Append("\"" + entry.Key + "\": ");
				entry.Value.write(builder, tabs + 1);
				builder.Append(",\n");
			}

			if (keyValuePairs.Count > 0 || !arrays.Empty || !documents.Empty)
			{
				builder.Remove(builder.Length - 2, builder.Length - 1 - builder.Length - 2);
			}

			tabs(tabs, builder);
			builder.Append("}");
		}

		internal static void tabs(int tabs, StringBuilder builder)
		{
			for (int i = 0; i < tabs; i++)
			{
				builder.Append("\t");
			}
		}
	}
}