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
  public class TerminalGeneTests
  {
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TerminalGene_Constructor_Given_methodInfo_not_returning_void_throws_exception() {
      var method = new DynamicMethod("TestMethod", typeof(bool), null);
      new TerminalGene(method);
    }

    [TestMethod]
    public void TerminalGene_Constructor_Given_methodInfo_with_IParameter_and_default_parameters_creates_object() {
      var method = GetType().GetMethod("TestMethod");
      var terminalGene = new TerminalGene(method);
      Assert.IsNotNull(terminalGene);
    }

    [TestMethod]
    public void TerminalGene_Constructor_Given_methodInfo_with_IParameter_parameters_creates_object() {
      var method = new DynamicMethod("TestMethod", typeof(void), new[] { typeof(TestParameter), typeof(TestParameter) });
      var terminalGene = new TerminalGene(method);
      Assert.IsNotNull(terminalGene);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TerminalGene_Constructor_Given_methodInfo_with_nonIParameter_parameters_throws_exception() {
      var method = new DynamicMethod("TestMethod", typeof(void), new[] { typeof(bool), typeof(int) });
      new TerminalGene(method);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TerminalGene_Constructor_Given_null_methodInfo_throws_exception() {
      new TerminalGene(null);
    }

    [TestMethod]
    public void TerminalGene_Constructor_Stores_methodInfo() {
      var method = new DynamicMethod("TestMethod", typeof(void), new[] { typeof(TestParameter), typeof(TestParameter) });
      var terminalGene = new TerminalGene(method);
      Assert.AreEqual(method, terminalGene.MethodInfo);
    }

    public void TestMethod(TestParameter number, int value = 6) {
    }

    #region Nested type: TestParameter

    public class TestParameter : IParameter<int>
    {
      public int Value {
        get { return 5; }
      }
    }

    #endregion
  }
}