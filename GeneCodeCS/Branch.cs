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
  public sealed class Branch : IExpression
  {
    public Branch(MethodInfo mi) : this(mi, new object[] { }) {}

    public Branch(MethodInfo mi, object[] parameters) {
      MethodInfo = mi;
      Parameters = parameters;
    }

    public ExpressionTree Left { get; set; }

    public MethodInfo MethodInfo { get; private set; }

    public object[] Parameters { get; private set; }

    public ExpressionTree Right { get; set; }

    #region IExpression Members

    public IExpression Clone() {
      var newExpression = new Branch(MethodInfo) { Parameters = (object[])Parameters.Clone() };
      if (Left != null) {
        newExpression.Left = Left.Clone();
      }
      if (Right != null) {
        newExpression.Right = Right.Clone();
      }
      return newExpression;
    }

    #endregion

    public static bool operator !=(Branch left, Branch right) {
      return !Equals(left, right);
    }

    public static bool operator ==(Branch left, Branch right) {
      return Equals(left, right);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      return ReferenceEquals(this, obj) || Equals(obj as Branch);
    }

    public bool Equals(Branch other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      return Equals(other.Left, Left) && Equals(other.MethodInfo, MethodInfo) && Equals(other.Parameters, Parameters) &&
             Equals(other.Right, Right);
    }

    public override int GetHashCode() {
      unchecked {
        var result = (Left != null ? Left.GetHashCode() : 0);
        result = (result * 397) ^ (MethodInfo != null ? MethodInfo.GetHashCode() : 0);
        result = (result * 397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
        return (result * 397) ^ (Right != null ? Right.GetHashCode() : 0);
      }
    }

    public override string ToString() {
      var output = new StringBuilder();
      output.AppendFormat("{0}({1}){2}", MethodInfo.Name, string.Join(", ", Parameters), Environment.NewLine);
      BranchToString(output, Left, "T");
      BranchToString(output, Right, "F");
      return output.ToString().TrimEnd();
    }

    private static void BranchToString(StringBuilder output, ExpressionTree branch, string prefix) {
      if (branch == null) {
        return;
      }
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