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

using System;
using System.Linq;
using System.Reflection;

namespace GeneCodeCS.Genetics
{
  public sealed class TerminalExpression : TerminalGene, IGeneExpression, IEquatable<TerminalExpression>
  {
    public TerminalExpression(MethodInfo mi, object[] parameters) : base(mi) {
      if (parameters == null) {
        throw new ArgumentNullException("parameters");
      }

      Parameters = parameters;
    }

    public TerminalExpression(MethodInfo mi) : this(mi, new object[] { }) {
    }

    /// <remarks>
    ///   Not null.
    /// </remarks>
    public object[] Parameters { get; private set; }

    #region IEquatable<TerminalExpression> Members

    /// <summary>
    ///   Determines whether this instance and another specified <see cref="TerminalExpression" /> object have the same value.
    /// </summary>
    /// <returns> <c>true</c> if the value of the <paramref name="value" /> parameter is the same as this instance; otherwise, false. </returns>
    /// <param name="value"> The terminal to compare to this instance. </param>
    public bool Equals(TerminalExpression value) {
      if (ReferenceEquals(null, value)) {
        return false;
      }
      if (ReferenceEquals(this, value)) {
        return true;
      }
      return Equals(value.MethodInfo, MethodInfo) && Parameters.Zip(value.Parameters, Equals).All(p => p);
    }

    #endregion

    #region IGeneExpression Members

    /// <summary>
    ///   Performs a deep copy of this <see cref="TerminalExpression" /> and returns the result.
    /// </summary>
    /// <returns> The copied object. </returns>
    public IGeneExpression Clone() {
      return new TerminalExpression(MethodInfo, (object[])Parameters.Clone());
    }

    #endregion

    /// <summary>
    ///   Determines whether two specified <see cref="TerminalExpression" /> objects have different values.
    /// </summary>
    /// <param name="a"> The first <see cref="TerminalExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="TerminalExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator !=(TerminalExpression a, TerminalExpression b) {
      return !(a == b);
    }

    /// <summary>
    ///   Determines whether two specified <see cref="TerminalExpression" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first <see cref="TerminalExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="TerminalExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator ==(TerminalExpression a, TerminalExpression b) {
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
      return ReferenceEquals(this, obj) || Equals(obj as TerminalExpression);
    }

    /// <summary>
    ///   Serves as a hash function for a particular type.
    /// </summary>
    /// <returns> A hash code for the current see cref="T:System.Object"/>. </returns>
    /// <filterpriority>2</filterpriority>
    public override int GetHashCode() {
      unchecked {
        return (MethodInfo.GetHashCode() * 397) ^ Parameters.GetHashCode();
      }
    }

    public override string ToString() {
      return string.Format("{0}({1})", MethodInfo.Name, string.Join(", ", Parameters));
    }
  }
}