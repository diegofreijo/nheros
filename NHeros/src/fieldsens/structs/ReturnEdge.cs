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
namespace heros.fieldsens.structs
{
	using Delta = heros.fieldsens.AccessPath.Delta;
	using PrefixTestResult = heros.fieldsens.AccessPath.PrefixTestResult;

	public class ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		public readonly Fact incFact;
		public readonly Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolverAtCaller;
		public readonly AccessPath.Delta<System.Reflection.FieldInfo> callDelta;
		public readonly AccessPath<System.Reflection.FieldInfo> incAccessPath;
		public readonly Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incResolver;
		public readonly AccessPath.Delta<System.Reflection.FieldInfo> usedAccessPathOfIncResolver;

		public ReturnEdge(WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolverAtCaller, AccessPath.Delta<System.Reflection.FieldInfo> callDelta) : this(fact.Fact, fact.AccessPath, fact.Resolver, resolverAtCaller, callDelta, AccessPath.Delta.empty<System.Reflection.FieldInfo>())
		{
		}

		private ReturnEdge(Fact incFact, AccessPath<System.Reflection.FieldInfo> incAccessPath, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incResolver, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolverAtCaller, AccessPath.Delta<System.Reflection.FieldInfo> callDelta, AccessPath.Delta<System.Reflection.FieldInfo> usedAccessPathOfIncResolver)
		{
			this.incFact = incFact;
			this.incAccessPath = incAccessPath;
			this.incResolver = incResolver;
			this.resolverAtCaller = resolverAtCaller;
			this.callDelta = callDelta;
			this.usedAccessPathOfIncResolver = usedAccessPathOfIncResolver;
		}

		public virtual ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> copyWithIncomingResolver(Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incResolver, AccessPath.Delta<System.Reflection.FieldInfo> usedAccessPathOfIncResolver)
		{
			return new ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(incFact, incAccessPath, incResolver, resolverAtCaller, callDelta, usedAccessPathOfIncResolver);
		}

		public virtual ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> copyWithResolverAtCaller(Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolverAtCaller, AccessPath.Delta<System.Reflection.FieldInfo> usedAccessPathOfIncResolver)
		{
			return new ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(incFact, incAccessPath, null, resolverAtCaller, callDelta, usedAccessPathOfIncResolver);
		}

		public override string ToString()
		{
			return string.Format("IncFact: {0}{1}, Delta: {2}, IncResolver: <{3}:{4}>, ResolverAtCallSite: {5}", incFact, incAccessPath, callDelta, usedAccessPathOfIncResolver, incResolver, resolverAtCaller);
		}


	}
}