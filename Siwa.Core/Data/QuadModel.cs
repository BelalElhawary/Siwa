namespace Siwa.Core.Data;

public static class QuadModel
{
    public static readonly float[] Vertices =
    [   // coordinates       // normals             // uv
        -.5f, -.5f, .0f,     1.0f,  0.0f,  0.0f,   0.0f, 0.0f,
        -.5f,  .5f, .0f,     0.0f,  1.0f,  0.0f,   0.0f, 1.0f,
        .5f,  .5f, .0f,     0.0f,  0.0f,  1.0f,   1.0f, 1.0f,
        .5f, -.5f, .0f,     1.0f,  1.0f,  1.0f,   1.0f, 0.0f,
    ];
    
    public static readonly float[] CoordinatesOnly = 
    [   // coordinates  
        -.5f, -.5f, .0f,
        -.5f,  .5f, .0f,
        .5f,  .5f, .0f,
        .5f, -.5f, .0f,
    ];

    public static readonly uint[] Indices = [0, 2, 1, 0, 3, 2];
}