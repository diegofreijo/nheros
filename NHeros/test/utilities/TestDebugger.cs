using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

	using Sets = com.google.common.collect.Sets;

	using Debugger = heros.fieldsens.Debugger;
	using FlowFunction_Constraint = heros.fieldsens.FlowFunction_Constraint;
	using PerAccessPathMethodAnalyzer = heros.fieldsens.PerAccessPathMethodAnalyzer;
	using Resolver = heros.fieldsens.Resolver;
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;

	public class TestDebugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private JsonDocument root = new JsonDocument();
		private InterproceduralCFG<Stmt, System.Reflection.MethodInfo> icfg;

		public virtual void writeJsonDebugFile(string filename)
		{
			try
			{
				StreamWriter writer = new StreamWriter(filename);
				StringBuilder builder = new StringBuilder();
				builder.Append("var root=");
				root.write(builder, 0);
				writer.Write(builder.ToString());
				writer.Close();
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		/* (non-Javadoc)
		 * @see heros.alias.Debugger#setICFG(I)
		 */
		public virtual InterproceduralCFG<Stmt, System.Reflection.MethodInfo> ICFG
		{
			set
			{
				this.icfg = value;
			}
		}

		/* (non-Javadoc)
		 * @see heros.alias.Debugger#initialSeed(Stmt)
		 */
		public virtual void initialSeed(Stmt stmt)
		{
			stmt(stmt).keyValue("seed", "true");

			includeSuccessors(stmt, Sets.newHashSet<Stmt> ());
		}

		private void includeSuccessors(Stmt stmt, ISet<Stmt> visited)
		{
			if (!visited.Add(stmt))
			{
				return;
			}

			JsonDocument doc = stmt(stmt);
			foreach (Stmt succ in icfg.getSuccsOf(stmt))
			{
				doc.array("successors").add(succ.ToString());
				stmt(succ);
				includeSuccessors(succ, visited);
			}

			if (icfg.isCallStmt(stmt))
			{
				foreach (System.Reflection.MethodInfo m in icfg.getCalleesOfCallAt(stmt))
				{
					doc.doc("calls").doc(m.ToString());
					foreach (Stmt sp in icfg.getStartPointsOf(m))
					{
						stmt(sp).keyValue("startPoint", "true");
						includeSuccessors(sp, visited);
					}
				}
				foreach (Stmt retSite in icfg.getReturnSitesOfCallAt(stmt))
				{
					doc.array("successors").add(retSite.ToString());
					stmt(retSite);
					includeSuccessors(retSite, visited);
				}
			}
			if (icfg.isExitStmt(stmt))
			{
				foreach (Stmt callSite in icfg.getCallersOf(icfg.getMethodOf(stmt)))
				{
					foreach (Stmt retSite in icfg.getReturnSitesOfCallAt(callSite))
					{
						doc.doc("returns").doc(retSite.ToString());
						includeSuccessors(retSite, visited);
					}
				}
			}
		}

		protected internal virtual JsonDocument stmt(Stmt stmt)
		{
			System.Reflection.MethodInfo methodOf = icfg.getMethodOf(stmt);
			return root.doc("methods").doc(methodOf.ToString()).doc(stmt.ToString());
		}

		public virtual void expectNormalFlow(Stmt unit, string expectedFlowFunctionsToString)
		{
			stmt(unit).keyValue("flow", expectedFlowFunctionsToString);
		}

		public virtual void expectCallFlow(Stmt callSite, System.Reflection.MethodInfo destinationMethod, string expectedFlowFunctionsToString)
		{
			stmt(callSite).doc("calls").doc(destinationMethod.ToString()).keyValue("flow", expectedFlowFunctionsToString);
		}

		public virtual void expectReturnFlow(Stmt exitStmt, Stmt returnSite, string expectedFlowFunctionsToString)
		{
			if (returnSite != default(Stmt))
			{
				stmt(exitStmt).doc("returns").doc(returnSite.ToString()).keyValue("flow", expectedFlowFunctionsToString);
			}
		}

		public virtual void edgeTo(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{

		}

		public virtual void newResolver(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
		{

		}

		public virtual void newJob(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{

		}

		public virtual void jobStarted(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{

		}

		public virtual void jobFinished(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{

		}

		public virtual void askedToResolve(Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver, FlowFunction_Constraint<System.Reflection.FieldInfo> constraint)
		{

		}

	}

}