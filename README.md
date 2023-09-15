# Three-Body Problem

## Introduction

This is a project for simulating, analyzing, and visualizing the three-body problem.
This is accomplished by simulating the three-body problem millions of times on the GPU.

## Dependencies

### .NET 7.0

The project targets .NET 7.0 with C# 11.0.

### [Atlas 1.0.0](https://github.com/apeltsi/Atlas)

The project uses Atlas for graphics & compute support.
As the levels of parallel computing required for simulating hundreds of millions of three-body systems is not possible with the CPU,
the project uses the GPU for simulation. This is made possible with the compute shader support in Atlas.

### Other dependencies

- SixLabors.ImageSharp (for generating & loading images)
- ZstdSharp.Port (for compressing & decompressing data)
