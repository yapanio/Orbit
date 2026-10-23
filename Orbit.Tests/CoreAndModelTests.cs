using System.Collections.Generic;
using Orbit.Core;
using Orbit.Models;
using Xunit;

namespace Orbit.Tests
{
    public class CoreAndModelTests
    {
        private class TestViewModel : ViewModelBase
        {
            private string _name = string.Empty;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }
        }

        [Fact]
        public void ViewModelBase_SetProperty_RaisesPropertyChanged()
        {
            var vm = new TestViewModel();
            var raisedEvents = new List<string?>();
            vm.PropertyChanged += (s, e) => raisedEvents.Add(e.PropertyName);

            vm.Name = "NewName";
            Assert.Equal("NewName", vm.Name);
            Assert.Contains(nameof(TestViewModel.Name), raisedEvents);

            // Setting identical value should not trigger another event
            raisedEvents.Clear();
            vm.Name = "NewName";
            Assert.Empty(raisedEvents);
        }

        [Fact]
        public void RelayCommand_ExecutesAndCanExecuteEvaluatesCorrectly()
        {
            bool executed = false;
            bool canExecuteFlag = true;

            var command = new RelayCommand(
                _ => executed = true,
                _ => canExecuteFlag
            );

            Assert.True(command.CanExecute(null));
            command.Execute(null);
            Assert.True(executed);

            canExecuteFlag = false;
            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void ServiceItemModel_StatusChangeUpdatesStatusFlags()
        {
            var item = new ServiceItemModel
            {
                ServiceName = "testsvc",
                DisplayName = "Test Service",
                Status = "Stopped"
            };

            Assert.True(item.IsStopped);
            Assert.False(item.IsRunning);
            Assert.False(item.IsTransitioning);

            item.Status = "Starting...";
            Assert.False(item.IsStopped);
            Assert.False(item.IsRunning);
            Assert.True(item.IsTransitioning);

            item.Status = "Running";
            Assert.False(item.IsStopped);
            Assert.True(item.IsRunning);
            Assert.False(item.IsTransitioning);
        }
    }
}
