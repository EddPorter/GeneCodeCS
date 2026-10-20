using System;

namespace GeneCodeCS.Test.Helpers
{
  /// <summary>
  ///   A tiny bot for end-to-end tests: it has to reach a target number by choosing between increments and decrements.
  /// </summary>
  public class CounterBot : BaseBot
  {
    private const int Target = 10;
    private int _steps;

    public int Counter { get; private set; }

    public void Increment(StepParameter step) { Counter += step.Value; }

    public void Decrement(StepParameter step) { Counter -= step.Value; }

    public bool BelowTarget() { return Counter < Target; }

    protected override bool NotFinished() { return ++_steps <= 20; }

    protected override void Evaluate() { Fitness = 1000 - Math.Abs(Target - Counter); }

    public class StepParameter : Genetics.IParameter<int>
    {
      private readonly int _value;

      public StepParameter(Random random) { _value = random.Next(1, 4); }

      public StepParameter(int value) { _value = value; }

      public int Value { get { return _value; } }
    }
  }
}
