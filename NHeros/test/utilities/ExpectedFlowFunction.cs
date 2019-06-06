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
    using heros.fieldsens;
    

	public abstract class ExpectedFlowFunction<Fact>
    {
		public readonly Fact source;
		public readonly Fact[] targets;
		public Edge<Fact> edge;
		internal int times;

		public ExpectedFlowFunction(int times, Fact source, params Fact[] targets)
		{
			this.times = times;
			this.source = source;
			this.targets = targets;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1} -> {{{2}}}", edge, source, string.Join(",", targets));
		}

		public abstract string transformerString();

		public abstract heros.fieldsens.FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler);
	}
}