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
using System.Text;
using GeneCodeCS.Properties;

namespace GeneCodeCS.Genetics
{
  /// <summary>
  ///   Represents a concrete implementation of a <see cref="BranchGene" /> . This provides a branching mechanism (if-then-else) in the execution tree.
  /// </summary>
  public sealed class BranchExpression : BranchGene, IGeneExpression, IEquatable<BranchExpression>
  {
    /// <summary>
    ///   Initialises a new instance of the <see cref="BranchExpression" /> class to represent the specified parameter-less method.
    /// </summary>
    /// <param name="mi"> The method used by this <see cref="BranchExpression" /> object. </param>
    /// <param name="trueBranch"> The <see cref="Chromosome" /> tree to execute if the <paramref name="mi" /> method returns <c>true</c> . </param>
    /// <param name="falseBranch"> The <see cref="Chromosome" /> tree to execute if the <paramref name="mi" /> method returns <c>false</c> . </param>
    public BranchExpression(MethodInfo mi, Chromosome trueBranch, Chromosome falseBranch)
      : this(mi, new object[] { }, trueBranch, falseBranch) { }

    /// <summary>
    ///   Initialises a new instance of the <see cref="BranchExpression" /> class to represent the specified method.
    /// </summary>
    /// <param name="mi"> The method used by this <see cref="BranchExpression" /> object. </param>
    /// <param name="parameters"> The parameters to be passed to the method during evaluation. The correct number of parameters must be provided for the method. </param>
    /// <param name="trueBranch"> The <see cref="Chromosome" /> tree to execute if the <paramref name="mi" /> method returns <c>true</c> . </param>
    /// <param name="falseBranch"> The <see cref="Chromosome" /> tree to execute if the <paramref name="mi" /> method returns <c>false</c> . </param>
    public BranchExpression(MethodInfo mi, object[] parameters, Chromosome trueBranch, Chromosome falseBranch)
      : base(mi) {
      if (parameters == null) {
        throw new ArgumentNullException("parameters");
      }
      if (parameters.Length < mi.GetParameters().Count(p => p.DefaultValue != null)) {
        throw new ArgumentException(Resources.MethodParametersRequired, "mi");
      }
      if (trueBranch == null) {
        throw new ArgumentNullException("trueBranch");
      }
      if (falseBranch == null) {
        throw new ArgumentNullException("falseBranch");
      }

      Parameters = parameters;
      TrueBranch = trueBranch;
      FalseBranch = falseBranch;
    }

    /// <summary>
    ///   Gets the expression tree to evaluate if the method evaluation returns <code>false</code> .
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public Chromosome FalseBranch { get; set; }

    /// <summary>
    ///   Gets the parameters to be passed to the method during evaluation.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public object[] Parameters { get; private set; }

    /// <summary>
    ///   Gets the expression tree to evaluate if the method evaluation returns <code>true</code> .
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public Chromosome TrueBranch { get; set; }

    #region IEquatable<BranchExpression> Members

    /// <summary>
    ///   Determines whether this instance and another specified <see cref="BranchExpression" /> object have the same value.
    /// </summary>
    /// <returns> <c>true</c> if the value of the <paramref name="value" /> parameter is the same as this instance; otherwise, false. </returns>
    /// <param name="value"> The branch to compare to this instance. </param>
    public bool Equals(BranchExpression value) {
      if (value == null) {
        return false;
      }
      if (ReferenceEquals(this, value)) {
        return true;
      }
      return Equals(value.TrueBranch, TrueBranch) && Equals(value.MethodInfo, MethodInfo) &&
             Equals(value.Parameters, Parameters) && Equals(value.FalseBranch, FalseBranch);
    }

    #endregion

    #region IGeneExpression Members

    /// <summary>
    ///   Performs a deep copy of this <see cref="BranchExpression" /> and returns the result.
    /// </summary>
    /// <returns> The copied object. </returns>
    public IGeneExpression Clone() {
      return new BranchExpression(MethodInfo, (object[])Parameters.Clone(), TrueBranch = TrueBranch.Clone(),
                                  FalseBranch = FalseBranch.Clone());
    }

    /// <summary>
    ///   Determines whether this instance and a specified <see cref="IGeneExpression" /> , which must also be a <see
    ///    cref="BranchExpression" /> object, have the same value.
    /// </summary>
    /// <param name="obj"> The gene expression to compare to this instance. </param>
    /// <returns> true if <paramref name="obj" /> is a <see cref="BranchExpression" /> and its value is the same as this instance; otherwise, false. </returns>
    public bool Equals(IGeneExpression obj) { return Equals(obj as BranchExpression); }

    #endregion

    /// <summary>
    ///   Determines whether two specified <see cref="BranchExpression" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first branch expression to compare, or null. </param>
    /// <param name="b"> The second branch expression to compare, or null. </param>
    /// <returns> true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool Equals(BranchExpression a, BranchExpression b) {
      if (a == b) {
        return true;
      }
      if (a == null || b == null) {
        return false;
      }
      return Equals(a.TrueBranch, b.TrueBranch) && Equals(a.MethodInfo, b.MethodInfo) &&
             Equals(a.Parameters, b.Parameters) && Equals(a.FalseBranch, b.FalseBranch);
    }

    /// <summary>
    ///   Determines whether two specified <see cref="BranchExpression" /> objects have different values.
    /// </summary>
    /// <param name="a"> The first <see cref="BranchExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="BranchExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator !=(BranchExpression a, BranchExpression b) { return !(a == b); }

    /// <summary>
    ///   Determines whether two specified <see cref="BranchExpression" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first <see cref="BranchExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="BranchExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator ==(BranchExpression a, BranchExpression b) {
      if (ReferenceEquals(a, b)) {
        return true;
      }
      if ((object)a == null || (object)b == null) {
        return false;
      }
      return a.TrueBranch == b.TrueBranch && a.MethodInfo == b.MethodInfo && a.Parameters == b.Parameters &&
             a.FalseBranch == b.FalseBranch;
    }

    /// <summary>
    ///   Determines whether this instance and a specified object, which must also be a <see cref="BranchExpression" /> object, have the same value.
    /// </summary>
    /// <param name="obj"> The branch expression to compare to this instance. </param>
    /// <returns> true if <paramref name="obj" /> is a <see cref="BranchExpression" /> and its value is the same as this instance; otherwise, false. </returns>
    public override bool Equals(object obj) {
      var beB = obj as BranchExpression;
      if (beB == null) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return Equals(TrueBranch, beB.TrueBranch) && Equals(MethodInfo, beB.MethodInfo) &&
             Equals(Parameters, beB.Parameters) && Equals(FalseBranch, beB.FalseBranch);
    }

    /// <summary>
    ///   Returns the hash code for this branch expression.
    /// </summary>
    /// <returns> A 32-bit signed integer hash code. </returns>
    public override int GetHashCode() {
      unchecked {
        var result = TrueBranch.GetHashCode();
        result = (result * 397) ^ MethodInfo.GetHashCode();
        result = (result * 397) ^ Parameters.GetHashCode();
        return (result * 397) ^ FalseBranch.GetHashCode();
      }
    }

    /// <summary>
    ///   Creates a string representation of this branch expression and its descendant tree structures.
    /// </summary>
    /// <returns> A string representation of this object. </returns>
    public override string ToString() {
      var output = new StringBuilder();
      output.AppendFormat("{0}({1}){2}", MethodInfo.Name, string.Join(", ", Parameters), Environment.NewLine);
      BranchToString(output, TrueBranch, "T");
      BranchToString(output, FalseBranch, "F");
      return output.ToString().TrimEnd();
    }

    /// <summary>
    ///   Helper method for converting a chromosome branch to a string representation.
    /// </summary>
    /// <param name="output"> The string builder object to fill. </param>
    /// <param name="branch"> The chromosome branch to convert to a string. </param>
    /// <param name="prefix"> The branch prefix: usually either 'T' or 'F'. </param>
    private static void BranchToString(StringBuilder output, Chromosome branch, string prefix) {
      var branchString = branch.ToString();
      var first = true;
      foreach (var line in branchString.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)) {
        if (first) {
          output.Append(prefix);
          output.Append(" ");
          first = false;
        } else {
          output.Append("| ");
        }

        output.AppendLine(line);
      }
    }
  }
}