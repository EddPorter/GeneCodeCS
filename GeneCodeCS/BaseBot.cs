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
using Common.Logging;
using GeneCodeCS.Genetics;

namespace GeneCodeCS
{
  /// <summary>
  ///   The base class for all bots. This class must be inherited from and the public abstract <c>NotFinished</c> method implemented. The resulting class should be abstract too and <c>RunBotLogic</c> left unimplemented. Optionally, the public virtual methods can be overridden. The resulting type must then be used as the generic type parameter used to construction the <see
  ///    cref="Population{TBot}" /> class.
  /// </summary>
  /// <remarks>
  ///   When the bot is executed a call is made to <c>Execute</c> . The default implementation simply calls <c>RunBotLogic</c> , which contains the generated chromosome bot code (and should not be implemented by the user class). This allows custom logic to be wrapped around the main bot code. The implemented sub-class must provide a constructor that takes a single <see
  ///    cref="ILog" /> argument. Additional public methods returning <c>void</c> become terminal genes. Additional public methods returning <c>bool</c> become branch genes. All parameters passed to these methods must implement <see
  ///    cref="IParameter{T}" /> .
  /// </remarks>
  public abstract class BaseBot
  {
    /// <summary>
    ///   The <see cref="NotFinished" /> method, for use by the generated code.
    /// </summary>
    internal static readonly MethodInfo NotFinishedMethod = typeof(BaseBot).GetMethod("IsNotFinished",
                                                                                    BindingFlags.Instance |
                                                                                    BindingFlags.NonPublic);

    /// <summary>
    ///   The compiled chromosome, set when the bot is created from its expression tree.
    /// </summary>
    private Action _logic;

    /// <summary>
    ///   Gets custom information about the bot as configured by the bot's implementation of the <see cref="Execute" /> method.
    /// </summary>
    public object CustomInformation { get; protected set; }

    /// <summary>
    ///   Gets the evaluated fitness of the bot after execution against a dataset. Equals <c>Int.MinValue</c> prior to evaluation. The Bot must set this as part of its implementation of <see
    ///    cref="Execute" /> .
    /// </summary>
    public int Fitness { get; protected set; }

    /// <summary>
    ///   Called to begin bot evaluation. By the end of the method, the <see cref="Fitness" /> property should have been updated to reflect how the bot did. If any uncaught exceptions are thrown during execution, <see
    ///    cref="Fitness" /> is reset to <see cref="int.MinValue" /> .
    /// </summary>
    public virtual void Execute() {
      try {
        RunBotLogic();
        Evaluate();
      } catch {
        Fitness = int.MinValue;
      }
    }

    /// <summary>
    ///   Can be used to pass parameters to the bot prior to execution.
    /// </summary>
    /// <param name="parameters"> The parameters to be passed. </param>
    public virtual void Initialise(object parameters) { }

    /// <summary>
    /// Calculate the fitness of the bot after execution against the data.
    /// </summary>
    protected abstract void Evaluate();

    /// <summary>
    ///   Must be implemented to allow the bot to terminate at the appropriate time.
    /// </summary>
    /// <returns> Whether the execution cycle is complete. </returns>
    protected abstract bool NotFinished();

    /// <summary>
    ///   Allows the generated code to call <see cref="NotFinished" /> without it being public.
    /// </summary>
    internal bool IsNotFinished() { return NotFinished(); }

    /// <summary>
    ///   Sets the compiled chromosome that <see cref="RunBotLogic" /> will run.
    /// </summary>
    internal void SetLogic(Action logic) { _logic = logic; }

    /// <summary>
    ///   Contains the bot logic. Implemented by running <see cref="Population{TBot}.SimulateGenerations{T}" /> or <see
    ///    cref="Population{TBot}.SimulateIndividuals{T}" /> .
    /// </summary>
    protected virtual void RunBotLogic() {
      if (_logic == null) {
        throw new InvalidOperationException("The bot has not been compiled.");
      }
      _logic();
    }
  }
}