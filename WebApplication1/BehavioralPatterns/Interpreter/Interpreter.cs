using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.Interpreter
{
    // Context: giữ chuỗi Input còn lại và Output đã tính.
    public sealed class RomanContext
    {
        public string Input { get; set; }
        public int Output { get; set; }
        public RomanContext(string input) { Input = input ?? ""; }
    }

    // AbstractExpression: khuôn mẫu giải thích cho một bậc (nghìn/trăm/chục/đơn vị)
    public abstract class RomanExpression
    {
        public void Interpret(RomanContext ctx)
        {
            if (ctx.Input.Length == 0) return;

            if (!string.IsNullOrEmpty(Nine()) && ctx.Input.StartsWith(Nine()))
            { ctx.Output += 9 * Multiplier(); ctx.Input = ctx.Input[2..]; }

            else if (!string.IsNullOrEmpty(Four()) && ctx.Input.StartsWith(Four()))
            { ctx.Output += 4 * Multiplier(); ctx.Input = ctx.Input[2..]; }

            else
            {
                if (!string.IsNullOrEmpty(Five()) && ctx.Input.StartsWith(Five()))
                { ctx.Output += 5 * Multiplier(); ctx.Input = ctx.Input[1..]; }

                while (!string.IsNullOrEmpty(One()) && ctx.Input.StartsWith(One()))
                { ctx.Output += 1 * Multiplier(); ctx.Input = ctx.Input[1..]; }
            }
        }

        protected abstract string One();
        protected abstract string Four();
        protected abstract string Five();
        protected abstract string Nine();
        protected abstract int Multiplier();
    }

    // ConcreteExpression cho từng bậc
    public sealed class Thousands : RomanExpression
    {
        protected override string One() => "M";
        protected override string Four() => "";
        protected override string Five() => "";
        protected override string Nine() => "";
        protected override int Multiplier() => 1000;
    }
    public sealed class Hundreds : RomanExpression
    {
        protected override string One() => "C";
        protected override string Four() => "CD";
        protected override string Five() => "D";
        protected override string Nine() => "CM";
        protected override int Multiplier() => 100;
    }
    public sealed class Tens : RomanExpression
    {
        protected override string One() => "X";
        protected override string Four() => "XL";
        protected override string Five() => "L";
        protected override string Nine() => "XC";
        protected override int Multiplier() => 10;
    }
    public sealed class Ones : RomanExpression
    {
        protected override string One() => "I";
        protected override string Four() => "IV";
        protected override string Five() => "V";
        protected override string Nine() => "IX";
        protected override int Multiplier() => 1;
    }

    // Facade nhỏ cho client sử dụng
    public static class RomanInterpreter
    {
        private static readonly List<RomanExpression> _tree = new()
        {
            new Thousands(), new Hundreds(), new Tens(), new Ones()
        };

        public static (int value, string rest) Parse(string roman)
        {
            var ctx = new RomanContext((roman ?? "").Trim().ToUpperInvariant());
            foreach (var exp in _tree) exp.Interpret(ctx);
            return (ctx.Output, ctx.Input); // rest = phần còn lại (nếu input không hợp lệ hoàn toàn)
        }
    }
}


//GET http://localhost:5102/api/interpreter/demo
//POST http://localhost:5102/api/interpreter/parse

/*
 {
  "romans": ["III", "IX", "LVIII", "MCMXCIV", "ABC", "XLII"]
}
*/