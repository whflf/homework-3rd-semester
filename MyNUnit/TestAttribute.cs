﻿namespace MyNUnit;

[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : Attribute
{
    public Type? Expected { get; set; }
    public string? Ignore { get; set; }
}
