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

	public class AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private AccessPath<System.Reflection.FieldInfo> accessPath;
		private Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver;
		private Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger;

		public AccessPathHandler(AccessPath<System.Reflection.FieldInfo> accessPath, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger)
		{
			this.accessPath = accessPath;
			this.resolver = resolver;
			this.debugger = debugger;
		}

		public virtual bool canRead(System.Reflection.FieldInfo field)
		{
			return accessPath.canRead(field);
		}

		public virtual bool mayCanRead(System.Reflection.FieldInfo field)
		{
			return accessPath.canRead(field) || (accessPath.hasEmptyAccessPath() && !accessPath.isAccessInExclusions(field));
		}

		public virtual bool mayBeEmpty()
		{
			return accessPath.hasEmptyAccessPath();
		}

		public virtual FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> generate(Fact fact)
		{
			return new FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, accessPath, resolver));
		}

		public virtual FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> generateWithEmptyAccessPath(Fact fact, ZeroHandler<System.Reflection.FieldInfo> zeroHandler)
		{
			return new FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, new AccessPath<System.Reflection.FieldInfo>(), new ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(resolver.analyzer, zeroHandler, debugger)));
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public ResultBuilder<Field, Fact, Stmt, Method> prepend(final Field field)
		public virtual ResultBuilder<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> prepend(System.Reflection.FieldInfo field)
		{
			return new ResultBuilderAnonymousInnerClass(this, field);
		}

		private class ResultBuilderAnonymousInnerClass : ResultBuilder<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			private System.Reflection.FieldInfo field;

			public ResultBuilderAnonymousInnerClass(AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance, System.Reflection.FieldInfo field)
			{
				this.outerInstance = outerInstance;
				this.field = field;
			}

			public FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> generate(Fact fact)
			{
				return new FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, outerInstance.accessPath.prepend(field), outerInstance.resolver));
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public ResultBuilder<Field, Fact, Stmt, Method> read(final Field field)
		public virtual ResultBuilder<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> read(System.Reflection.FieldInfo field)
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

		private class ResultBuilderAnonymousInnerClass2 : ResultBuilder<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			private System.Reflection.FieldInfo field;

			public ResultBuilderAnonymousInnerClass2(AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance, System.Reflection.FieldInfo field)
			{
				this.outerInstance = outerInstance;
				this.field = field;
			}

			public FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> generate(Fact fact)
			{
				if (outerInstance.canRead(field))
				{
					return new FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, outerInstance.accessPath.removeFirst(), outerInstance.resolver));
				}
				else
				{
					return new FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, new AccessPath<System.Reflection.FieldInfo>(), outerInstance.resolver), new FlowFunction_ReadFieldConstraint<System.Reflection.FieldInfo>(field));
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public ResultBuilder<Field, Fact, Stmt, Method> overwrite(final Field field)
		public virtual ResultBuilder<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> overwrite(System.Reflection.FieldInfo field)
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

		private class ResultBuilderAnonymousInnerClass3 : ResultBuilder<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			private System.Reflection.FieldInfo field;

			public ResultBuilderAnonymousInnerClass3(AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance, System.Reflection.FieldInfo field)
			{
				this.outerInstance = outerInstance;
				this.field = field;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public heros.fieldsens.FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> generate(Fact fact)
			public override FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> generate(Fact fact)
			{
				if (outerInstance.accessPath.isAccessInExclusions(field))
				{
					return new FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, outerInstance.accessPath, outerInstance.resolver));
				}
				else
				{
					return new FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, outerInstance.accessPath.appendExcludedFieldReference(field), outerInstance.resolver), new FlowFunction_WriteFieldConstraint<System.Reflection.FieldInfo>(field));
				}
			}
		}

		public interface ResultBuilder<FieldRef, FactAbstraction, Stmt, System.Reflection.MethodInfo>
		{
			FlowFunction_ConstrainedFact<FieldRef, FactAbstraction, Stmt, System.Reflection.MethodInfo> generate(FactAbstraction fact);
		}

	}

}