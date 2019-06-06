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

	public class ReturnEdge<Field, Fact, Stmt, Method>
	{

		public readonly Fact incFact;
		public readonly Resolver<Field, Fact, Stmt, Method> resolverAtCaller;
		public readonly AccessPath.Delta<Field> callDelta;
		public readonly AccessPath<Field> incAccessPath;
		public readonly Resolver<Field, Fact, Stmt, Method> incResolver;
		public readonly AccessPath.Delta<Field> usedAccessPathOfIncResolver;

		public ReturnEdge(WrappedFact<Field, Fact, Stmt, Method> fact, Resolver<Field, Fact, Stmt, Method> resolverAtCaller, AccessPath.Delta<Field> callDelta) : this(fact.Fact, fact.AccessPath, fact.Resolver, resolverAtCaller, callDelta, AccessPath.Delta.empty<Field>())
		{
		}

		private ReturnEdge(Fact incFact, AccessPath<Field> incAccessPath, Resolver<Field, Fact, Stmt, Method> incResolver, Resolver<Field, Fact, Stmt, Method> resolverAtCaller, AccessPath.Delta<Field> callDelta, AccessPath.Delta<Field> usedAccessPathOfIncResolver)
		{
			this.incFact = incFact;
			this.incAccessPath = incAccessPath;
			this.incResolver = incResolver;
			this.resolverAtCaller = resolverAtCaller;
			this.callDelta = callDelta;
			this.usedAccessPathOfIncResolver = usedAccessPathOfIncResolver;
		}

		public virtual ReturnEdge<Field, Fact, Stmt, Method> copyWithIncomingResolver(Resolver<Field, Fact, Stmt, Method> incResolver, AccessPath.Delta<Field> usedAccessPathOfIncResolver)
		{
			return new ReturnEdge<Field, Fact, Stmt, Method>(incFact, incAccessPath, incResolver, resolverAtCaller, callDelta, usedAccessPathOfIncResolver);
		}

		public virtual ReturnEdge<Field, Fact, Stmt, Method> copyWithResolverAtCaller(Resolver<Field, Fact, Stmt, Method> resolverAtCaller, AccessPath.Delta<Field> usedAccessPathOfIncResolver)
		{
			return new ReturnEdge<Field, Fact, Stmt, Method>(incFact, incAccessPath, null, resolverAtCaller, callDelta, usedAccessPathOfIncResolver);
		}

		public override string ToString()
		{
			return string.Format("IncFact: {0}{1}, Delta: {2}, IncResolver: <{3}:{4}>, ResolverAtCallSite: {5}", incFact, incAccessPath, callDelta, usedAccessPathOfIncResolver, incResolver, resolverAtCaller);
		}


	}
}