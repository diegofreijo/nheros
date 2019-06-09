using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using heros.fieldsens;
using NHeros.src.util;
using heros.fieldsens.structs;

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
	public class TestDebugger<Field, Fact, Stmt, Method> : Debugger<Field, Fact, Stmt, Method>
	{

		private JsonDocument root = new JsonDocument();
		private InterproceduralCFG<Stmt, Method> icfg;

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

		public virtual InterproceduralCFG<Stmt, Method> ICFG
		{
			set
			{
				this.icfg = value;
			}
		}

		public virtual void initialSeed(Stmt stmt)
		{
			Statements(stmt).keyValue("seed", "true");
			includeSuccessors(stmt, new HashSet<Stmt>());
		}

		private void includeSuccessors(Stmt stmt, ISet<Stmt> visited)
		{
			if (!visited.Add(stmt))
			{
				return;
			}

			JsonDocument doc = Statements(stmt);
			foreach (Stmt succ in icfg.getSuccsOf(stmt))
			{
				doc.array("successors").add(succ.ToString());
				Statements(succ);
				includeSuccessors(succ, visited);
			}

			if (icfg.isCallStmt(stmt))
			{
				foreach (Method m in icfg.getCalleesOfCallAt(stmt))
				{
					doc.doc("calls").doc(m.ToString());
					foreach (Stmt sp in icfg.getStartPointsOf(m))
					{
						Statements(sp).keyValue("startPoint", "true");
						includeSuccessors(sp, visited);
					}
				}
				foreach (Stmt retSite in icfg.getReturnSitesOfCallAt(stmt))
				{
					doc.array("successors").add(retSite.ToString());
					Statements(retSite);
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

		protected internal virtual JsonDocument Statements(Stmt stmt)
		{
			Method methodOf = icfg.getMethodOf(stmt);
			return root.doc("methods").doc(methodOf.ToString()).doc(stmt.ToString());
		}

		public virtual void expectNormalFlow(Stmt unit, string expectedFlowFunctionsToString)
		{
			Statements(unit).keyValue("flow", expectedFlowFunctionsToString);
		}

		public virtual void expectCallFlow(Stmt callSite, Method destinationMethod, string expectedFlowFunctionsToString)
		{
			Statements(callSite).doc("calls").doc(destinationMethod.ToString()).keyValue("flow", expectedFlowFunctionsToString);
		}

		public virtual void expectReturnFlow(Stmt exitStmt, Stmt returnSite, string expectedFlowFunctionsToString)
		{
			if (Utils.IsDefault(returnSite))
			{
				Statements(exitStmt).doc("returns").doc(returnSite.ToString()).keyValue("flow", expectedFlowFunctionsToString);
			}
		}

		public virtual void edgeTo(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{

		}

		public virtual void newResolver(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Resolver<Field, Fact, Stmt, Method> resolver)
		{

		}

		public virtual void newJob(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{

		}

		public virtual void jobStarted(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{

		}

		public virtual void jobFinished(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{

		}

		public virtual void askedToResolve(Resolver<Field, Fact, Stmt, Method> resolver, FlowFunction_Constraint<Field> constraint)
		{

		}

	}

}