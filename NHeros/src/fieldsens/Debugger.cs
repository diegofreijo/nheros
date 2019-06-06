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

	public interface Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		InterproceduralCFG<Stmt, System.Reflection.MethodInfo> ICFG {set;}
		void initialSeed(Stmt stmt);
		void edgeTo(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt);
		void newResolver(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver);
		void newJob(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt);
		void jobStarted(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt);
		void jobFinished(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt);
		void askedToResolve(Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver, FlowFunction_Constraint<System.Reflection.FieldInfo> constraint);
	}

	public class Debugger_NullDebugger <System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		public virtual InterproceduralCFG<Stmt, System.Reflection.MethodInfo> ICFG
		{
			set
			{
    
			}
		}

		public virtual void initialSeed(Stmt stmt)
		{

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