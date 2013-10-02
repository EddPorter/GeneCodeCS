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
  ///   A <see cref="T:GeneCodeCS.Genetics.Chromosome" /> forms the brains of a bot and consists of a tree of <see
  ///    cref="T:GeneCodeCS.Genetics.IGeneExpression" />s representing the bot's behaviour.
  /// </summary>
  public sealed class Chromosome : IEquatable<Chromosome>
  {
    public IGeneExpression Node { get; internal set; }

    internal Chromosome Parent { private get; set; }

    #region IEquatable<Chromosome> Members

    public bool Equals(Chromosome value) {
      if (ReferenceEquals(null, value)) {
        return false;
      }
      if (ReferenceEquals(this, value)) {
        return true;
      }
      return Equals(value.Node, Node);
    }

    #endregion

    public static bool operator !=(Chromosome a, Chromosome b) {
      return !(a == b);
    }

    public static bool operator ==(Chromosome a, Chromosome b) {
      if (ReferenceEquals(a, b)) {
        return true;
      }
      if (((object)a == null) || ((object)b == null)) {
        return false;
      }
      return a.Equals(b);
    }

    public static Chromosome ReplaceNodeWithCopy(Chromosome removedNode, Chromosome insertedNode) {
      return new Chromosome {Node = insertedNode.Node, Parent = removedNode.Parent};
    }

    /// <summary>
    ///   Creates a new, independent instance of the whole expression tree.
    /// </summary>
    /// <returns> A new, memory-independent copy of this expression tree. </returns>
    public Chromosome Clone() {
      return new Chromosome {Node = Node.Clone(), Parent = Parent};
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj is Chromosome && Equals((Chromosome)obj);
    }

    public override int GetHashCode() {
      unchecked {
        return Node != null ? Node.GetHashCode() : 0;
      }
    }

    public override string ToString() {
      return Node.ToString();
    }
  }
}