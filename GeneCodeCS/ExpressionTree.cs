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

namespace GeneCodeCS
{
  /// <summary>
  ///   A tree of expressions representing the bot behaviour.
  /// </summary>
  public sealed class ExpressionTree : IEquatable<ExpressionTree>
  {
    public IExpressionInstance Node { get; internal set; }

    internal ExpressionTree Parent { private get; set; }

    #region IEquatable<ExpressionTree> Members

    public bool Equals(ExpressionTree value) {
      if (ReferenceEquals(null, value)) {
        return false;
      }
      if (ReferenceEquals(this, value)) {
        return true;
      }
      return Equals(value.Node, Node);
    }

    #endregion

    public static bool operator !=(ExpressionTree a, ExpressionTree b) {
      return !(a == b);
    }

    public static bool operator ==(ExpressionTree a, ExpressionTree b) {
      if (ReferenceEquals(a, b)) {
        return true;
      }
      if (((object)a == null) || ((object)b == null)) {
        return false;
      }
      return a.Equals(b);
    }

    public static ExpressionTree ReplaceNodeWithCopy(ExpressionTree removedNode, ExpressionTree insertedNode) {
      return new ExpressionTree {Node = insertedNode.Node, Parent = removedNode.Parent};
    }

    /// <summary>
    ///   Creates a new, independent instance of the whole expression tree.
    /// </summary>
    /// <returns> A new, memory-independent copy of this expression tree. </returns>
    public ExpressionTree Clone() {
      return new ExpressionTree {Node = Node.Clone(), Parent = Parent};
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj is ExpressionTree && Equals((ExpressionTree)obj);
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