using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace heros.fieldsens
{

	using Function = com.google.common.@base.Function;
	
	using Lists = com.google.common.collect.Lists;
	using Sets = com.google.common.collect.Sets;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AccessPath<T>
	public class AccessPath<T>
	{

		public static AccessPath<T> empty<T>()
		{
			return new AccessPath<T>();
		}

		private readonly T[] accesses;
		private readonly ISet<T> exclusions;

		public AccessPath()
		{
			accesses = (T[]) new object[0];
			exclusions = Sets.newHashSet();
		}

		internal AccessPath(T[] accesses, ISet<T> exclusions)
		{
			this.accesses = accesses;
			this.exclusions = exclusions;
		}

		public virtual bool isAccessInExclusions(T fieldReference)
		{
			return exclusions.Contains(fieldReference);
		}

		public virtual bool hasAllExclusionsOf(AccessPath<T> accPath)
		{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			return exclusions.containsAll(accPath.exclusions);
		}

		public virtual AccessPath<T> append(params T[] fieldReferences)
		{
			if (fieldReferences.Length == 0)
			{
				return this;
			}

			if (isAccessInExclusions(fieldReferences[0]))
			{
				throw new System.ArgumentException("FieldRef " + Arrays.ToString(fieldReferences) + " cannot be added to " + ToString());
			}

			T[] newAccesses = Arrays.copyOf(accesses, accesses.Length + fieldReferences.Length);
			Array.Copy(fieldReferences, 0, newAccesses, accesses.Length, fieldReferences.Length);
			return new AccessPath<T>(newAccesses, Sets.newHashSet<T>());
		}

		public virtual AccessPath<T> prepend(T fieldRef)
		{
			T[] newAccesses = (T[]) new object[accesses.Length + 1];
			newAccesses[0] = fieldRef;
			Array.Copy(accesses, 0, newAccesses, 1, accesses.Length);
			return new AccessPath<T>(newAccesses, exclusions);
		}

		public virtual AccessPath<T> removeFirst()
		{
			T[] newAccesses = (T[]) new object[accesses.Length - 1];
			Array.Copy(accesses, 1, newAccesses, 0, accesses.Length - 1);
			return new AccessPath<T>(newAccesses, exclusions);
		}

		public virtual AccessPath<T> appendExcludedFieldReference(ICollection<T> fieldReferences)
		{
			HashSet<T> newExclusions = Sets.newHashSet(fieldReferences);
			newExclusions.addAll(exclusions);
			return new AccessPath<T>(accesses, newExclusions);
		}

		public virtual AccessPath<T> appendExcludedFieldReference(params T[] fieldReferences)
		{
			HashSet<T> newExclusions = Sets.newHashSet(fieldReferences);
			newExclusions.addAll(exclusions);
			return new AccessPath<T>(accesses, newExclusions);
		}

		public sealed class PrefixTestResult
		{
			public static readonly PrefixTestResult GUARANTEED_PREFIX = new PrefixTestResult("GUARANTEED_PREFIX", InnerEnum.GUARANTEED_PREFIX, 2);
			public static readonly PrefixTestResult POTENTIAL_PREFIX = new PrefixTestResult("POTENTIAL_PREFIX", InnerEnum.POTENTIAL_PREFIX, 1);
			public static readonly PrefixTestResult NO_PREFIX = new PrefixTestResult("NO_PREFIX", InnerEnum.NO_PREFIX, 0);

			private static readonly IList<PrefixTestResult> valueList = new List<PrefixTestResult>();

			static PrefixTestResult()
			{
				valueList.Add(GUARANTEED_PREFIX);
				valueList.Add(POTENTIAL_PREFIX);
				valueList.Add(NO_PREFIX);
			}

			public enum InnerEnum
			{
				GUARANTEED_PREFIX,
				POTENTIAL_PREFIX,
				NO_PREFIX
			}

			public readonly InnerEnum innerEnumValue;
			private readonly string nameValue;
			private readonly int ordinalValue;
			private static int nextOrdinal = 0;

			internal int value;

			internal PrefixTestResult(string name, InnerEnum innerEnum, int value)
			{
				this.value = value;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			public bool atLeast(PrefixTestResult minimum)
			{
				return value >= minimum.value;
			}

			public static IList<PrefixTestResult> values()
			{
				return valueList;
			}

			public int ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static PrefixTestResult valueOf(string name)
			{
				foreach (PrefixTestResult enumInstance in PrefixTestResult.valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}

		public virtual PrefixTestResult isPrefixOf(AccessPath<T> accessPath)
		{
			if (accesses.Length > accessPath.accesses.Length)
			{
				return PrefixTestResult.NO_PREFIX;
			}

			for (int i = 0; i < accesses.Length; i++)
			{
				if (!accesses[i].Equals(accessPath.accesses[i]))
				{
					return PrefixTestResult.NO_PREFIX;
				}
			}

			if (accesses.Length < accessPath.accesses.Length)
			{
				if (exclusions.Contains(accessPath.accesses[accesses.Length]))
				{
					return PrefixTestResult.NO_PREFIX;
				}
				else
				{
					return PrefixTestResult.GUARANTEED_PREFIX;
				}
			}

			if (exclusions.Count == 0)
			{
				return PrefixTestResult.GUARANTEED_PREFIX;
			}
			if (accessPath.exclusions.Count == 0)
			{
				return PrefixTestResult.NO_PREFIX;
			}

			bool intersection = !Sets.intersection(exclusions, accessPath.exclusions).Empty;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			bool containsAll = exclusions.containsAll(accessPath.exclusions);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			bool oppositeContainsAll = accessPath.exclusions.containsAll(exclusions);
			bool potentialMatch = oppositeContainsAll || !intersection || (!containsAll && !oppositeContainsAll);
			if (potentialMatch)
			{
				if (oppositeContainsAll)
				{
					return PrefixTestResult.GUARANTEED_PREFIX;
				}
				else
				{
					return PrefixTestResult.POTENTIAL_PREFIX;
				}
			}
			return PrefixTestResult.NO_PREFIX;
		}

		public virtual Delta<T> getDeltaTo(AccessPath<T> accPath)
		{
			Debug.Assert(isPrefixOf(accPath).atLeast(PrefixTestResult.POTENTIAL_PREFIX));
			HashSet<T> mergedExclusions = Sets.newHashSet(accPath.exclusions);
			if (accesses.Length == accPath.accesses.Length)
			{
				mergedExclusions.addAll(exclusions);
			}
			Delta<T> delta = new Delta<T>(Arrays.copyOfRange(accPath.accesses, accesses.Length, accPath.accesses.Length), mergedExclusions);
			assert(isPrefixOf(accPath).atLeast(PrefixTestResult.POTENTIAL_PREFIX) && accPath.isPrefixOf(delta.applyTo(this)) == PrefixTestResult.GUARANTEED_PREFIX) || (isPrefixOf(accPath) == PrefixTestResult.GUARANTEED_PREFIX && accPath.Equals(delta.applyTo(this)));
			return delta;
		}

		public class Delta<T>
		{
			internal readonly T[] accesses;
			internal readonly ISet<T> exclusions;

			protected internal Delta(T[] accesses, ISet<T> exclusions)
			{
				this.accesses = accesses;
				this.exclusions = exclusions;
			}

			public virtual bool canBeAppliedTo(AccessPath<T> accPath)
			{
				if (accesses.Length > 0)
				{
					return !accPath.isAccessInExclusions(accesses[0]);
				}
				else
				{
					return true;
				}
			}

			public virtual AccessPath<T> applyTo(AccessPath<T> accPath)
			{
				return accPath.append(accesses).appendExcludedFieldReference(exclusions);
			}

			public override string ToString()
			{
				string result = accesses.Length > 0 ? "." + Joiner.on(".").join(accesses) : "";
				if (exclusions.Count > 0)
				{
					result += "^" + Joiner.on(",").join(exclusions);
				}
				return result;
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + Arrays.GetHashCode(accesses);
				result = prime * result + ((exclusions == null) ? 0 : exclusions.GetHashCode());
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
				Delta other = (Delta) obj;
				if (!Arrays.Equals(accesses, other.accesses))
				{
					return false;
				}
				if (exclusions == null)
				{
					if (other.exclusions != null)
					{
						return false;
					}
				}
				else if (!exclusions.SetEquals(other.exclusions))
				{
					return false;
				}
				return true;
			}

			public static Delta<T> empty<T>()
			{
				return new Delta<T>((T[]) new object[0], Sets.newHashSet<T>());
			}
		}

		public virtual AccessPath<T> mergeExcludedFieldReferences(AccessPath<T> accPath)
		{
			HashSet<T> newExclusions = Sets.newHashSet(exclusions);
			newExclusions.addAll(accPath.exclusions);
			return new AccessPath<T>(accesses, newExclusions);
		}

		public virtual bool canRead(T field)
		{
			return accesses.Length > 0 && accesses[0].Equals(field);
		}

		public virtual bool Empty
		{
			get
			{
				return exclusions.Count == 0 && accesses.Length == 0;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + Arrays.GetHashCode(accesses);
			result = prime * result + ((exclusions == null) ? 0 : exclusions.GetHashCode());
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
			AccessPath other = (AccessPath) obj;
			if (!Arrays.Equals(accesses, other.accesses))
			{
				return false;
			}
			if (exclusions == null)
			{
				if (other.exclusions != null)
				{
					return false;
				}
			}
			else if (!exclusions.SetEquals(other.exclusions))
			{
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			string result = accesses.Length > 0 ? "." + Joiner.on(".").join(accesses) : "";
			if (exclusions.Count > 0)
			{
				result += "^" + Joiner.on(",").join(exclusions);
			}
			return result;
		}

		public virtual AccessPath<T> removeAnyAccess()
		{
			if (accesses.Length > 0)
			{
				return new AccessPath<T>((T[]) new object[0], exclusions);
			}
			else
			{
				return this;
			}
		}

		public virtual bool hasEmptyAccessPath()
		{
			return accesses.Length == 0;
		}

		public virtual T FirstAccess
		{
			get
			{
				return accesses[0];
			}
		}

		internal virtual ISet<T> Exclusions
		{
			get
			{
				return exclusions;
			}
		}
	}

}