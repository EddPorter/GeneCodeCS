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
using System.Text;

namespace GeneCodeCS
{
  public sealed class Sequence : IExpression
  {
    public Sequence(ExpressionTree[] expressions) {
      Expressions = expressions;
    }

    public ExpressionTree[] Expressions { get; private set; }

    public static bool operator !=(Sequence left, Sequence right) {
      return !Equals(left, right);
    }

    public static bool operator ==(Sequence left, Sequence right) {
      return Equals(left, right);
    }

    public IExpression Clone() {
      return new Sequence(Expressions.Select(e => e.Clone()).ToArray());
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj is Sequence && Equals((Sequence)obj);
    }

    public bool Equals(Sequence other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }
      return ReferenceEquals(this, other) || Equals(other.Expressions, Expressions);
    }

    public override int GetHashCode() {
      return (Expressions != null ? Expressions.GetHashCode() : 0);
    }

    public override string ToString() {
      var output = new StringBuilder();
      output.AppendLine("Sequence:");
      foreach (var tree in Expressions) {
        var left = tree.ToString();
        var first = true;
        foreach (var line in left.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)) {
          if (first) {
            output.Append("* ");
            first = false;
          } else {
            output.Append("  ");
          }
          output.AppendLine(line);
        }
      }
      return output.ToString().TrimEnd();
    }
  }
}