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
using System.Reflection;
using System.Text;

namespace GeneCodeCS
{
  /// <summary>
  ///   The branch.
  /// </summary>
  internal sealed class BranchInstance : Branch, IExpressionInstance, IEquatable<BranchInstance>
  {
    /// <summary>
    ///   Initialises a new instance of the <see cref="T:GeneCodeCS.BranchInstance" /> class to represent the specified parameter-less method.
    /// </summary>
    /// <param name="mi"> The method used by this <see cref="T:GeneCodeCS.BranchInstance" /> object. </param>
    /// <param name="trueBranch"> </param>
    /// <param name="falseBranch"> </param>
    public BranchInstance(MethodInfo mi, ExpressionTree trueBranch, ExpressionTree falseBranch)
      : this(mi, new object[] {}, trueBranch, falseBranch) {
    }

    /// <summary>
    ///   Initialises a new instance of the <see cref="T:GeneCodeCS.BranchInstance" /> class.
    /// </summary>
    /// <param name="mi"> The method used by this <see cref="T:GeneCodeCS.BranchInstance" /> object. </param>
    /// <param name="parameters"> The parameters to be passed to the method during evaluation. </param>
    /// <param name="trueBranch"> </param>
    /// <param name="falseBranch"> </param>
    public BranchInstance(MethodInfo mi, object[] parameters, ExpressionTree trueBranch, ExpressionTree falseBranch)
      : base(mi) {
      if (parameters == null) {
        throw new ArgumentNullException("parameters");
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
    /// <remarks>Not null.</remarks>
    public ExpressionTree FalseBranch { get; internal set; }

    /// <summary>
    ///   Gets the expression tree to evaluate if the method evaluation returns <code>true</code> .
    /// </summary>
    /// <remarks>Not null.</remarks>
    public ExpressionTree TrueBranch { get; internal set; }

    /// <summary>
    ///   Gets the parameters to be passed to the method during evaluation.
    /// </summary>
    /// <remarks>Not null.</remarks>
    public object[] Parameters { get; private set; }

    #region IEquatable<BranchInstance> Members

    /// <summary>
    ///   The equals.
    /// </summary>
    /// <param name="other"> The other. </param>
    /// <returns> The <see cref="bool" /> . </returns>
    public bool Equals(BranchInstance other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      if (ReferenceEquals(this, other)) {
        return true;
      }

      return Equals(other.TrueBranch, TrueBranch) && Equals(other.MethodInfo, MethodInfo) &&
             Equals(other.Parameters, Parameters) && Equals(other.FalseBranch, FalseBranch);
    }

    #endregion

    #region IExpressionInstance Members

    /// <summary>
    ///   The clone.
    /// </summary>
    /// <returns> The <see cref="IExpressionInstance" /> . </returns>
    public IExpressionInstance Clone() {
      return new BranchInstance(MethodInfo, (object[])Parameters.Clone(), TrueBranch = TrueBranch.Clone(),
                                FalseBranch = FalseBranch.Clone());
    }

    #endregion

    /// <summary>
    ///   Determines whether two specified <see cref="T:GeneCodeCS.BranchInstance" /> objects have different values.
    /// </summary>
    /// <param name="a"> The first <see cref="T:GeneCodeCS.BranchInstance" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="T:GeneCodeCS.BranchInstance" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator !=(BranchInstance a, BranchInstance b) {
      return !Equals(a, b);
    }

    /// <summary>
    ///   Determines whether two specified <see cref="T:GeneCodeCS.BranchInstance" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first <see cref="T:GeneCodeCS.BranchInstance" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="T:GeneCodeCS.BranchInstance" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator ==(BranchInstance a, BranchInstance b) {
      return Equals(a, b);
    }

    /// <summary>
    ///   The equals.
    /// </summary>
    /// <param name="obj"> The obj. </param>
    /// <returns> The <see cref="bool" /> . </returns>
    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }

      return ReferenceEquals(this, obj) || Equals(obj as BranchInstance);
    }

    /// <summary>
    ///   The get hash code.
    /// </summary>
    /// <returns> The <see cref="int" /> . </returns>
    public override int GetHashCode() {
      unchecked {
        var result = TrueBranch.GetHashCode();
        result = (result * 397) ^ MethodInfo.GetHashCode();
        result = (result * 397) ^ Parameters.GetHashCode();
        return (result * 397) ^ FalseBranch.GetHashCode();
      }
    }

    /// <summary>
    ///   The to string.
    /// </summary>
    /// <returns> The <see cref="string" /> . </returns>
    public override string ToString() {
      var output = new StringBuilder();
      output.AppendFormat("{0}({1}){2}", MethodInfo.Name, string.Join(", ", Parameters), Environment.NewLine);
      BranchToString(output, TrueBranch, "T");
      BranchToString(output, FalseBranch, "F");
      return output.ToString().TrimEnd();
    }

    /// <summary>
    ///   The branch to string.
    /// </summary>
    /// <param name="output"> The output. </param>
    /// <param name="branch"> The branch. </param>
    /// <param name="prefix"> The prefix. </param>
    private static void BranchToString(StringBuilder output, ExpressionTree branch, string prefix) {
      var branchString = branch.ToString();
      var first = true;
      foreach (var line in branchString.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries)) {
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