namespace WorkflowLibrary
{
    public interface IParameter
    {
        string Name { get; }
        object Value { get; set; }
        Parameter.SourceType Source { get; set; }
    }
}
