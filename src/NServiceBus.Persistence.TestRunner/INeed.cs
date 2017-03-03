namespace TestRunner
{
    [Need]
    // ReSharper disable once TypeParameterCanBeVariant
    public interface INeed<TDependency>
    {
        void Need(TDependency dependency);
    }
}