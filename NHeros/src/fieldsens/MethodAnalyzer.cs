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
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;

	public interface MethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		void addIncomingEdge(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge);

		void addInitialSeed(Stmt startPoint, Fact val);

		void addUnbalancedReturnFlow(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> target, Stmt callSite);
	}

}