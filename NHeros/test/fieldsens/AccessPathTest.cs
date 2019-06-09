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
	using static heros.fieldsens.AccessPath.PrefixTestResult;

//	import static org.junit.Assert.assertArrayEquals;

//	import static org.junit.Assert.assertEquals;

//	import static org.junit.Assert.assertFalse;

//	import static org.junit.Assert.assertTrue;


	using Delta = heros.fieldsens.AccessPath.Delta;
	using PrefixTestResult = heros.fieldsens.AccessPath.PrefixTestResult;

	using Test = org.junit.Test;

	using Sets = com.google.common.collect.Sets;

	public class AccessPathTest
	{

		public static AccessPath<string> ap(string ap)
		{
			Pattern pattern = Pattern.compile("(\\.|\\^)?([^\\.\\^]+)");
			Matcher matcher = pattern.matcher(ap);
			AccessPath<string> accessPath = new AccessPath<string>();
			bool addedExclusions = false;

			while (matcher.find())
			{
				string separator = matcher.group(1);
				string identifier = matcher.group(2);

				if (".".Equals(separator) || string.ReferenceEquals(separator, null))
				{
					if (addedExclusions)
					{
						throw new System.ArgumentException("Access path contains field references after exclusions.");
					}
					accessPath = accessPath.append(identifier);
				}
				else
				{
					addedExclusions = true;
					string[] excl = identifier.Split(",", true);
					accessPath = accessPath.appendExcludedFieldReference(excl);
				}
			}
			return accessPath;
		}


//ORIGINAL LINE: @Test public void append()
		public virtual void append()
		{
			AccessPath<string> sut = ap("a");
			assertEquals(ap("a.b"), sut.append("b"));
		}


//ORIGINAL LINE: @Test public void addOnExclusion()
		public virtual void addOnExclusion()
		{
			AccessPath<string> sut = ap("^a");
			assertEquals(ap("b"), sut.append("b"));
		}


//ORIGINAL LINE: @Test(expected=IllegalArgumentException.class) public void addMergedFieldsOnSingleExclusion()
		public virtual void addMergedFieldsOnSingleExclusion()
		{
			AccessPath<string> sut = ap("^a");
			sut.append("a");
		}


//ORIGINAL LINE: @Test public void prepend()
		public virtual void prepend()
		{
			assertEquals(ap("c.a.b"), ap("a.b").prepend("c"));
		}


//ORIGINAL LINE: @Test public void remove()
		public virtual void remove()
		{
			assertEquals(ap("b"), ap("a.b").removeFirst());
		}


//ORIGINAL LINE: @Test public void deltaDepth1()
		public virtual void deltaDepth1()
		{
			assertArrayEquals(new string[] {"b"}, ap("a").getDeltaTo(ap("a.b")).accesses);
		}


//ORIGINAL LINE: @Test public void deltaDepth2()
		public virtual void deltaDepth2()
		{
			assertArrayEquals(new string[] {"b", "c"}, ap("a").getDeltaTo(ap("a.b.c")).accesses);
		}


//ORIGINAL LINE: @Test public void deltaOnNonEmptyAccPathsWithExclusions()
		public virtual void deltaOnNonEmptyAccPathsWithExclusions()
		{
			Delta<string> delta = ap("a^f").getDeltaTo(ap("a.b^g"));
			assertArrayEquals(new object[] {"b"}, delta.accesses);
			assertEquals(Sets.newHashSet("g"), delta.exclusions);
		}


//ORIGINAL LINE: @Test public void deltaOnPotentialPrefix()
		public virtual void deltaOnPotentialPrefix()
		{
			assertEquals(Sets.newHashSet("f", "g"), ap("^f").getDeltaTo(ap("^g")).exclusions);
		}


//ORIGINAL LINE: @Test public void emptyDeltaOnEqualExclusions()
		public virtual void emptyDeltaOnEqualExclusions()
		{
			AccessPath<string> actual = ap("^f");
			object[] accesses = actual.getDeltaTo(ap("^f")).accesses;
			assertEquals(0, accesses.Length);
			assertTrue(actual.getDeltaTo(ap("^f")).exclusions.SetEquals(Sets.newHashSet("f")));
		}


//ORIGINAL LINE: @Test public void multipleExclPrefixOfMultipleExcl()
		public virtual void multipleExclPrefixOfMultipleExcl()
		{
			assertEquals(PrefixTestResult.POTENTIAL_PREFIX, ap("^f,g").isPrefixOf(ap("^f,h")));
		}


//ORIGINAL LINE: @Test public void testBaseValuePrefixOfFieldAccess()
		public virtual void testBaseValuePrefixOfFieldAccess()
		{
			assertEquals(GUARANTEED_PREFIX, ap("").isPrefixOf(ap("f")));
			assertEquals(NO_PREFIX, ap("f").isPrefixOf(ap("")));
		}


//ORIGINAL LINE: @Test public void testBaseValueIdentity()
		public virtual void testBaseValueIdentity()
		{
			assertEquals(GUARANTEED_PREFIX, ap("").isPrefixOf(ap("")));
		}


//ORIGINAL LINE: @Test public void testFieldAccessPrefixOfFieldAccess()
		public virtual void testFieldAccessPrefixOfFieldAccess()
		{
			assertEquals(GUARANTEED_PREFIX, ap("b").isPrefixOf(ap("b.c")));
			assertEquals(NO_PREFIX, ap("b.c").isPrefixOf(ap("b")));
		}


//ORIGINAL LINE: @Test public void testPrefixOfFieldAccessWithExclusion()
		public virtual void testPrefixOfFieldAccessWithExclusion()
		{
			assertEquals(GUARANTEED_PREFIX,ap("^f").isPrefixOf(ap("g")));
			assertEquals(NO_PREFIX,ap("g").isPrefixOf(ap("^f")));
		}


//ORIGINAL LINE: @Test public void testIdentityWithExclusion()
		public virtual void testIdentityWithExclusion()
		{
			assertEquals(GUARANTEED_PREFIX,ap("^f").isPrefixOf(ap("^f")));
			assertEquals(GUARANTEED_PREFIX,ap("^f,g").isPrefixOf(ap("^f,g")));
		}


//ORIGINAL LINE: @Test public void testDifferentExclusions()
		public virtual void testDifferentExclusions()
		{
			assertEquals(POTENTIAL_PREFIX,ap("^f").isPrefixOf(ap("^g")));
		}


//ORIGINAL LINE: @Test public void testMixedFieldAccess()
		public virtual void testMixedFieldAccess()
		{
			assertEquals(GUARANTEED_PREFIX,ap("^f").isPrefixOf(ap("g.g")));
			assertEquals(NO_PREFIX,ap("^f").isPrefixOf(ap("f.h")));
			assertEquals(GUARANTEED_PREFIX,ap("f").isPrefixOf(ap("f^g")));
		}


//ORIGINAL LINE: @Test public void testMultipleExclusions()
		public virtual void testMultipleExclusions()
		{
			assertEquals(NO_PREFIX,ap("^f,g").isPrefixOf(ap("^f")));
			assertEquals(POTENTIAL_PREFIX,ap("^f,h").isPrefixOf(ap("^f,g")));
			assertEquals(NO_PREFIX,ap("^f,g").isPrefixOf(ap("^g")));
			assertEquals(GUARANTEED_PREFIX,ap("^f").isPrefixOf(ap("^f,g")));
		}


//ORIGINAL LINE: @Test public void testDifferentAccessPathLength()
		public virtual void testDifferentAccessPathLength()
		{
			assertEquals(GUARANTEED_PREFIX,ap("^f").isPrefixOf(ap("g.h")));
		}


//ORIGINAL LINE: @Test public void testExclusionRequiresFieldAccess()
		public virtual void testExclusionRequiresFieldAccess()
		{
			assertEquals(GUARANTEED_PREFIX,ap("").isPrefixOf(ap("^f")));
			assertEquals(NO_PREFIX, ap("^f").isPrefixOf(ap("")));

			assertEquals(GUARANTEED_PREFIX,ap("f").isPrefixOf(ap("f^g")));
			assertEquals(NO_PREFIX,ap("f^g").isPrefixOf(ap("f")));

			assertEquals(GUARANTEED_PREFIX,ap("f").isPrefixOf(ap("f^g^h")));
			assertEquals(NO_PREFIX,ap("f^g^h").isPrefixOf(ap("f")));
		}

	}

}