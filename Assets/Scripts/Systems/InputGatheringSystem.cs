// Copyright 2013-2022 AFI, Inc. All Rights Reserved.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class InputGatheringSystem : SystemBase, InputMap.IGameActions {
    private ClientSimulationSystemGroup _clientSimulationSystemGroup;

    private InputMap _inputMap;
    private int      _horizontal;
    private int      _vertical;
    private int      _rotationHorizontal;
    private int      _rotationVertical;
    private int      _jump;
    private float    _jumpLife;
    private bool     _isRotation;

    protected override void OnCreate() {
        RequireSingletonForUpdate<NetworkIdComponent>();

        _clientSimulationSystemGroup = World.GetOrCreateSystem<ClientSimulationSystemGroup>();

        _inputMap = new InputMap();
        _inputMap.Game.SetCallbacks(this);
    }

    protected override void OnStartRunning() {
        _inputMap.Enable();
    }

    protected override void OnStopRunning() {
        _inputMap.Disable();
    }

    protected override void OnUpdate() {
        var localEntity = GetSingleton<CommandTargetComponent>().targetEntity;
        if (Entity.Null == localEntity) {
            return;
        }

        EntityManager.GetBuffer<InputCommand>(localEntity).AddCommandData(new InputCommand {
            Tick = _clientSimulationSystemGroup.ServerTick,
            horizontal = _horizontal,
            vertical = _vertical,
            rotationHorizontal = _rotationHorizontal,
            rotationVertical = _rotationVertical,
            jump = _jump
        });
        if (1 == _jump) {
            _jumpLife -= Time.DeltaTime;
            if (0.0f >= _jumpLife) {
                _jump = 0;
            }
        }
    }

    private void SendShootRpc() {
        var netOwnerEntity = GetSingletonEntity<NetworkIdComponent>();
        if (Entity.Null == netOwnerEntity) {
            return;
        }

        var gunEntity = GetSingletonEntity<NetOwnerGunDisplayComponent>();
        if (Entity.Null == gunEntity) {
            return;
        }

        var gunLocalToWorld = EntityManager.GetComponentData<LocalToWorld>(gunEntity);

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var requestEntity = commandBuffer.CreateEntity();
        commandBuffer.AddComponent(requestEntity, new ShootRequest {
            direction = gunLocalToWorld.Forward,
            translate = gunLocalToWorld.Position,
        });
        commandBuffer.AddComponent(requestEntity, new SendRpcCommandRequestComponent {
            TargetConnection = netOwnerEntity
        });
        commandBuffer.Playback(EntityManager);
    }

    public void OnMove(InputAction.CallbackContext context) {
        var direction = context.ReadValue<Vector2>() * 10.0f;
        _horizontal = math.clamp((int)direction.x, -1, 1);
        _vertical = math.clamp((int)direction.y, -1, 1);
    }

    public void OnRotation(InputAction.CallbackContext context) {
        if (false == _isRotation) {
            _rotationHorizontal = 0;
            _rotationVertical = 0;
            return;
        }

        var direction = context.ReadValue<Vector2>();
        _rotationHorizontal = (int)direction.x;
        _rotationVertical = (int)direction.y;
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (context.performed) {
            _jumpLife = 0.3f;
            _jump = 1;
        }
    }

    public void OnShoot(InputAction.CallbackContext context) {
        if (context.performed) {
            SendShootRpc();
        }
    }

    public void OnOnRotation(InputAction.CallbackContext context) {
        if (context.performed) {
            _isRotation = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (context.canceled) {
            _isRotation = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}