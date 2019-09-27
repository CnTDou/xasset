public interface ILoading
{
    float Progress { get; }
    
    bool IsDone { get; }
    
    string Error { get; }
}