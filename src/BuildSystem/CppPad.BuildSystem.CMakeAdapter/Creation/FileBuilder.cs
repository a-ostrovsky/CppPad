using System.Text;
using CppPad.Scripting;

namespace CppPad.BuildSystem.CMakeAdapter.Creation;

public class FileBuilder
{
    public string Build(CMakeOptions options)
    {
        var builder = new StringBuilder();
        builder.AppendLine("cmake_minimum_required(VERSION 3.10)");
        var projectName = Path.GetFileNameWithoutExtension(options.CppFileName);
        builder.AppendLine($"project({projectName})");
        builder.AppendLine();

        if (options.Compiler is not null)
        {
            builder.AppendLine($"# Compiler setting");
            builder.AppendLine($"set(CMAKE_CXX_COMPILER {options.Compiler})");
            builder.AppendLine();
        }

        if (options.CppStandard != CppStandard.Unspecified)
        {
            builder.AppendLine($"# C++ standard");
            builder.AppendLine($"set(CMAKE_CXX_STANDARD {GetCppStandard(options.CppStandard)})");
            builder.AppendLine($"set(CMAKE_CXX_STANDARD_REQUIRED ON)");
            builder.AppendLine($"set(CMAKE_CXX_EXTENSIONS OFF)");
            builder.AppendLine();
        }

        if (options.OptimizationLevel != OptimizationLevel.Unspecified)
        {
            builder.AppendLine($"# Optimization level");
            builder.AppendLine($"set(CMAKE_CXX_FLAGS \"${{CMAKE_CXX_FLAGS}} {GetOptimizationLevel(options.OptimizationLevel)}\")");
            builder.AppendLine();
        }
        
        if (options.IncludeDirectories.Count != 0)
        {
            builder.AppendLine($"# Include directories");
            foreach (var includeDir in options.IncludeDirectories)
            {
                builder.AppendLine($"include_directories({includeDir})");
            }
            builder.AppendLine();
        }
        
        if (options.LibSearchPaths.Count != 0)
        {
            builder.AppendLine($"# Library search paths");
            foreach (var libSearchPath in options.LibSearchPaths)
            {
                builder.AppendLine($"link_directories({libSearchPath})");
            }
            builder.AppendLine();
        }

        if (options.LibFiles.Count != 0)
        {
            builder.AppendLine($"# Library files");
            foreach (var libFile in options.LibFiles)
            {
                builder.AppendLine($"target_link_libraries({projectName} {libFile})");
            }
            builder.AppendLine();
        }

        builder.AppendLine($"# Source file");
        var filePathWithUnixSeparators = options.CppFileName.Replace('\\', '/');
        builder.AppendLine($"add_executable({projectName} {filePathWithUnixSeparators})");

        return builder.ToString();
    }
    
    private static string GetOptimizationLevel(OptimizationLevel optimizationLevel)
    {
        return optimizationLevel switch
        {
            OptimizationLevel.O0 => "-O0",
            OptimizationLevel.O1 => "-O1",
            OptimizationLevel.O2 => "-O2",
            OptimizationLevel.O3 => "-O3",
            _ => "-O0"
        };
    }
    
    private static string GetCppStandard(CppStandard cppStandard)
    {
        return cppStandard switch
        {
            CppStandard.Cpp11 => "11",
            CppStandard.Cpp14 => "14",
            CppStandard.Cpp17 => "17",
            CppStandard.Cpp20 => "20",
            CppStandard.Cpp23 => "23",
            CppStandard.CppLatest => "latest",
            _ => "latest"
        };
    }
}