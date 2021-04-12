using Godot;

public class HumanNode : Node2D {
    public Pilot pilot;

    public PlayerInput playerInput;

    public ArenaCameraNode camera;
    public CanvasLayer canvas;

    private GamepadCursorNode _cursor;
    private TargetLockNode _targetLock;

    private VesselHudNode _hud;

    [Signal]
    public delegate void Defeated();

    public override void _Ready() {
        camera.SetTarget(pilot.Vessel);
        pilot.Vessel.Connect("Destroyed", this, nameof(OnVesselDestroyed));

        _targetLock = TargetLockNode.New();
        _targetLock.camera = camera;
        canvas.AddChild(_targetLock);

        _cursor = GamepadCursorNode.New(pilot);
        _cursor.SetAnchor(_targetLock);
        _cursor.Position = camera.GetViewport().Size / 2;
        canvas.AddChild(_cursor);

        _hud = VesselHudNode.New();
        _hud.camera = camera;
        _cursor.AddChild(_hud);

        foreach (var w in pilot.Vessel.weapons) {
            if (w is ZapWeapon zapGun) {
                zapGun.SetTargetLock(_cursor);
            }
        }
    }

    private void OnVesselDestroyed() {
        pilot.Active = false;
        QueueFree();
        EmitSignal(nameof(Defeated));
    }

    public override void _Process(float delta) {
        HandleInput();
        if (!playerInput.IsGamepadControlled()) {
            _cursor.GlobalPosition = GetGlobalMousePosition();
        }
        UpdateHUD();
    }

    private void UpdateHUD() {
        var state = pilot.Vessel.State;
        _hud.UpdateEnergyPercentage(QMath.Percantage(state.energy, state.maxEnergy));
        _hud.UpdateBackupEnergyPercentage(QMath.Percantage(state.backupEnergy, state.maxBackupEnergy));
    }

    private void HandleInput() {
        Vector2 cursor;
        if (!playerInput.IsGamepadControlled()) {
            cursor = GetGlobalMousePosition();
        } else {
            // TODO: use qmath TranslateViewportPos here?
            var offset = camera.GetCameraScreenCenter() - camera.GetViewportRect().Size / 2;
            cursor = _cursor.Position + offset;
        }

        if (playerInput.IsActionJustPressed("clearWaypoints")) {
            pilot.Vessel.ClearWaypoints();
        }

        if (playerInput.IsActionPressed("shield")) {
            if (pilot.Vessel.shield.CanActivate(pilot.Vessel.State)) {
                pilot.Vessel.shield.Activate(pilot.Vessel.State);
            }
        }

        if (playerInput.IsActionJustPressed("leftMouseButton")) {
            var wp = Waypoint.New(false, null);
            if (playerInput.GetDeviceId() != 0) {
                wp.GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>("res://images/waypoint2.png");
            }
            GetParent().AddChild(wp);
            wp.GlobalPosition = cursor;
            pilot.Vessel.AddWaypoint(wp);
            GetTree().SetInputAsHandled();
        }

        if (playerInput.IsActionPressed("weapon0")) {
            if (pilot.Vessel.CanFire(0, cursor)) {
                pilot.Vessel.Fire(0, cursor);
            }
        }

        if (playerInput.IsActionPressed("weapon1")) {
            if (pilot.Vessel.CanFire(1, cursor)) {
                pilot.Vessel.Fire(1, cursor);
            }
        }

        if (playerInput.IsActionPressed("weapon2")) {
            if (pilot.Vessel.CanFire(2, cursor)) {
                pilot.Vessel.Fire(2, cursor);
            }
        }

        if (playerInput.IsActionPressed("special")) {
            if (pilot.Vessel.specialWeapon.CanFire(pilot.Vessel.State, cursor)) {
                pilot.Vessel.specialWeapon.Fire(pilot.Vessel.State, cursor);
            }
        }

        if (playerInput.IsActionPressed("cursorUp")) {
            _cursor.MoveMainCursor(0, -18);
        }
        if (playerInput.IsActionPressed("cursorDown")) {
            _cursor.MoveMainCursor(0, 18);
        }
        if (playerInput.IsActionPressed("cursorLeft")) {
            _cursor.MoveMainCursor(-18, 0);
        }
        if (playerInput.IsActionPressed("cursorRight")) {
            _cursor.MoveMainCursor(18, 0);
        }

        if (playerInput.IsActionPressed("attackCursorMotionLeft")) {
            var value = playerInput.GetJoyAxis(JoystickList.Axis0);
            _cursor.MoveAttackCursor(-18 * Mathf.Abs(value / 2), 0);
        }
        if (playerInput.IsActionPressed("attackCursorMotionRight")) {
            var value = playerInput.GetJoyAxis(JoystickList.Axis0);
            _cursor.MoveAttackCursor(18 * Mathf.Abs(value / 2), 0);
        }
        if (playerInput.IsActionPressed("attackCursorMotionUp")) {
            var value = playerInput.GetJoyAxis(JoystickList.Axis1);
            _cursor.MoveAttackCursor(0, -18 * Mathf.Abs(value / 2));
        }
        if (playerInput.IsActionPressed("attackCursorMotionDown")) {
            var value = playerInput.GetJoyAxis(JoystickList.Axis1);
            _cursor.MoveAttackCursor(0, 18 * Mathf.Abs(value / 2));
        }

        if (playerInput.IsActionJustPressed("targetLock")) {
            if (_targetLock.HasTarget()) {
                _targetLock.SetTarget(null);
            } else {
                var target = QMath.NearestEnemy(cursor, pilot);
                if (target != null) {
                    _targetLock.SetTarget(target.Vessel);
                }
                GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/target_locked.wav")));
            }
        }
    }

    // public override void _Input(InputEvent e) {
    //     if (e is InputEventKey keyEvent) {
    //         if (keyEvent.Scancode == (uint)Godot.KeyList.Space && !keyEvent.Pressed) {
    //             pilot.Vessel.ClearWaypoints();
    //         }
    //     }
    // }

    // public override void _UnhandledInput(InputEvent e) {
    //     if (e is InputEventMouseButton mouseEvent) {
    //         if (mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.Pressed) {
    //             var wp = Waypoint.New(true, null);
    //             wp.GlobalPosition = GetGlobalMousePosition();
    //             pilot.Vessel.AddWaypoint(wp);
    //             GetParent().AddChild(wp);
    //             GetTree().SetInputAsHandled();
    //             return;
    //         }
    //     }
    // }
}
