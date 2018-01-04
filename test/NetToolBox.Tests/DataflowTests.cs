using FluentAssertions;
using NetToolBox.TPLDataflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace NetToolBox.Tests
{
    public class DataflowTests
    {
        [Fact]
        public async Task ProcessSinglePost()
        {
            var fixture = new DataflowTestFixture();
            await fixture.ActionBlockFixture.SendAsync("file1");
            fixture.ActionBlockFixture.Complete();
            await fixture.ActionBlockFixture.Completion;
            fixture.ProcessedList.Single().Should().Be("file1");
        }
        [Fact]
        public async Task DoesntProcessDuplicates()
        {
            var fixture = new DataflowTestFixture();
            await fixture.ActionBlockFixture.SendAsync("file1");
            await fixture.ActionBlockFixture.SendAsync("file1");
            fixture.ActionBlockFixture.Complete();
            await fixture.ActionBlockFixture.Completion;
            fixture.ProcessedList.Single().Should().Be("file1");
        }
    }

    public class DataflowTestFixture
    {
        public readonly ActionBlockPreventDuplicates<string> ActionBlockFixture;
        public readonly List<string> ProcessedList=new List<string>();
        private readonly object _listLock = new object();
        public DataflowTestFixture()
        {
            ActionBlockFixture = new ActionBlockPreventDuplicates<string>(x => ProcessItem(x), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2, EnsureOrdered = false });
        }
        private void ProcessItem(string item)
        {
            lock (_listLock)
            {
                ProcessedList.Add(item);
            }
        }
    }
}
