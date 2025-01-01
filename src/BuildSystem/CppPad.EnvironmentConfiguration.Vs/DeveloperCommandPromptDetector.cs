using CppPad.SystemAdapter.IO;

namespace CppPad.EnvironmentConfiguration.Vs;

public class DeveloperCommandPromptDetector(DiskFileSystem fileSystem, IVsWhereAdapter vsWhereAdapter)
{
    public async Task<string> GetDeveloperCommandPromptAsync()
    {
        var visualStudioPaths = await vsWhereAdapter.GetVisualStudioPathsAsync();
        
        foreach (var visualStudioPath in visualStudioPaths)
        {
            var developerCommandPromptPath = Path.Combine(visualStudioPath, @"Common7\Tools\VsDevCmd.bat");
            if (fileSystem.FileExists(developerCommandPromptPath))
            {
                return developerCommandPromptPath;
            }
        }
        
        throw new EnvironmentConfigurationException("Developer command prompt not found.");
    }
}