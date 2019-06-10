﻿using heros.fieldsens.structs;
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
	public interface MethodAnalyzer<Field, Fact, Stmt, Method>
	{
		void addIncomingEdge(CallEdge<Field, Fact, Stmt, Method> incEdge);

		void addInitialSeed(Stmt startPoint, Fact val);

		void addUnbalancedReturnFlow(WrappedFactAtStatement<Field, Fact, Stmt, Method> target, Stmt callSite);
	}

}