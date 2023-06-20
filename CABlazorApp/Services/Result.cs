namespace CABlazorApp.Services;

public class Result
{
    public bool? State { get; set; }
    public string[] Errors { get; set; }
    
    public Result()
    {
        State = null;
        Errors = new[] { "", "", "", "", "", "", "", "", "", "" };
    }

    public Result(bool? state)
    {
        State = state;
        Errors = new[] { "", "", "", "", "", "", "", "", "", "" };
    }
    
    public Result(string[] errors)
    {
        State = null;
        Errors = errors;
    }
    
    public Result(bool? state, string[] errors)
    {
        State = state;
        Errors = errors;
    }
}