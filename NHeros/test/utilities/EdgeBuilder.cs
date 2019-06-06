using System.Collections.Generic;

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
	public abstract class EdgeBuilder<Fact>
    {

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal IList<Edge<Fact>> edges_Conflict = new List<Edge<Fact>>();
		public virtual ICollection<Edge<Fact>> edges()
		{
			if (edges_Conflict.Count == 0)
			{
				throw new System.InvalidOperationException("Not a single edge created on EdgeBuilder: " + ToString());
			}

			return edges_Conflict;
		}

		public class CallSiteBuilder : EdgeBuilder<Fact>
        {
			internal Statement callSite;

			public CallSiteBuilder(Statement callSite)
			{
				this.callSite = callSite;
			}

			public virtual CallSiteBuilder calls(string method, params ExpectedFlowFunction<Fact>[] flows)
			{
				edges_Conflict.Add(new Edge<Fact>.CallEdge(callSite, new TestMethod(method), flows));
				return this;
			}

			public virtual CallSiteBuilder retSite(string returnSite, ExpectedFlowFunction<Fact>[] flows)
			{
				edges_Conflict.Add(new Edge<Fact>.Call2ReturnEdge(callSite, new Statement(returnSite), flows));
				return this;
			}
		}

		public class NormalStmtBuilder : EdgeBuilder<Fact>
        {

			internal Statement stmt;
			internal ExpectedFlowFunction<Fact>[] flowFunctions;

			public NormalStmtBuilder(Statement stmt, ExpectedFlowFunction<Fact>[] flowFunctions)
			{
				this.stmt = stmt;
				this.flowFunctions = flowFunctions;
			}

			public virtual NormalStmtBuilder succ(string succ)
			{
				edges_Conflict.Add(new Edge<Fact>.NormalEdge(stmt, new Statement(succ), flowFunctions));
				return this;
			}
		}

		public class ExitStmtBuilder : EdgeBuilder<Fact>
        {
			internal Statement exitStmt;

			public ExitStmtBuilder(Statement exitStmt)
			{
				this.exitStmt = exitStmt;
			}

			public virtual ExitStmtBuilder expectArtificalFlow(ExpectedFlowFunction<Fact>[] flows)
			{
				edges_Conflict.Add(new Edge<Fact>.ReturnEdge(null, exitStmt, null, flows));
				return this;
			}

			public virtual ExitStmtBuilder returns(Statement callSite, Statement returnSite, params ExpectedFlowFunction<Fact>[] flows)
			{
				edges_Conflict.Add(new Edge<Fact>.ReturnEdge(callSite, exitStmt, returnSite, flows));
				return this;
			}

		}
	}

}