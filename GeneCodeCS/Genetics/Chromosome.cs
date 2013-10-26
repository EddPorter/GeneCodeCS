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

namespace GeneCodeCS.Genetics
{
  /// <summary>
  ///   A <see cref="Chromosome" /> forms the brains of a bot and consists of a tree of <see cref="IGeneExpression" /> s representing the bot's behaviour.
  /// </summary>
  public sealed class Chromosome : IEquatable<Chromosome>
  {
    /// <summary>
    ///   Gets the genetic code of the chromosome.
    /// </summary>
    public IGeneExpression Node { get; internal set; }

    /// <summary>
    ///   Gets the parent of this chromosome in the genetic tree.
    /// </summary>
    // TODO: Do we care?
    public Chromosome Parent { get; set; }

    #region IEquatable<Chromosome> Members

    /// <summary>
    ///   Determines whether this instance and another specified <see cref="Chromosome" /> object have the same value. The parent object is not considered during this process.
    /// </summary>
    /// <returns> <c>true</c> if the value of the <paramref name="value" /> parameter is the same as this instance; otherwise, false. </returns>
    /// <param name="value"> The branch to compare to this instance. </param>
    public bool Equals(Chromosome value) {
      if (this == null) {
        throw new NullReferenceException();
      }
      if (value == null) {
        return false;
      }
      if (ReferenceEquals(this, value)) {
        return true;
      }

      return (Node == null && value.Node == null) || (Node != null && Node.Equals(value.Node));
    }

    #endregion

    /// <summary>
    ///   Determines whether two specified <see cref="Chromosome" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first chromosome to compare, or null. </param>
    /// <param name="b"> The second chromosome to compare, or null. </param>
    /// <returns> true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool Equals(Chromosome a, Chromosome b) {
      if (a == b) {
        return true;
      }
      if (a == null || b == null) {
        return false;
      }
      return (a.Node == null && b.Node == null) || (a.Node != null && a.Node.Equals(b.Node));
    }

    /// <summary>
    ///   Determines whether two specified <see cref="Chromosome" /> objects have different values.
    /// </summary>
    /// <param name="a"> The first <see cref="Chromosome" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="Chromosome" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is different from the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator !=(Chromosome a, Chromosome b) {
      return !(a == b);
    }

    /// <summary>
    ///   Determines whether two specified <see cref="Chromosome" /> objects have the same value.
    /// </summary>
    /// <param name="a"> The first <see cref="Chromosome" /> object to compare, or null. </param>
    /// <param name="b"> The second <see cref="Chromosome" /> object to compare, or null. </param>
    /// <returns> <c>true</c> if the value of <paramref name="a" /> is the same as the value of <paramref name="b" /> ; otherwise, false. </returns>
    public static bool operator ==(Chromosome a, Chromosome b) {
      if (ReferenceEquals(a, b)) {
        return true;
      }
      if ((object)a == null || (object)b == null) {
        return false;
      }
      return (a.Node == null && b.Node == null) || (a.Node != null && a.Node.Equals(b.Node));
    }

    /// <summary>
    ///   Creates a new, independent instance of the whole expression tree.
    /// </summary>
    /// <returns> A new, memory-independent copy of this expression tree. </returns>
    public Chromosome Clone() {
      return new Chromosome { Node = Node.Clone(), Parent = Parent };
    }

    /// <summary>
    ///   Determines whether this instance and a specified object, which must also be a <see cref="BranchExpression" /> object, have the same value.
    /// </summary>
    /// <param name="obj"> The branch expression to compare to this instance. </param>
    /// <returns> true if <paramref name="obj" /> is a <see cref="BranchExpression" /> and its value is the same as this instance; otherwise, false. </returns>
    public override bool Equals(object obj) {
      if (this == null) {
        throw new NullReferenceException();
      }
      var cB = obj as Chromosome;
      if (cB == null) {
        return false;
      }
      return ReferenceEquals(this, obj) || (Node == null && cB.Node == null) || (Node != null && Node.Equals(cB.Node));
    }

    /// <summary>
    ///   Returns the hash code for this chromosome.
    /// </summary>
    /// <returns> A 32-bit signed integer hash code. </returns>
    public override int GetHashCode() {
      unchecked {
        return Node != null ? Node.GetHashCode() : 0;
      }
    }

    /// <summary>
    ///   Creates a string representation of this chromosome and its descendant tree structures.
    /// </summary>
    /// <returns> A string representation of this object. </returns>
    public override string ToString() {
      return Node.ToString();
    }
  }
}