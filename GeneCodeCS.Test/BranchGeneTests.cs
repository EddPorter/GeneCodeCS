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
using System.Reflection.Emit;
using GeneCodeCS.Genetics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class BranchGeneTests
  {
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void BranchGene_Constructor_Given_methodInfo_not_returning_bool_throws_exception() {
      var method = new DynamicMethod("TestMethod", typeof(void), null);
      new BranchGene(method);
    }

    [TestMethod]
    public void BranchGene_Constructor_Given_methodInfo_with_IParameter_and_default_parameters_creates_object() {
      var method = GetType().GetMethod("TestMethod");
      var branchGene = new BranchGene(method);
      Assert.IsNotNull(branchGene);
    }

    [TestMethod]
    public void BranchGene_Constructor_Given_methodInfo_with_IParameter_parameters_creates_object() {
      var method = new DynamicMethod("TestMethod", typeof(bool), new[] { typeof(TestParameter), typeof(TestParameter) });
      var branchGene = new BranchGene(method);
      Assert.IsNotNull(branchGene);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void BranchGene_Constructor_Given_methodInfo_with_nonIParameter_parameters_throws_exception() {
      var method = new DynamicMethod("TestMethod", typeof(bool), new[] { typeof(bool), typeof(int) });
      new BranchGene(method);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void BranchGene_Constructor_Given_null_methodInfo_throws_exception() {
      new BranchGene(null);
    }

    [TestMethod]
    public void BranchGene_Constructor_Stores_methodInfo() {
      var method = new DynamicMethod("TestMethod", typeof(bool), new[] { typeof(TestParameter), typeof(TestParameter) });
      var branchGene = new BranchGene(method);
      Assert.AreEqual(method,branchGene.MethodInfo);
    }

    public bool TestMethod(TestParameter number, int value = 6) {
      return false;
    }

    #region Nested type: TestParameter

    public class TestParameter : IParameter<int>
    {
      #region IParameter<int> Members

      public int Value {
        get { return 5; }
      }

      #endregion
    }

    #endregion
  }
}