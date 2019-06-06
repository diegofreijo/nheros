using System.Collections.Generic;
using System.Diagnostics;

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



	using HashBasedTable = com.google.common.collect.HashBasedTable;
	using Table = com.google.common.collect.Table;
	using Cell = com.google.common.collect.Table.Cell;


	/// <summary>
	/// The IDE algorithm uses a list of jump functions. Instead of a list, we use a set of three
	/// maps that are kept in sync. This allows for efficient indexing: the algorithm accesses
	/// elements from the list through three different indices.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ThreadSafe public class JumpFunctions<N,D,L>
	public class JumpFunctions<N, D, L>
	{
		[SynchronizedBy("consistent lock on this")]
		protected internal Table<N, D, IDictionary<D, EdgeFunction<L>>> nonEmptyReverseLookup = HashBasedTable.create();

		//mapping from source value and target node to a list of all target values and associated functions
		//where the list is implemented as a mapping from the source value to the function
		//we exclude empty default functions 
		[SynchronizedBy("consistent lock on this")]
		protected internal Table<D, N, IDictionary<D, EdgeFunction<L>>> nonEmptyForwardLookup = HashBasedTable.create();

		//a mapping from target node to a list of triples consisting of source value,
		//target value and associated function; the triple is implemented by a table
		//we exclude empty default functions 
		[SynchronizedBy("consistent lock on this")]
		protected internal IDictionary<N, Table<D, D, EdgeFunction<L>>> nonEmptyLookupByTargetNode = new Dictionary<N, Table<D, D, EdgeFunction<L>>>();

		[DontSynchronize("immutable")]
		private readonly EdgeFunction<L> allTop;

		public JumpFunctions(EdgeFunction<L> allTop)
		{
			this.allTop = allTop;
		}

		/// <summary>
		/// Records a jump function. The source statement is implicit. </summary>
		/// <seealso cref= PathEdge </seealso>
		public virtual void addFunction(D sourceVal, N target, D targetVal, EdgeFunction<L> function)
		{
			lock (this)
			{
				Debug.Assert(sourceVal != default(D));
				Debug.Assert(target != default(N));
				Debug.Assert(targetVal != default(D));
				Debug.Assert(function != null);
        
				//we do not store the default function (all-top)
				if (function.equalTo(allTop))
				{
					return;
				}
        
				IDictionary<D, EdgeFunction<L>> sourceValToFunc = nonEmptyReverseLookup.get(target, targetVal);
				if (sourceValToFunc == null)
				{
					sourceValToFunc = new LinkedHashMap<D, EdgeFunction<L>>();
					nonEmptyReverseLookup.put(target,targetVal,sourceValToFunc);
				}
				sourceValToFunc[sourceVal] = function;
        
				IDictionary<D, EdgeFunction<L>> targetValToFunc = nonEmptyForwardLookup.get(sourceVal, target);
				if (targetValToFunc == null)
				{
					targetValToFunc = new LinkedHashMap<D, EdgeFunction<L>>();
					nonEmptyForwardLookup.put(sourceVal,target,targetValToFunc);
				}
				targetValToFunc[targetVal] = function;
        
				Table<D, D, EdgeFunction<L>> table = nonEmptyLookupByTargetNode[target];
				if (table == null)
				{
					table = HashBasedTable.create();
					nonEmptyLookupByTargetNode[target] = table;
				}
				table.put(sourceVal, targetVal, function);
			}
		}

		/// <summary>
		/// Returns, for a given target statement and value all associated
		/// source values, and for each the associated edge function.
		/// The return value is a mapping from source value to function.
		/// </summary>
		public virtual IDictionary<D, EdgeFunction<L>> reverseLookup(N target, D targetVal)
		{
			lock (this)
			{
				Debug.Assert(target != default(N));
				Debug.Assert(targetVal != default(D));
				IDictionary<D, EdgeFunction<L>> res = nonEmptyReverseLookup.get(target,targetVal);
				if (res == null)
				{
					return Collections.emptyMap();
				}
				return res;
			}
		}

		/// <summary>
		/// Returns, for a given source value and target statement all
		/// associated target values, and for each the associated edge function. 
		/// The return value is a mapping from target value to function.
		/// </summary>
		public virtual IDictionary<D, EdgeFunction<L>> forwardLookup(D sourceVal, N target)
		{
			lock (this)
			{
				Debug.Assert(sourceVal != default(D));
				Debug.Assert(target != default(N));
				IDictionary<D, EdgeFunction<L>> res = nonEmptyForwardLookup.get(sourceVal, target);
				if (res == null)
				{
					return Collections.emptyMap();
				}
				return res;
			}
		}

		/// <summary>
		/// Returns for a given target statement all jump function records with this target.
		/// The return value is a set of records of the form (sourceVal,targetVal,edgeFunction).
		/// </summary>
		public virtual ISet<Table.Cell<D, D, EdgeFunction<L>>> lookupByTarget(N target)
		{
			lock (this)
			{
				Debug.Assert(target != default(N));
				Table<D, D, EdgeFunction<L>> table = nonEmptyLookupByTargetNode[target];
				if (table == null)
				{
					return Collections.emptySet();
				}
				ISet<Table.Cell<D, D, EdgeFunction<L>>> res = table.cellSet();
				if (res == null)
				{
					return Collections.emptySet();
				}
				return res;
			}
		}

		/// <summary>
		/// Removes a jump function. The source statement is implicit. </summary>
		/// <seealso cref= PathEdge </seealso>
		/// <returns> True if the function has actually been removed. False if it was not
		/// there anyway. </returns>
		public virtual bool removeFunction(D sourceVal, N target, D targetVal)
		{
			lock (this)
			{
				Debug.Assert(sourceVal != default(D));
				Debug.Assert(target != default(N));
				Debug.Assert(targetVal != default(D));
        
				IDictionary<D, EdgeFunction<L>> sourceValToFunc = nonEmptyReverseLookup.get(target, targetVal);
				if (sourceValToFunc == null)
				{
					return false;
				}
				if (sourceValToFunc.Remove(sourceVal) == null)
				{
					return false;
				}
				if (sourceValToFunc.Count == 0)
				{
					nonEmptyReverseLookup.remove(targetVal, targetVal);
				}
        
				IDictionary<D, EdgeFunction<L>> targetValToFunc = nonEmptyForwardLookup.get(sourceVal, target);
				if (targetValToFunc == null)
				{
					return false;
				}
				if (targetValToFunc.Remove(targetVal) == null)
				{
					return false;
				}
				if (targetValToFunc.Count == 0)
				{
					nonEmptyForwardLookup.remove(sourceVal, target);
				}
        
				Table<D, D, EdgeFunction<L>> table = nonEmptyLookupByTargetNode[target];
				if (table == null)
				{
					return false;
				}
				if (table.remove(sourceVal, targetVal) == null)
				{
					return false;
				}
				if (table.Empty)
				{
					nonEmptyLookupByTargetNode.Remove(target);
				}
        
				return true;
			}
		}

		/// <summary>
		/// Removes all jump functions
		/// </summary>
		public virtual void clear()
		{
			lock (this)
			{
				this.nonEmptyForwardLookup.clear();
				this.nonEmptyLookupByTargetNode.Clear();
				this.nonEmptyReverseLookup.clear();
			}
		}

	}

}