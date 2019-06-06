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

	public class ZeroCallEdgeResolver<Field, Fact, Stmt, Method> : CallEdgeResolver<Field, Fact, Stmt, Method>
	{

		private ZeroHandler<Field> zeroHandler;

		public ZeroCallEdgeResolver(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, ZeroHandler<Field> zeroHandler, Debugger<Field, Fact, Stmt, Method> debugger) : base(analyzer, debugger)
		{
			this.zeroHandler = zeroHandler;
		}

		internal virtual ZeroCallEdgeResolver<Field, Fact, Stmt, Method> copyWithAnalyzer(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer)
		{
			return new ZeroCallEdgeResolver<Field, Fact, Stmt, Method>(analyzer, zeroHandler, debugger);
		}

		public override void resolve(FlowFunction_Constraint<Field> constraint, InterestCallback<Field, Fact, Stmt, Method> callback)
		{
			if (zeroHandler.shouldGenerateAccessPath(constraint.applyToAccessPath(new AccessPath<Field>())))
			{
				callback.interest(analyzer, this);
			}
		}

		public override void interest(Resolver<Field, Fact, Stmt, Method> resolver)
		{
		}

		protected internal override ZeroCallEdgeResolver<Field, Fact, Stmt, Method> getOrCreateNestedResolver(AccessPath<Field> newAccPath)
		{
			return this;
		}

		public override string ToString()
		{
			return "[0-Resolver" + base.ToString() + "]";
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((zeroHandler == null) ? 0 : zeroHandler.GetHashCode());
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj == null)
			{
				return false;
			}
			if (this.GetType() != obj.GetType())
			{
				return false;
			}
			ZeroCallEdgeResolver other = (ZeroCallEdgeResolver) obj;
			if (zeroHandler == null)
			{
				if (other.zeroHandler != null)
				{
					return false;
				}
			}
			else if (!zeroHandler.Equals(other.zeroHandler))
			{
				return false;
			}
			return true;
		}
	}

}