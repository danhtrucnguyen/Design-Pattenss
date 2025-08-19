using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.BehavioralPatterns.Interpreter
{
    // DTO input cho Postman
    public sealed class InterpreterJobDto
    {
        public List<string> Romans { get; set; } = new();
    }

    public static class InterpreterExample
    {
        public static object Run(InterpreterJobDto job)
        {
            var list = job?.Romans ?? new List<string>();
            var results = list.Select(s =>
            {
                var (val, rest) = RomanInterpreter.Parse(s);
                return new
                {
                    input = s,
                    value = val,
                    fullyParsed = string.IsNullOrEmpty(rest),
                    leftover = rest // nếu còn ký tự => input chứa phần không hợp lệ
                };
            });

            return new { count = list.Count, results };
        }
    }
}
