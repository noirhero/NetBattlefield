// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public partial struct CopySpawnPointJob : IJobEntity {
    public NativeArray<float4>     copyTranslateAndScales;
    public NativeArray<quaternion> rotation;

    public void Execute([EntityInQueryIndex] int entityInQueryIndex, in SpawnPointComponent spawnPoint) {
        copyTranslateAndScales[entityInQueryIndex] = spawnPoint.translateAndScale;
        rotation[entityInQueryIndex] = spawnPoint.rotation;
    }
}