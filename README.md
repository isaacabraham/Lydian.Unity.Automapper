Lydian.Unity.Automapper
=======================

An easy-to-use, flexible package that provides automatic resolution of interface and concrete mappings for the Microsoft Unity framework.

Get up and running with the following three lines: -

```
using Lydian.Unity.Automapper;
var container = new UnityContainer();
container.AutomapAssemblies("MyFirstAssembly", "MySecondAssembly", "MyThirdAssembly");

// All registrations between the three assemblies have been wired up.
```

Read the full wiki for details.