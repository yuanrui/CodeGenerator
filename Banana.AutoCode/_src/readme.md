Code from https://github.com/mono/t4, base on v2.3.1
Some changes:
1. /Mono.TextTemplating.CodeCompilation/RuntimeInfo.cs
```
public static RuntimeInfo GetRuntime ()
{
	if (Type.GetType ("Mono.Runtime") != null)
	{
		return GetMonoRuntime ();
	}
	else if (RuntimeInformation.FrameworkDescription.StartsWith (".NET Framework", StringComparison.OrdinalIgnoreCase))
	{
		return GetNetFrameworkRuntime ();
	}
	else
	{
		if (UseSelfContainedRuntime())
		{
			return GetSelfContainedRuntime();
		}

		return GetDotNetCoreSdk ();
	}
}

static bool UseSelfContainedRuntime()
{
    var location = typeof(object).Assembly.Location;
    if (string.IsNullOrEmpty(location))
    {
        return false;
    }

    var runtimeDir = Path.GetDirectoryName(location);

    return File.Exists(Path.Combine(runtimeDir, "coreclr.dll")) && File.Exists(Path.Combine(runtimeDir, "clrjit.dll"));
}

static RuntimeInfo GetSelfContainedRuntime()
{
    var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);

    var hostVersion = Environment.Version;
    if (hostVersion.Major < 5)
    {
        var versionPathComponent = Path.GetFileName(runtimeDir);
        if (SemVersion.TryParse(versionPathComponent, out var hostSemVersion))
        {
            hostVersion = new Version(hostSemVersion.Major, hostSemVersion.Minor, hostSemVersion.Patch);
        }
        else
        {
            return FromError(RuntimeKind.NetCore, "Could not determine host runtime version");
        }
    }

    static string MakeCscPath(string d) => Path.Combine(d, "csc.dll");

    SemVersion.TryParse(hostVersion.ToString(), out var sdkVersion);
    var maxCSharpVersion = CSharpLangVersionHelper.FromNetCoreSdkVersion(sdkVersion);

    return new RuntimeInfo(RuntimeKind.NetCore) { RuntimeDir = runtimeDir, RefAssembliesDir = null, CscPath = MakeCscPath(runtimeDir), MaxSupportedLangVersion = maxCSharpVersion, Version = hostVersion };
}
```