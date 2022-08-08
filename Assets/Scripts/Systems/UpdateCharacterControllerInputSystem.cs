// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public partial class UpdateCharacterControllerInputSystem : SystemBase {
    private GhostPredictionSystemGroup _ghostPredictionSystemGroup;

    protected override void OnCreate() {
        _ghostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
    }

    protected override void OnUpdate() {
        var tick = _ghostPredictionSystemGroup.PredictingTick;
        Entities
            .ForEach((DynamicBuffer<InputCommand> inputBuffer, ref CharacterControllerInternalData internalData, in PredictedGhostComponent prediction) => {
                if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction)) {
                    return;
                }

                inputBuffer.GetDataAtTick(tick, out var input);
                internalData.Input.Movement.x = input.horizontal;
                internalData.Input.Movement.y = input.vertical;

                internalData.Input.Looking.x = input.rotationHorizontal;
                internalData.Input.Looking.y = input.rotationVertical;

                if (1 == input.jump) {
                    internalData.Input.Jumped = input.jump;
                }
            })
            .ScheduleParallel();
    }
}