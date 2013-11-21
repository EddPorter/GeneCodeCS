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
  /// <summary>
  ///   Represents a concrete implementation of a <see cref="SequenceGene" /> . This provides a mechanism for executing a sequence of instructions serially.
  /// </summary>
  public sealed class SequenceExpression : SequenceGene, IGeneExpression, IEquatable<SequenceExpression>
  {
    /// <summary>
    ///   Initialises a new instance of the <see cref="SequenceExpression" /> class to represent calling the specified expressions in order.
    /// </summary>
    /// <param name="expressions"> The expressions to sequence. </param>
    public SequenceExpression(Chromosome[] expressions) : base(expressions.Length) {
      if (expressions == null) {
        throw new ArgumentNullException("expressions");
      }

      Expressions = expressions;
    }

    /// <summary>
    ///   Gets the chromosome expressions in the sequence.
    /// </summary>
    /// <remarks>
    ///   Not null.
    /// </remarks>
    public Chromosome[] Expressions { get; set; }

    #region IEquatable<SequenceExpression> Members

    /// <summary>
    ///   Determines whether this instance and another specified <see cref="SequenceExpression" /> object have the same value.
    /// </summary>
    /// <returns> <c>true</c> if the value of the <paramref name="value" /> parameter is the same as this instance; otherwise, false. </returns>
    /// <param name="value"> The sequence to compare to this instance. </param>
    public bool Equals(SequenceExpression value) {
      if (value == null) {
        return false;
      }
      return ReferenceEquals(this, value) || Expressions.Zip(value.Expressions, Equals).All(p => p);
    }

    #endregion

    #region IGeneExpression Members

    /// <summary>
    ///   Performs a deep copy of this <see cref="SequenceExpression" /> and returns the result.
    /// </summary>
    /// <returns> The copied object. </returns>
    public IGeneExpression Clone() { return new SequenceExpression(Expressions.Select(e => e.Clone()).ToArray()); }

    /// <summary>
    ///   Determines whether this instance and a specified <see cref="IGeneExpression" /> , which must also be a <see
    ///    cref="SequenceExpression" /> object, have the same value.
    /// </summary>
    /// <param name="obj"> The gene expression to compare to this instance. </param>
    /// <returns> true if <paramref name="obj" /> is a <see cref="SequenceExpression" /> and its value is the same as this instance; otherwise, false. </returns>
    public bool Equals(IGeneExpression obj) { return Equals(obj as SequenceExpression); }

    #endregion

    /// <summary>
    ///   Determines whether two specified <see cref="SequenceExpression" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first sequence expression to compare, or null. </param>
    /// <param name="b"> The second sequence expression to compare, or null. </param>
    /// <returns> true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool Equals(SequenceExpression a, SequenceExpression b) {
      if (a == b) {
        return true;
      }
      if (a == null || b == null) {
        return false;
      }
      return a.Expressions.Zip(b.Expressions, Equals).All(p => p);
    }

    /// <summary>
    ///   Determines whether two specified <see cref="SequenceExpression" /> objects have different values.
    /// </summary>
    /// <param name="a"> The first <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator !=(SequenceExpression a, SequenceExpression b) { return !(a == b); }

    /// <summary>
    ///   Determines whether two specified <see cref="SequenceExpression" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="SequenceExpression" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator ==(SequenceExpression a, SequenceExpression b) {
      if (ReferenceEquals(a, b)) {
        return true;
      }
      if ((object)a == null || (object)b == null) {
        return false;
      }
      return a.Expressions.Zip(b.Expressions, (l, r) => l == r).All(p => p);
    }

    /// <summary>
    ///   Determines whether this instance and a specified object, which must also be a <see cref="SequenceExpression" /> object, have the same value.
    /// </summary>
    /// <param name="obj"> The sequence expression to compare to this instance. </param>
    /// <returns> true if <paramref name="obj" /> is a <see cref="SequenceExpression" /> and its value is the same as this instance; otherwise, false. </returns>
    public override bool Equals(object obj) {
      var seB = obj as SequenceExpression;
      if (seB == null) {
        return false;
      }
      return ReferenceEquals(this, obj) || Expressions.Zip(seB.Expressions, Equals).All(p => p);
    }

    /// <summary>
    ///   Returns the hash code for this branch expression.
    /// </summary>
    /// <returns> A 32-bit signed integer hash code. </returns>
    public override int GetHashCode() {
      unchecked {
        return Length * 397 ^ Expressions.Aggregate(0, (acc, exp) => acc * 397 ^ exp.GetHashCode());
      }
    }

    /// <summary>
    ///   Creates a string representation of this sequence expression and its descendant tree structures.
    /// </summary>
    /// <returns> A string representation of this object. </returns>
    public override string ToString() {
      var output = new StringBuilder();
      output.AppendLine("[");
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
      output.AppendLine("]");
      return output.ToString().TrimEnd();
    }
  }
}