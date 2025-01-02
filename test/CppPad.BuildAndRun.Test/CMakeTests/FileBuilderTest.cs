using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.Scripting;

namespace CppPad.BuildAndRun.Test.CMakeTests;

public class FileBuilderTest
{
    [Fact]
    public void Build_ShouldIncludeProjectName()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp"
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("project(main)", result);
    }

    [Fact]
    public void Build_ShouldIncludeCompilerSetting()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp",
            Compiler = "g++"
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("set(CMAKE_CXX_COMPILER g++)", result);
    }

    [Fact]
    public void Build_ShouldIncludeCppStandard()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp",
            CppStandard = CppStandard.Cpp17
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("set(CMAKE_CXX_STANDARD 17)", result);
    }

    [Fact]
    public void Build_ShouldIncludeOptimizationLevel()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp",
            OptimizationLevel = OptimizationLevel.O2
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("set(CMAKE_CXX_FLAGS \"${CMAKE_CXX_FLAGS} -O2\")", result);
    }

    [Fact]
    public void Build_ShouldIncludeIncludeDirectories()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp",
            IncludeDirectories = new List<string> { "include" }
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("include_directories(include)", result);
    }

    [Fact]
    public void Build_ShouldIncludeLibrarySearchPaths()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp",
            LibSearchPaths = new List<string> { "lib" }
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("link_directories(lib)", result);
    }

    [Fact]
    public void Build_ShouldIncludeLibraryFiles()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp",
            LibFiles = new List<string> { "libmylib.a" }
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("target_link_libraries(main libmylib.a)", result);
    }

    [Fact]
    public void Build_ShouldIncludeSourceFile()
    {
        var options = new CMakeOptions
        {
            CppFileName = "main.cpp"
        };
        var builder = new FileBuilder();
        var result = builder.Build(options);

        Assert.Contains("add_executable(main main.cpp)", result);
    }
}