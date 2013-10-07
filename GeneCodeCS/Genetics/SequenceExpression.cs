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

namespace GeneCodeCS.Genetics
{
  public sealed class SequenceExpression : SequenceGene, IGeneExpression, IEquatable<SequenceExpression>
  {
    public SequenceExpression(Chromosome[] expressions) : base(expressions.Length) {
      Expressions = expressions;
    }

    public Chromosome[] Expressions { get; private set; }

    #region IEquatable<SequenceExpression> Members

    /// <summary>
    ///   Determines whether this instance and another specified <see cref="SequenceExpression" /> object have the same value.
    /// </summary>
    /// <returns> <c>true</c> if the value of the <paramref name="value" /> parameter is the same as this instance; otherwise, false. </returns>
    /// <param name="value"> The sequence to compare to this instance. </param>
    public bool Equals(SequenceExpression value) {
      if (ReferenceEquals(null, value)) {
        return false;
      }
      return ReferenceEquals(this, value) || Equals(value.Expressions, Expressions);
    }

    #endregion

    #region IGeneExpression Members

    /// <summary>
    ///   Performs a deep copy of this <see cref="SequenceExpression" /> and returns the result.
    /// </summary>
    /// <returns> The copied object. </returns>
    public IGeneExpression Clone() {
      return new SequenceExpression(Expressions.Select(e => e.Clone()).ToArray());
    }

    #endregion

    /// <summary>
    ///   Determines whether two specified <see cref="SequenceExpression" /> objects have different values.
    /// </summary>
    /// <param name="a"> The first <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator !=(SequenceExpression a, SequenceExpression b) {
      return !Equals(a, b);
    }

    /// <summary>
    ///   Determines whether two specified <see cref="SequenceExpression" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator ==(SequenceExpression a, SequenceExpression b) {
      return Equals(a, b);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj is SequenceExpression && Equals((SequenceExpression)obj);
    }

    public override int GetHashCode() {
      return (Expressions != null ? Expressions.GetHashCode() : 0);
    }

    public override string ToString() {
      var output = new StringBuilder();
      output.AppendLine("SequenceExpression:");
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