using System.Collections.Generic;

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


	using Lists = com.google.common.collect.Lists;
	using Sets = com.google.common.collect.Sets;

	public abstract class Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private ISet<Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> interest_Conflict = Sets.newHashSet();
		private IList<InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> interestCallbacks = new List();
		protected internal PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool canBeResolvedEmpty_Conflict = false;

		public Resolver(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer)
		{
			this.analyzer = analyzer;
		}

		public abstract void resolve(FlowFunction_Constraint<System.Reflection.FieldInfo> constraint, InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callback);

		public virtual void interest(Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
		{
			if (!interest_Conflict.Add(resolver))
			{
				return;
			}

			log("Interest given by: " + resolver);
			foreach (InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callback in Lists.newLinkedList(interestCallbacks))
			{
				callback.interest(analyzer, resolver);
			}
		}

		protected internal virtual void canBeResolvedEmpty()
		{
			if (canBeResolvedEmpty_Conflict)
			{
				return;
			}

			canBeResolvedEmpty_Conflict = true;
			foreach (InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callback in Lists.newLinkedList(interestCallbacks))
			{
				callback.canBeResolvedEmpty();
			}
		}

		public virtual bool InterestGiven
		{
			get
			{
				return interest_Conflict.Count > 0;
			}
		}

		protected internal virtual void registerCallback(InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callback)
		{
			if (interest_Conflict.Count > 0)
			{
				foreach (Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver in Lists.newLinkedList(interest_Conflict))
				{
					callback.interest(analyzer, resolver);
				}
			}
			log("Callback registered");
			interestCallbacks.Add(callback);

			if (canBeResolvedEmpty_Conflict)
			{
				callback.canBeResolvedEmpty();
			}
		}

		protected internal abstract void log(string message);

	}

}