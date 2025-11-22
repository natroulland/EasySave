using Moq;
using System.IO.Abstractions;

using EasySave.Core.Model;
using EasySave.ViewModel;
using EasySave.Core.Model.Entities;
using EasySave.Core.Model.Enums;

namespace EasySave.Tests.EasySave.Tests.ViewModel;

public class JobManagerViewModelTests
{
    private Mock<IFileSystem> _mockFileSystem;
    private JobManagerViewModel _jobManagerViewModel;

    public JobManagerViewModelTests()
    {
        _mockFileSystem = new Mock<IFileSystem>();
        _jobManagerViewModel = new JobManagerViewModel();
    }

    [Fact]
    public void ValidateJobOptionsTests_DirectoryDoesNotExist_ShouldThrowError()
    {
        //arrange
        _mockFileSystem.Setup(d => d.Directory.Exists(It.IsAny<string>())).Returns(false); // Whatever the path is, method will return false

        Job job = new()
        {
            name = "test",
            sourcePath = "test",
            targetPath = "test",
            saveType = SaveType.Differential
        };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => _jobManagerViewModel.ValidateAndSaveJob(job, ""));
    }

    [Fact]
    public void ValidateJobOptionsTests_JobIsNull_ShouldThrow()
    {
        // arrange
        Job job = null;

        // Act and assert
        Assert.Throws<ArgumentNullException>(() => _jobManagerViewModel.ValidateAndSaveJob(job, ""));
    }

    [Fact]
    public void ValidateJobOptionsTests_NameIsEmpty_ShouldReturnFalse()
    {
        //arrange
        Job job = new()
        {
            name = "",
            sourcePath = "test",
            targetPath = "test",
            saveType = SaveType.Differential
        };

        //act
        bool result = _jobManagerViewModel.ValidateAndSaveJob(job, ""); // Using ValidateAndSaveJob because ValidateJobOptions is private

        // Assert
        Assert.False(result);
    }
}
