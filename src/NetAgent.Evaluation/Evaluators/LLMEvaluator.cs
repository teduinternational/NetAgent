using NetAgent.Core.LLM;
using NetAgent.Evaluation.Interfaces;
using NetAgent.Evaluation.Models;
using NetAgent.LLM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetAgent.Evaluation.Evaluators
{
    public class LLMEvaluator : IEvaluator
    {
        private readonly ILLMProvider _llm;

        public LLMEvaluator(ILLMProvider llm)
        {
            _llm = llm;
        }

        public async Task<EvaluationResult> EvaluateAsync(string prompt, string output, string goal, string context)
        {
            var evalPrompt = $"""
            You are an evaluator. Given the agent's goal and its response, rate the quality (0-1).

            Goal: {goal}
            Context: {context}
            Prompt: {prompt}
            Output: {output}

            Rate the response from 0 to 1. What could be improved?

            Respond in format:
            Score: <number>
            Weaknesses: <comma separated>
            Suggestions: <comma separated>
        """;

            var evalResponse = await _llm.GenerateAsync(evalPrompt);

            var score = double.Parse(Regex.Match(evalResponse, @"Score:\s*([\d\.]+)").Groups[1].Value);
            var weaknesses = Regex.Match(evalResponse, @"Weaknesses:\s*(.*)").Groups[1].Value
                .Split(',').Select(x => x.Trim()).ToArray();
            var suggestions = Regex.Match(evalResponse, @"Suggestions:\s*(.*)").Groups[1].Value
                .Split(',').Select(x => x.Trim()).ToArray();

            return new EvaluationResult
            {
                Score = score,
                Weaknesses = weaknesses,
                Suggestions = suggestions,
                Summary = evalResponse
            };
        }
    }
}
