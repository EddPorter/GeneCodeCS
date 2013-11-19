namespace GeneCodeCS
{
  /// <summary>
  ///   Defines the method to use for selection of bots to breed to create the next generation.
  /// </summary>
  public enum Selection
  {
    /// <summary>
    ///   Picks bots more often in proportion to their calculated fitness.
    /// </summary>
    FitnessProportionate,

    /// <summary>
    ///   Plays a selection of bots against each other, picking those with high fitnesses.
    /// </summary>
    Tournament
  }
}