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
    public abstract class Edge<Fact>
    {
        public readonly ExpectedFlowFunction<Fact>[] flowFunctions;
        public bool includeInCfg = true;

        public Edge(ExpectedFlowFunction<Fact>[] flowFunctions)
        {
            this.flowFunctions = flowFunctions;
            foreach (ExpectedFlowFunction<Fact> ff in flowFunctions)
            {
                ff.edge = this;
            }
        }

        public abstract void accept(EdgeVisitor<Fact> visitor);
    }

	public class NormalEdge<Fact> : Edge<Fact>
    {

		public readonly Statement unit;
		public readonly Statement succUnit;

		public NormalEdge(Statement unit, Statement succUnit, ExpectedFlowFunction<Fact>[] flowFunctions) : base(flowFunctions)
		{
			this.unit = unit;
			this.succUnit = succUnit;
		}

		public override string ToString()
		{
			return string.Format("{0} -normal-> {1}", unit, succUnit);
		}

		public override void accept(EdgeVisitor<Fact> visitor)
		{
			visitor.visit(this);
		}
	}

	public class CallEdge<Fact> : Edge<Fact>
    {

		public readonly Statement callSite;
		public readonly TestMethod destinationMethod;

		public CallEdge(Statement callSite, TestMethod destinationMethod, ExpectedFlowFunction<Fact>[] flowFunctions) : base(flowFunctions)
		{
			this.callSite = callSite;
			this.destinationMethod = destinationMethod;
		}

		public override string ToString()
		{
			return string.Format("{0} -call-> {1}", callSite, destinationMethod);
		}

		public override void accept(EdgeVisitor<Fact> visitor)
		{
			visitor.visit(this);
		}
	}

	public class Call2ReturnEdge<Fact> : Edge<Fact>
    {
		public readonly Statement callSite;
		public readonly Statement returnSite;

		public Call2ReturnEdge(Statement callSite, Statement returnSite, ExpectedFlowFunction<Fact>[] flowFunctions) : base(flowFunctions)
		{
			this.callSite = callSite;
			this.returnSite = returnSite;
		}

		public override string ToString()
		{
			return string.Format("{0} -call2ret-> {1}", callSite, returnSite);
		}

		public override void accept(EdgeVisitor<Fact> visitor)
		{
			visitor.visit(this);
		}
	}

	public class ReturnEdge<Fact> : Edge<Fact>
    {
		public readonly Statement exitStmt;
		public readonly Statement returnSite;
		public readonly Statement callSite;
		public TestMethod calleeMethod;

		public ReturnEdge(Statement callSite, Statement exitStmt, Statement returnSite, ExpectedFlowFunction<Fact>[] flowFunctions) : base(flowFunctions)
		{
			this.callSite = callSite;
			this.exitStmt = exitStmt;
			this.returnSite = returnSite;
			if (callSite == null || returnSite == null)
			{
				includeInCfg = false;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} -return-> {1}", exitStmt, returnSite);
		}

		public override void accept(EdgeVisitor<Fact> visitor)
		{
			visitor.visit(this);
		}
	}


	public interface EdgeVisitor<Fact>
    {
		void visit(NormalEdge<Fact> edge);
		void visit(CallEdge<Fact> edge);
		void visit(Call2ReturnEdge<Fact> edge);
		void visit(ReturnEdge<Fact> edge);
	}
}