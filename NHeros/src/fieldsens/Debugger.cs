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
namespace heros.fieldsens
{
    public interface Debugger<Field, Fact, Stmt, Method>
	{
		InterproceduralCFG<Stmt, Method> ICFG {set;}
		void initialSeed(Stmt stmt);
		void edgeTo(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt);
		void newResolver(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Resolver<Field, Fact, Stmt, Method> resolver);
		void newJob(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt);
		void jobStarted(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt);
		void jobFinished(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt);
		void askedToResolve(Resolver<Field, Fact, Stmt, Method> resolver, FlowFunction_Constraint<Field> constraint);
	}

	public class Debugger_NullDebugger <Field, Fact, Stmt, Method> : Debugger<Field, Fact, Stmt, Method>
	{

		public virtual InterproceduralCFG<Stmt, Method> ICFG
		{
			set
			{
    
			}
		}

		public virtual void initialSeed(Stmt stmt)
		{

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