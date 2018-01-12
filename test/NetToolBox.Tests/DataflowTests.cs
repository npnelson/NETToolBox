using FluentAssertions;
using Moq;
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
            var fixture = new DataflowTestFixture(false);
            fixture.MockProcessItem.Setup(x => x.ProcessItem(It.IsAny<string>()));
            await fixture.ActionBlockFixture.SendAsync("file1");
            fixture.ActionBlockFixture.Complete();
            await fixture.ActionBlockFixture.Completion;
            fixture.MockProcessItem.Verify(x=>x.ProcessItem(It.Is<string>(y=>y=="file1")),Times.Once);
        }
        [Fact]
        public async Task DoesntProcessDuplicates()
        {
            var fixture = new DataflowTestFixture(false);
            fixture.MockProcessItem.Setup(x => x.ProcessItem(It.IsAny<string>()));
            await fixture.ActionBlockFixture.SendAsync("file1");
            await fixture.ActionBlockFixture.SendAsync("file1");
            fixture.ActionBlockFixture.Complete();
            await fixture.ActionBlockFixture.Completion;
            fixture.MockProcessItem.Verify(x => x.ProcessItem(It.Is<string>(y => y == "file1")), Times.Once);
        }

        [Fact]
        public async Task DoesntProcessDuplicatesAsync()
        {
            var fixture = new DataflowTestFixture(true);
            fixture.MockProcessItem.Setup(x => x.ProcessItem(It.IsAny<string>()));
            await fixture.ActionBlockFixture.SendAsync("file1");
            await fixture.ActionBlockFixture.SendAsync("file1");
            fixture.ActionBlockFixture.Complete();
            await fixture.ActionBlockFixture.Completion;
            fixture.MockProcessItem.Verify(x => x.ProcessItem(It.Is<string>(y => y == "file1")), Times.Once);
        }

        //i can't figure out how to test this considering the threading concerns  in the unit test runner 
        //[Fact]
        //public async Task ProcessDuplicateAfterAlreadyProcessesedAsync()
        //{
        //    var fixture = new DataflowTestFixture(true);
        //    fixture.MockProcessItem.Setup(x => x.ProcessItem(It.IsAny<string>()));
        //    await fixture.ActionBlockFixture.SendAsync("file1");
        //    await fixture.ActionBlockFixture.SendAsync("file2");
        //    await fixture.ActionBlockFixture.SendAsync("file1");
        //    fixture.ActionBlockFixture.Complete();
        //    await fixture.ActionBlockFixture.Completion;
        //    fixture.MockProcessItem.Verify(x => x.ProcessItem(It.Is<string>(y => y == "file1")), Times.Exactly(2));
        //    fixture.MockProcessItem.Verify(x => x.ProcessItem(It.Is<string>(y => y == "file2")), Times.Once);
        //}
    }

    public interface IProcessItem
    {
        void ProcessItem(string item);
    }

    public class DataflowTestFixture
    {
        public readonly Mock<IProcessItem> MockProcessItem = new Mock<IProcessItem>();
        public readonly ActionBlockPreventDuplicates<string> ActionBlockFixture;
        public readonly List<string> ProcessedList=new List<string>();
        private readonly object _listLock = new object();
        public DataflowTestFixture(bool asyncFixture)
        {
            if (!asyncFixture)
            {
                ActionBlockFixture = new ActionBlockPreventDuplicates<string>(x => MockProcessItem.Object.ProcessItem(x), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2, EnsureOrdered = false });
            }
            else
            {
                ActionBlockFixture = new ActionBlockPreventDuplicates<string>(async x => await ProcessItemAsync(x), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2, EnsureOrdered = false });
            }
        }

        private  Task ProcessItemAsync(string item)
        {
            MockProcessItem.Object.ProcessItem(item);
            return Task.CompletedTask;
        }
        //private void ProcessItem(string item)
        //{
        //    lock (_listLock)
        //    {
        //        ProcessedList.Add(item);
        //    }
        //}
    }
}
