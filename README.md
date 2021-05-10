# aiv-fast3d &middot; [![Nuget Version](https://img.shields.io/nuget/v/Aiv.Fast3D?color=blue)](https://www.nuget.org/packages/Aiv.Fast3D) [![Nuget Downloads](https://img.shields.io/nuget/dt/Aiv.Fast3D?color=yellow)](https://www.nuget.org/packages/Aiv.Fast3D) [![Api Doc](https://img.shields.io/badge/api--doc-read-blue)](http://aiv01.github.io/aiv-fast3d/) [![Build Status](https://travis-ci.org/aiv01/aiv-fast3d.svg?branch=master)](https://travis-ci.org/aiv01/aiv-fast3d)

Hardware accelerated 3D library built upon Aiv.Fast2D, mainly aimed at developing 2.5D games

# Examples
Below a very basic usage example.

> More examples are available in [Example project](./aiv-fast3d-example).

```cs
// Loading an Wavefront Obj file and showing the mesh in wireframe mode
Window window = new Window(1024, 768, "3D Suzanne Test");
window.SetDefaultViewportOrthographicSize(10);
window.SetZNearZFar(-300, 300);
window.EnableDepthTest();
window.CullBackFaces();

Mesh3 suzanne = SceneImporter.LoadMesh("Assets/suzanne.obj", new Vector3(2, -2, 2))[0];
suzanne.Position3 = new Vector3(window.OrthoWidth / 2.0f, window.OrthoHeight / 2.0f, 0);

while (window.IsOpened)
{
    suzanne.DrawWireframe(255, 0, 0);
    suzanne.EulerRotation3 += new Vector3(0, 10, 0) * window.DeltaTime;
    window.Update();
}
```


# Documentation
API documentation related to the last version of the library is published [here](http://aiv01.github.io/aiv-fast3d/).

# Compliance
Library tested on:
* Visual Studio 2019 v16.9.4
* .NET Framework 4.8
* Any Cpu architecture

# Contribution
If you want to contribute to the project, please follow the [guidelines](CONTRIBUTING.md).
