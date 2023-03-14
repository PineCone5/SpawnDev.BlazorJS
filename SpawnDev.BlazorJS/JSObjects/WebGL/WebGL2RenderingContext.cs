﻿using Microsoft.JSInterop;
using SpawnDev.BlazorJS.JsonConverters;
using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.JSObjects
{
    
    public class WebGL2RenderingContext : WebGLRenderingContext
    {
        public WebGL2RenderingContext(IJSInProcessObjectReference _ref) : base(_ref) { }
    }
}
