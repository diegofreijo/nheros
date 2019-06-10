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
	public interface FactMergeHandler<Fact>
	{
		/// <summary>
		/// Called when propagating a Fact to a statement at which an equal Fact was already propagated to. </summary>
		/// <param name="previousFact"> The Fact instance that was propagated to the statement first. </param>
		/// <param name="currentFact"> The Fact that was propagated to the statement last. </param>
		void merge(Fact previousFact, Fact currentFact);

		/// <summary>
		/// Called on Facts being propagated over a return edge. Via this method context can be restored that was abstracted when propagating over the call edge. </summary>
		/// <param name="factAtReturnSite"> The fact being propagated over the return edge. </param>
		/// <param name="factAtCallSite"> The Fact that was present at the call site, i.e., the Fact used as input to the call flow function. </param>
		void restoreCallingContext(Fact factAtReturnSite, Fact factAtCallSite);
	}

}