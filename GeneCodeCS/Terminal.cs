// 
// GeneCodeCS - Genetic programming library for code bot natural selection.
// Copyright (C) 2013 Edd Porter <genecodecs@eddporter.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see {http://www.gnu.org/licenses/}.
//  

using System.Linq;
using System.Reflection;

namespace GeneCodeCS
{
  public sealed class Terminal : IExpression
  {
    public Terminal(MethodInfo mi, object[] parameters) {
      MethodInfo = mi;
      Parameters = parameters;
    }

    public Terminal(MethodInfo mi) : this(mi, new object[] { }) {}

    public MethodInfo MethodInfo { get; private set; }

    public object[] Parameters { get; private set; }

    #region IExpression Members

    public IExpression Clone() {
      return new Terminal(MethodInfo, (object[])Parameters.Clone());
    }

    #endregion

    public static bool operator !=(Terminal a, Terminal b) {
      return !(a == b);
    }

    public static bool operator ==(Terminal a, Terminal b) {
      if (ReferenceEquals(a, b)) {
        return true;
      }
      if (((object)a == null) || ((object)b == null)) {
        return false;
      }
      return a.Equals(b);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      return ReferenceEquals(this, obj) || Equals(obj as Terminal);
    }

    public bool Equals(Terminal value) {
      if (ReferenceEquals(null, value)) {
        return false;
      }
      if (ReferenceEquals(this, value)) {
        return true;
      }
      return Equals(value.MethodInfo, MethodInfo) && Parameters.Zip(value.Parameters, Equals).All(p => p);
    }

    /// <summary>
    ///   Serves as a hash function for a particular type.
    /// </summary>
    /// <returns> A hash code for the current see cref="T:System.Object"/>. </returns>
    /// <filterpriority>2</filterpriority>
    public override int GetHashCode() {
      unchecked {
        return ((MethodInfo != null ? MethodInfo.GetHashCode() : 0) * 397) ^
               (Parameters != null ? Parameters.GetHashCode() : 0);
      }
    }

    public override string ToString() {
      return string.Format("{0}({1})", MethodInfo.Name, string.Join(", ", Parameters));
    }
  }
}