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
	public class ReturnEdge<Field, Fact, Stmt, Method>
	{

		public readonly Fact incFact;
		public readonly Resolver<Field, Fact, Stmt, Method> resolverAtCaller;
		public readonly AccessPath<Field>.Delta<Field> callDelta;
		public readonly AccessPath<Field> incAccessPath;
		public readonly Resolver<Field, Fact, Stmt, Method> incResolver;
		public readonly AccessPath<Field>.Delta<Field> usedAccessPathOfIncResolver;

		public ReturnEdge(WrappedFact<Field, Fact, Stmt, Method> fact, Resolver<Field, Fact, Stmt, Method> resolverAtCaller, AccessPath<Field>.Delta<Field> callDelta) : this(fact.Fact, fact.AccessPath, fact.Resolver, resolverAtCaller, callDelta, AccessPath<Field>.Delta<Field>.empty<Field>())
		{
		}

		private ReturnEdge(Fact incFact, AccessPath<Field> incAccessPath, Resolver<Field, Fact, Stmt, Method> incResolver, Resolver<Field, Fact, Stmt, Method> resolverAtCaller, AccessPath<Field>.Delta<Field> callDelta, AccessPath<Field>.Delta<Field> usedAccessPathOfIncResolver)
		{
			this.incFact = incFact;
			this.incAccessPath = incAccessPath;
			this.incResolver = incResolver;
			this.resolverAtCaller = resolverAtCaller;
			this.callDelta = callDelta;
			this.usedAccessPathOfIncResolver = usedAccessPathOfIncResolver;
		}

		public virtual ReturnEdge<Field, Fact, Stmt, Method> copyWithIncomingResolver(Resolver<Field, Fact, Stmt, Method> incResolver, AccessPath<Field> .Delta<Field> usedAccessPathOfIncResolver)
		{
			return new ReturnEdge<Field, Fact, Stmt, Method>(incFact, incAccessPath, incResolver, resolverAtCaller, callDelta, usedAccessPathOfIncResolver);
		}

		public virtual ReturnEdge<Field, Fact, Stmt, Method> copyWithResolverAtCaller(Resolver<Field, Fact, Stmt, Method> resolverAtCaller, AccessPath<Field>.Delta<Field> usedAccessPathOfIncResolver)
		{
			return new ReturnEdge<Field, Fact, Stmt, Method>(incFact, incAccessPath, null, resolverAtCaller, callDelta, usedAccessPathOfIncResolver);
		}

		public override string ToString()
		{
			return string.Format("IncFact: {0}{1}, Delta: {2}, IncResolver: <{3}:{4}>, ResolverAtCallSite: {5}", incFact, incAccessPath, callDelta, usedAccessPathOfIncResolver, incResolver, resolverAtCaller);
		}


	}
}