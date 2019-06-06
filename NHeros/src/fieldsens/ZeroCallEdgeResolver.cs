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

	public class ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : CallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private ZeroHandler<System.Reflection.FieldInfo> zeroHandler;

		public ZeroCallEdgeResolver(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, ZeroHandler<System.Reflection.FieldInfo> zeroHandler, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger) : base(analyzer, debugger)
		{
			this.zeroHandler = zeroHandler;
		}

		internal virtual ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> copyWithAnalyzer(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer)
		{
			return new ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(analyzer, zeroHandler, debugger);
		}

		public override void resolve(FlowFunction_Constraint<System.Reflection.FieldInfo> constraint, InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callback)
		{
			if (zeroHandler.shouldGenerateAccessPath(constraint.applyToAccessPath(new AccessPath<System.Reflection.FieldInfo>())))
			{
				callback.interest(analyzer, this);
			}
		}

		public override void interest(Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
		{
		}

		protected internal override ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> getOrCreateNestedResolver(AccessPath<System.Reflection.FieldInfo> newAccPath)
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