using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2015 Eric Bodden.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     John Toman - initial API and implementation 
/// *****************************************************************************
/// </summary>
namespace heros.solver
{


	using Table = com.google.common.collect.Table;
	using Cell = com.google.common.collect.Table.Cell;

	/// <summary>
	/// A class to dump the results of flow functions to a dot file for visualization.
	/// 
	/// This class can be used for both IDE and IFDS problems that have implemented the
	/// <seealso cref="SolverDebugConfiguration"/> and overridden <seealso cref="SolverDebugConfiguration.recordEdges()"/>
	/// to return true.
	/// </summary>
	/// @param <N> The type of nodes in the interprocedural control-flow graph. Typically <seealso cref="Unit"/>. </param>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	/// @param <M> The type of objects used to represent methods. Typically <seealso cref="SootMethod"/>. </param>
	/// @param <I> The type of inter-procedural control-flow graph being used. </param>
	public class FlowFunctionDotExport<N, D, M, I> where I : heros.InterproceduralCFG<N, M>
	{
		private class Numberer<D>
		{
			internal long counter = 1;
			internal IDictionary<D, long> map = new Dictionary<D, long>();

			public virtual void add(D o)
			{
				if (map.ContainsKey(o))
				{
					return;
				}
				map[o] = counter++;
			}
			public virtual long get(D o)
			{
				if (Utils.IsDefault(o))
				{
					throw new System.ArgumentException("Null key");
				}
				if (!map.ContainsKey(o))
				{
					throw new System.ArgumentException("Failed to find number for: " + o);
				}
				return map[o];

			}
		}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final IDESolver<N, D, M, ?, I> solver;
		private readonly IDESolver<N, D, M, ?, I> solver;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final heros.ItemPrinter<? super N, ? super D, ? super M> printer;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		private readonly ItemPrinter<object, ?, ?> printer;
		private readonly ISet<M> methodWhitelist;

		/// <summary>
		/// Constructor. </summary>
		/// <param name="solver"> The solver instance to dump. </param>
		/// <param name="printer"> The printer object to use to create the string representations of
		/// the nodes, facts, and methods in the exploded super-graph. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public FlowFunctionDotExport(IDESolver<N, D, M, ?, I> solver, heros.ItemPrinter<? super N, ? super D, ? super M> printer)
		public FlowFunctionDotExport<T1, T2>(IDESolver<T1> solver, ItemPrinter<T2> printer) : this(solver, printer, null)
		{
		}

		/// <summary>
		/// Constructor. </summary>
		/// <param name="solver"> The solver instance to dump. </param>
		/// <param name="printer"> The printer object to use to create the string representations of
		/// the nodes, facts, and methods in the exploded super-graph. </param>
		/// <param name="methodWhitelist"> A set of methods of type M for which the full graphs should be printed.
		/// Flow functions for which both unit endpoints are not contained in a method in methodWhitelist are not printed.
		/// Callee/caller edges into/out of the methods in the set are still printed.   </param>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public FlowFunctionDotExport(IDESolver<N, D, M, ?, I> solver, heros.ItemPrinter<? super N, ? super D, ? super M> printer, java.util.Set<M> methodWhitelist)
		public FlowFunctionDotExport<T1, T2>(IDESolver<T1> solver, ItemPrinter<T2> printer, ISet<M> methodWhitelist)
		{
			this.solver = solver;
			this.printer = printer;
			this.methodWhitelist = methodWhitelist;
		}

		private static ISet<U> getOrMakeSet<K, U>(IDictionary<K, ISet<U>> map, K key)
		{
			if (map.ContainsKey(key))
			{
				return map[key];
			}
			HashSet<U> toRet = new HashSet<U>();
			map[key] = toRet;
			return toRet;
		}

		private string escapeLabelString(string toEscape)
		{
			return toEscape.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("<", "\\<").Replace(">", "\\>");
		}

		private class UnitFactTracker
		{
			private readonly FlowFunctionDotExport<N, D, M, I> outerInstance;

			public UnitFactTracker(FlowFunctionDotExport<N, D, M, I> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal Numberer<Pair<N, D>> factNumbers = new Numberer<Pair<N, D>>();
			internal Numberer<N> unitNumbers = new Numberer<N>();
			internal IDictionary<N, ISet<D>> factsForUnit = new Dictionary<N, ISet<D>>();
			internal IDictionary<M, ISet<N>> methodToUnit = new Dictionary<M, ISet<N>>();

			internal IDictionary<M, ISet<N>> stubMethods = new Dictionary<M, ISet<N>>();

			public virtual void registerFactAtUnit(N unit, D fact)
			{
				getOrMakeSet(factsForUnit, unit).Add(fact);
				factNumbers.add(new Pair<N, D>(unit, fact));
			}

			public virtual void registerUnit(M method, N unit)
			{
				unitNumbers.add(unit);
				getOrMakeSet(methodToUnit, method).Add(unit);
			}

			public virtual void registerStubUnit(M method, N unit)
			{
				Debug.Assert(!methodToUnit.ContainsKey(method));
				unitNumbers.add(unit);
				getOrMakeSet(stubMethods, method).Add(unit);
			}

			public virtual string getUnitLabel(N unit)
			{
				return "u" + unitNumbers.get(unit);
			}

			public virtual string getFactLabel(N unit, D fact)
			{
				return "f" + factNumbers.get(new Pair<N, D>(unit, fact));
			}

			public virtual string getEdgePoint(N unit, D fact)
			{
				return this.getUnitLabel(unit) + ":" + this.getFactLabel(unit, fact);
			}
		}

		private void numberEdges(Table<N, N, IDictionary<D, ISet<D>>> edgeSet, UnitFactTracker utf)
		{
			foreach (Table.Cell<N, N, IDictionary<D, ISet<D>>> c in edgeSet.cellSet())
			{
				N sourceUnit = c.RowKey;
				N destUnit = c.ColumnKey;
				M destMethod = solver.icfg.getMethodOf(destUnit);
				M sourceMethod = solver.icfg.getMethodOf(sourceUnit);
				if (isMethodFiltered(sourceMethod) && isMethodFiltered(destMethod))
				{
					continue;
				}
				if (isMethodFiltered(destMethod))
				{
					utf.registerStubUnit(destMethod, destUnit);
				}
				else
				{
					utf.registerUnit(destMethod, destUnit);
				}
				if (isMethodFiltered(sourceMethod))
				{
					utf.registerStubUnit(sourceMethod, sourceUnit);
				}
				else
				{
					utf.registerUnit(sourceMethod, sourceUnit);
				}
				foreach (KeyValuePair<D, ISet<D>> entry in c.Value.entrySet())
				{
					utf.registerFactAtUnit(sourceUnit, entry.Key);
					foreach (D destFact in entry.Value)
					{
						utf.registerFactAtUnit(destUnit, destFact);
					}
				}
			}
		}

		private bool isMethodFiltered(M method)
		{
			return methodWhitelist != null && !methodWhitelist.Contains(method);
		}

		private bool isNodeFiltered(N node)
		{
			return isMethodFiltered(solver.icfg.getMethodOf(node));
		}

		private void printMethodUnits(ISet<N> units, M method, PrintStream pf, UnitFactTracker utf)
		{
			foreach (N methodUnit in units)
			{
				ISet<D> loc = utf.factsForUnit[methodUnit];
				string unitText = escapeLabelString(printer.printNode(methodUnit, method));
				pf.print(utf.getUnitLabel(methodUnit) + " [shape=record,label=\"" + unitText + " ");
				foreach (D hl in loc)
				{
					pf.print("| <" + utf.getFactLabel(methodUnit, hl) + "> " + escapeLabelString(printer.printFact(hl)));
				}
				pf.println("\"];");
			}
		}

		/// <summary>
		/// Write a graph representation of the flow functions computed by the solver
		/// to the file indicated by fileName.
		/// 
		/// <b>Note:</b> This method should only be called after 
		/// the solver passed to this object's constructor has had its <seealso cref="IDESolver.solve()"/>
		/// method called. </summary>
		/// <param name="fileName"> The output file to which to write the dot representation. </param>
		public virtual void dumpDotFile(string fileName)
		{
			File f = new File(fileName);
			PrintStream pf = null;
			try
			{
				pf = new PrintStream(f);
				UnitFactTracker utf = new UnitFactTracker(this);

				numberEdges(solver.computedIntraPEdges, utf);
				numberEdges(solver.computedInterPEdges, utf);

				pf.println("digraph ifds {" + "node[shape=record];");
				int methodCounter = 0;
				foreach (KeyValuePair<M, ISet<N>> kv in utf.methodToUnit.SetOfKeyValuePairs())
				{
					ISet<N> intraProc = kv.Value;
					pf.println("subgraph cluster" + methodCounter + " {");
					methodCounter++;
					printMethodUnits(intraProc, kv.Key, pf, utf);
					foreach (N methodUnit in intraProc)
					{
						IDictionary<N, IDictionary<D, ISet<D>>> flows = solver.computedIntraPEdges.row(methodUnit);
						foreach (KeyValuePair<N, IDictionary<D, ISet<D>>> kv2 in flows.SetOfKeyValuePairs())
						{
							N destUnit = kv2.Key;
							foreach (KeyValuePair<D, ISet<D>> pointFlow in kv2.Value.entrySet())
							{
								foreach (D destFact in pointFlow.Value)
								{
									string edge = utf.getEdgePoint(methodUnit, pointFlow.Key) + " -> " + utf.getEdgePoint(destUnit, destFact);
									pf.print(edge);
									pf.println(";");
								}
							}
						}
					}
					pf.println("label=\"" + escapeLabelString(printer.printMethod(kv.Key)) + "\";");
					pf.println("}");
				}
				foreach (KeyValuePair<M, ISet<N>> kv in utf.stubMethods.SetOfKeyValuePairs())
				{
					pf.println("subgraph cluster" + methodCounter++ + " {");
					printMethodUnits(kv.Value, kv.Key, pf, utf);
					pf.println("label=\"" + escapeLabelString("[STUB] " + printer.printMethod(kv.Key)) + "\";");
					pf.println("graph[style=dotted];");
					pf.println("}");
				}
				foreach (Table.Cell<N, N, IDictionary<D, ISet<D>>> c in solver.computedInterPEdges.cellSet())
				{
					if (isNodeFiltered(c.RowKey) && isNodeFiltered(c.ColumnKey))
					{
						continue;
					}
					foreach (KeyValuePair<D, ISet<D>> kv in c.Value.entrySet())
					{
						foreach (D dFact in kv.Value)
						{
							pf.print(utf.getEdgePoint(c.RowKey, kv.Key));
							pf.print(" -> ");
							pf.print(utf.getEdgePoint(c.ColumnKey, dFact));
							pf.println(" [style=dotted];");
						}
					}
				}
				pf.println("}");
			}
			catch (FileNotFoundException e)
			{
				throw new Exception("Writing dot output failed", e);
			}
			finally
			{
				if (pf != null)
				{
					pf.close();
				}
			}
		}
	}

}