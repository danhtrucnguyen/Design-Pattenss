using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace WebApplication1.BehavioralPatterns.TemplateMethod
{

    // AbstractClass:  BeverageMaker
    // Primitive Ops:  Brew(), AddCondiments()
    // Hooks:          WantCondiments(), WaterTempC
    // ConcreteClass:  CoffeeMaker, TeaMaker

    public abstract class BeverageMaker
    {
        private readonly int? _overrideTempC;
        private readonly bool? _overrideCondiments;

        protected BeverageMaker(int? waterTempC = null, bool? withCondiments = null)
        {
            _overrideTempC = waterTempC;
            _overrideCondiments = withCondiments;
        }

        //Template Method
        public List<string> Prepare()
        {
            var log = new List<string>();
            log.Add($"Bắt đầu pha {Name}");

            BoilWater(log, WaterTempC);
            Brew(log);
            PourInCup(log);

            if (WantCondiments)
                AddCondiments(log);

            log.Add($"Hoàn tất {Name}");
            return log;
        }

        //Hooks
        protected virtual string Name => "Đồ uống";
        protected virtual int DefaultWaterTempC => 100;
        protected int WaterTempC => _overrideTempC ?? DefaultWaterTempC;

        protected bool WantCondiments => _overrideCondiments ?? DefaultWantCondiments;
        protected virtual bool DefaultWantCondiments => true;

        //Primitive operations
        protected abstract void Brew(List<string> log);
        protected virtual void AddCondiments(List<string> log) { }

        //Common steps
        private static void BoilWater(List<string> log, int tempC)
            => log.Add($"Đun nước tới {tempC}°C");
        private static void PourInCup(List<string> log)
            => log.Add("Rót ra cốc");
    }

    //Coffee
    public sealed class CoffeeMaker : BeverageMaker
    {
        public CoffeeMaker(int? waterTempC = null, bool? withCondiments = null)
            : base(waterTempC, withCondiments) { }

        protected override string Name => "Cà phê";
        protected override void Brew(List<string> log)
            => log.Add("Lọc cà phê qua phin");
        protected override void AddCondiments(List<string> log)
            => log.Add("Thêm đường và sữa");
        protected override bool DefaultWantCondiments => true;
    }

    //Tea
    public sealed class TeaMaker : BeverageMaker
    {
        public TeaMaker(int? waterTempC = null, bool? withCondiments = null)
            : base(waterTempC, withCondiments) { }

        protected override string Name => "Trà";
        protected override int DefaultWaterTempC => 95;
        protected override void Brew(List<string> log)
            => log.Add("Ngâm trà trong nước");
        protected override void AddCondiments(List<string> log)
            => log.Add("Thêm lát chanh");
        protected override bool DefaultWantCondiments => true;
    }
}

//GET http://localhost:5102/api/template/demo
//POST http://localhost:5102/api/template/run

/*
[
  { "title": "Coffee sữa đường", "type": "coffee", "withCondiments": true },
  { "title": "Tea 95°C không chanh", "type": "tea", "waterTempC": 95, "withCondiments": false },
  { "title": "Coffee đen", "type": "coffee", "withCondiments": false }
]
*/