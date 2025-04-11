namespace NetAgent.Abstractions
{
    /// <summary>
    /// Đại diện cho một agent hoạt động theo chuẩn MCP (Model Context Protocol)
    /// </summary>
    public interface IAgent
    {
        Task<string> ExecuteGoalAsync(string goal);
    }

}
