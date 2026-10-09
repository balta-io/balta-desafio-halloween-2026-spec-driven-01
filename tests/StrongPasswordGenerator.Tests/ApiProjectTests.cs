using System.Reflection;
using System.Runtime.Versioning;

namespace StrongPasswordGenerator.Tests;

public class ApiProjectTests
{
    [Fact]
    public void ApiTargetsDotNet10()
    {
        var apiAssembly = Assembly.Load("StrongPasswordGenerator.Api");
        var targetFramework = apiAssembly.GetCustomAttribute<TargetFrameworkAttribute>();

        Assert.Equal(".NETCoreApp,Version=v10.0", targetFramework?.FrameworkName);
    }
}
