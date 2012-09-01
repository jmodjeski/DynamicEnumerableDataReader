using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using FluentAssertions;

namespace ModjeskiNet.Data.Tests
{
    [TestClass]
    public class PredicateBuilderTests
    {
        private IEnumerable<dynamic> DynamicSet;
        private IEnumerable<SampleData> SampleSet;

        public class SampleData
        {
            public string Name { get; set; }
            public int Number { get; set; }
            public DateTime Date { get; set; }
        }

        public class SampleDynamic
            : DynamicObject
        {
            private IDictionary<string, object> Values = new Dictionary<string, object>();

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                Values[binder.Name] = value;
                return true;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = null;
                if (!Values.ContainsKey(binder.Name))
                    return false;

                result = Values[binder.Name];
                return true;
            }
        }

        [TestInitialize]
        public void SetupSets()
        {
            var dynamicSet = new dynamic[8];
            for(var i = 0; i < dynamicSet.Length; i++)
            {
                dynamic o = new SampleDynamic();
                o.Name = String.Format("Name{0}", i);
                o.Date = DateTime.Today.AddDays(i);
                o.Number = i;
                dynamicSet[i] = o;
            }
            DynamicSet = dynamicSet;

            var sampleSet = new SampleData[8];
            for (var i = 0; i < sampleSet.Length; i++)
            {
                sampleSet[i] = new SampleData{
                Name = String.Format("Name{0}", i),
                Date = DateTime.Today.AddDays(i),
                Number = i
                };
            }
            SampleSet = sampleSet;
        }

        [TestMethod]
        public void RealClass_String_Equals()
        {
            var builder = new PredicateBuilder<SampleData>();
            var predicate = builder.Build("[Name] == 'Name0'");
            var query = SampleSet.Where(predicate);

            query.Count().Should().Be(1);
        }

        [TestMethod]
        public void RealClass_Date_LessThan()
        {
            var builder = new PredicateBuilder<SampleData>();
            var predicate = builder.Build(
                String.Format("[Date] < #{0:MM-dd-yyyy}#", 
                    DateTime.Today.AddDays(3)));
            var query = SampleSet.Where(predicate);

            query.Count().Should().Be(4);
        }

        [TestMethod]
        public void RealClass_Number_GreaterThan()
        {
            var builder = new PredicateBuilder<SampleData>();
            var predicate = builder.Build(
                String.Format("[Number] > 2",
                    DateTime.Today.AddDays(3)));
            var query = SampleSet.Where(predicate);

            query.Count().Should().Be(5);
        }


        [TestMethod]
        public void Dynamic_String_Equals()
        {
            var builder = new PredicateBuilder<dynamic>();
            var predicate = builder.Build("[Name] == 'Name0'");
            var query = DynamicSet.Where(predicate);

            query.Count().Should().Be(1);
        }

        [TestMethod]
        public void Dynamic_Date_LessThan()
        {
            var builder = new PredicateBuilder<dynamic>();
            var predicate = builder.Build(
                String.Format("[Date] < #{0:MM-dd-yyyy}#",
                    DateTime.Today.AddDays(3)));
            var query = DynamicSet.Where(predicate);

            query.Count().Should().Be(4);
        }

        [TestMethod]
        public void Dynamic_Number_GreaterThan()
        {
            var builder = new PredicateBuilder<dynamic>();
            var predicate = builder.Build(
                String.Format("[Number] > 2",
                    DateTime.Today.AddDays(3)));
            var query = DynamicSet.Where(predicate);

            query.Count().Should().Be(5);
        }
    }
}
