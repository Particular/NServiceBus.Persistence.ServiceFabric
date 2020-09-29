namespace TestRunner
{
    /// <summary>
    /// Allows to inject a property of type TDependency that is declared on the stateful service hierarchy.
    /// <code>
    ///  <![CDATA[
    ///  [TestFixture]
    ///  public class TestAccessingReliableStateManager : INeed<IReliableStateManager> {
    ///     IReliableStateManager stateManager;
    ///  
    ///     public void Need(IReliableStateManager dependency) {
    ///         stateManager = dependency;
    ///     }
    /// }
    ///  ]]>
    ///  </code>
    /// </summary>
    [Need]
    public interface INeed<TDependency>
    {
        void Need(TDependency dependency);
    }
}