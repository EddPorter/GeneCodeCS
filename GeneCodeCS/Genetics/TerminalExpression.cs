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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeneCodeCS.Properties;

namespace GeneCodeCS.Genetics
{
  /// <summary>
  ///   Represents a concrete implementation of a <see cref="TerminalGene" /> . This provides an action-taking mechanism at the leaf of the expression tree.
  /// </summary>
  public sealed class TerminalExpression : TerminalGene, IGeneExpression, IEquatable<TerminalExpression>
  {
    /// <summary>
    ///   Initialises a new instance of the <see cref="TerminalExpression" /> class to represent the specified method.
    /// </summary>
    /// <param name="mi"> The method used by this <see cref="TerminalExpression" /> object. </param>
    /// <param name="parameters"> The parameters to be passed to the method during evaluation. The correct number of parameters must be provided for the method. </param>
    public TerminalExpression(MethodInfo mi, ICollection<object> parameters) : base(mi) {
      if (parameters == null) {
        throw new ArgumentNullException("parameters");
      }
      if (parameters.Count < mi.GetParameters().Count(p => (p.Attributes & ParameterAttributes.HasDefault) == 0)) {
        throw new ArgumentException(Resources.MethodParametersRequired, "mi");
      }

      Parameters = parameters.Take(mi.GetParameters().Length).ToArray();
    }

    /// <summary>
    ///   Initialises a new instance of the <see cref="TerminalExpression" /> class to represent the specified parameter-less method.
    /// </summary>
    /// <param name="mi"> The method used by this <see cref="TerminalExpression" /> object. </param>
    public TerminalExpression(MethodInfo mi) : this(mi, new object[] { }) { }

    /// <summary>
    ///   Gets the parameters to be passed to the method during evaluation.
    /// </summary>
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
      if (value == null) {
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
    public IGeneExpression Clone() { return new TerminalExpression(MethodInfo, (object[])Parameters.Clone()); }

    /// <summary>
    ///   Determines whether this instance and a specified <see cref="IGeneExpression" /> , which must also be a <see
    ///    cref="TerminalExpression" /> object, have the same value.
    /// </summary>
    /// <param name="obj"> The gene expression to compare to this instance. </param>
    /// <returns> true if <paramref name="obj" /> is a <see cref="TerminalExpression" /> and its value is the same as this instance; otherwise, false. </returns>
    public bool Equals(IGeneExpression obj) { return Equals(obj as TerminalExpression); }

    #endregion

    /// <summary>
    ///   Determines whether two specified <see cref="TerminalExpression" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first terminal expression to compare, or null. </param>
    /// <param name="b"> The second terminal expression to compare, or null. </param>
    /// <returns> true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool Equals(TerminalExpression a, TerminalExpression b) {
      if (a == b) {
        return true;
      }
      if (a == null || b == null) {
        return false;
      }
      return Equals(a.MethodInfo, b.MethodInfo) && a.Parameters.Zip(b.Parameters, Equals).All(p => p);
    }

    /// <summary>
    ///   Determines whether two specified <see cref="TerminalExpression" /> objects have different values.
    /// </summary>
    /// <param name="a"> The first <see cref="TerminalExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="TerminalExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator !=(TerminalExpression a, TerminalExpression b) { return !(a == b); }

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
      if ((object)a == null || (object)b == null) {
        return false;
      }
      return Equals(a.MethodInfo, b.MethodInfo) && a.Parameters.Zip(b.Parameters, Equals).All(p => p);
    }

    /// <summary>
    ///   Determines whether this instance and a specified object, which must also be a <see cref="TerminalExpression" /> object, have the same value.
    /// </summary>
    /// <param name="obj"> The terminal expression to compare to this instance. </param>
    /// <returns> true if <paramref name="obj" /> is a <see cref="TerminalExpression" /> and its value is the same as this instance; otherwise, false. </returns>
    public override bool Equals(object obj) {
      var teB = obj as TerminalExpression;
      if (teB == null) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return Equals(MethodInfo, teB.MethodInfo) && Parameters.Zip(teB.Parameters, Equals).All(p => p);
    }

    /// <summary>
    ///   Returns the hash code for this terminal expression.
    /// </summary>
    /// <returns> A 32-bit signed integer hash code. </returns>
    public override int GetHashCode() {
      unchecked {
        return (MethodInfo.GetHashCode() * 397) ^ Parameters.GetHashCode();
      }
    }

    /// <summary>
    ///   Creates a string representation of this terminal expression.
    /// </summary>
    /// <returns> A string representation of this object. </returns>
    public override string ToString() { return string.Format("{0}({1})", MethodInfo.Name, string.Join(", ", Parameters)); }
  }
}