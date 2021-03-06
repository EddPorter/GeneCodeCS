﻿//
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

using Common.Logging;
using GeneCodeCS.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneCodeCS.Test
{
  [TestClass]
  public class BotGeneratorTest
  {
    [TestMethod]
    [DeploymentItem("GeneCodeCS.dll")]
    public void BotGeneratorCreateBotNameCreatesExpectedNameGivenParameters1() {
      BotGeneratorCreateBotNameCreatesExpectedNameGivenParameters1Helper<BaseBot>();
    }

    [TestMethod]
    [DeploymentItem("GeneCodeCS.dll")]
    public void BotGeneratorCreateBotNameCreatesExpectedNameGivenParameters2() {
      BotGeneratorCreateBotNameCreatesExpectedNameGivenParameters2Helper<BaseBot>();
    }

    [TestMethod]
    public void BotGeneratorCreateRandomBotReturnsNonNullBotTree() {
      BotGeneratorCreateRandomBotReturnsNonNullBotTreeHelper<TestBot>();
    }

    private static void BotGeneratorCreateBotNameCreatesExpectedNameGivenParameters1Helper<TBot>() {
      const int generation = 0;
      const int populus = 0;
      const string expected = "BaseBot_Gen0_Bot0";
      var actual = Reproduction_Accessor<TBot>.CreateBotName(generation, populus);
      Assert.AreEqual(expected, actual);
    }

    private static void BotGeneratorCreateBotNameCreatesExpectedNameGivenParameters2Helper<TBot>() {
      const int generation = 4;
      const int populus = 5;
      const string expected = "BaseBot_Gen4_Bot5";
      var actual = Reproduction_Accessor<TBot>.CreateBotName(generation, populus);
      Assert.AreEqual(expected, actual);
    }

    private static void BotGeneratorCreateRandomBotReturnsNonNullBotTreeHelper<TBot>() where TBot : BaseBot {
      var target = new Reproduction<TBot>();
      const int maxTreeDepth = 3;

      var actual = target.CreateBot(maxTreeDepth);

      Assert.IsNotNull(actual.Tree);
    }
  }
}