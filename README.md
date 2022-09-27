# CredentialProvider.Net6

This is a kind-of a fork of Steve Syfuhs' [Windows Credential Provider](https://syfuhs.net/2017/10/15/creating-custom-windows-credential-providers-in-net/) project written in C#, updated from .NET Framework 4.6 to .NET6. It's bare bones, i.e. nothing on top of basic functionality of the original, except now it is in .NET6, allowing for further development and using up-to-date libraries.

The process to register .NET6 binaries to COM is a bit different from .NET Framework case:

```sh
# at first, publish the project to make sure all dependencies will be found in runtime
dotnet publish CredentialProvider.Net6.csproj -c Release -r win-x64  # and perhaps some other arguments of your choice

# now register the *.comhost.dll
regsvr32 bin\Release\net6\win-x64\publish\CredentialProvider.Net6.comhost.dll
```

That should be it!

**Note:** one potential pitfall is that I haven't succeeded in embedding the icon to the assembly as a resource (I need a NuGet package then and end up with runtime DLL load exceptions), so it is expected to see the icon in an absolute folder (currently `ProgramFiles\CredentialProvider.Net6\tile-icon.bmp`) - create it manually. PR to embed the icon in resource file is very welcome :-)