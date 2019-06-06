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
	using WrappedFact = heros.fieldsens.structs.WrappedFact;

	public class AccessPathHandler<Field, Fact, Stmt, Method>
	{

		private AccessPath<Field> accessPath;
		private Resolver<Field, Fact, Stmt, Method> resolver;
		private Debugger<Field, Fact, Stmt, Method> debugger;

		public AccessPathHandler(AccessPath<Field> accessPath, Resolver<Field, Fact, Stmt, Method> resolver, Debugger<Field, Fact, Stmt, Method> debugger)
		{
			this.accessPath = accessPath;
			this.resolver = resolver;
			this.debugger = debugger;
		}

		public virtual bool canRead(Field field)
		{
			return accessPath.canRead(field);
		}

		public virtual bool mayCanRead(Field field)
		{
			return accessPath.canRead(field) || (accessPath.hasEmptyAccessPath() && !accessPath.isAccessInExclusions(field));
		}

		public virtual bool mayBeEmpty()
		{
			return accessPath.hasEmptyAccessPath();
		}

		public virtual FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> generate(Fact fact)
		{
			return new FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>(new WrappedFact<Field, Fact, Stmt, Method>(fact, accessPath, resolver));
		}

		public virtual FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> generateWithEmptyAccessPath(Fact fact, ZeroHandler<Field> zeroHandler)
		{
			return new FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>(new WrappedFact<Field, Fact, Stmt, Method>(fact, new AccessPath<Field>(), new ZeroCallEdgeResolver<Field, Fact, Stmt, Method>(resolver.analyzer, zeroHandler, debugger)));
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public ResultBuilder<Field, Fact, Stmt, Method> prepend(final Field field)
		public virtual ResultBuilder<Field, Fact, Stmt, Method> prepend(Field field)
		{
			return new ResultBuilderAnonymousInnerClass(this, field);
		}

		private class ResultBuilderAnonymousInnerClass : ResultBuilder<Field, Fact, Stmt, Method>
		{
			private readonly AccessPathHandler<Field, Fact, Stmt, Method> outerInstance;

			private Field field;

			public ResultBuilderAnonymousInnerClass(AccessPathHandler<Field, Fact, Stmt, Method> outerInstance, Field field)
			{
				this.outerInstance = outerInstance;
				this.field = field;
			}

			public FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> generate(Fact fact)
			{
				return new FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>(new WrappedFact<Field, Fact, Stmt, Method>(fact, outerInstance.accessPath.prepend(field), outerInstance.resolver));
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public ResultBuilder<Field, Fact, Stmt, Method> read(final Field field)
		public virtual ResultBuilder<Field, Fact, Stmt, Method> read(Field field)
		{
			if (mayCanRead(field))
			{
				return new ResultBuilderAnonymousInnerClass2(this, field);
			}
			else
			{
				throw new System.ArgumentException("Cannot read field " + field);
			}
		}

		private class ResultBuilderAnonymousInnerClass2 : ResultBuilder<Field, Fact, Stmt, Method>
		{
			private readonly AccessPathHandler<Field, Fact, Stmt, Method> outerInstance;

			private Field field;

			public ResultBuilderAnonymousInnerClass2(AccessPathHandler<Field, Fact, Stmt, Method> outerInstance, Field field)
			{
				this.outerInstance = outerInstance;
				this.field = field;
			}

			public FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> generate(Fact fact)
			{
				if (outerInstance.canRead(field))
				{
					return new FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>(new WrappedFact<Field, Fact, Stmt, Method>(fact, outerInstance.accessPath.removeFirst(), outerInstance.resolver));
				}
				else
				{
					return new FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>(new WrappedFact<Field, Fact, Stmt, Method>(fact, new AccessPath<Field>(), outerInstance.resolver), new FlowFunction_ReadFieldConstraint<Field>(field));
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public ResultBuilder<Field, Fact, Stmt, Method> overwrite(final Field field)
		public virtual ResultBuilder<Field, Fact, Stmt, Method> overwrite(Field field)
		{
			if (mayBeEmpty())
			{
				return new ResultBuilderAnonymousInnerClass3(this, field);
			}
			else
			{
				throw new System.ArgumentException("Cannot write field " + field);
			}
		}

		private class ResultBuilderAnonymousInnerClass3 : ResultBuilder<Field, Fact, Stmt, Method>
		{
			private readonly AccessPathHandler<Field, Fact, Stmt, Method> outerInstance;

			private Field field;

			public ResultBuilderAnonymousInnerClass3(AccessPathHandler<Field, Fact, Stmt, Method> outerInstance, Field field)
			{
				this.outerInstance = outerInstance;
				this.field = field;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public heros.fieldsens.FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> generate(Fact fact)
			public override FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> generate(Fact fact)
			{
				if (outerInstance.accessPath.isAccessInExclusions(field))
				{
					return new FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>(new WrappedFact<Field, Fact, Stmt, Method>(fact, outerInstance.accessPath, outerInstance.resolver));
				}
				else
				{
					return new FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>(new WrappedFact<Field, Fact, Stmt, Method>(fact, outerInstance.accessPath.appendExcludedFieldReference(field), outerInstance.resolver), new FlowFunction_WriteFieldConstraint<Field>(field));
				}
			}
		}

		public interface ResultBuilder<FieldRef, FactAbstraction, Stmt, Method>
		{
			FlowFunction_ConstrainedFact<FieldRef, FactAbstraction, Stmt, Method> generate(FactAbstraction fact);
		}

	}

}