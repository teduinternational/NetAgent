namespace NetAgent.Abstractions.LLM
{
    public interface IResponseScorer
    {
        double ScoreResponse(string response);
    }
}
