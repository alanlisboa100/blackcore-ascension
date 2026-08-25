using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityControl : MonoBehaviour {

    private LayerMask GroundMask;
    private LayerMask EntityMask;
    private Camera MainCamera;

    private PendingAction CurrentPendingAction = new PendingAction.None();

    public Entity Entity;

    private CursorRenderer CursorRenderer;
    private GridRenderer GridRenderer;
    private PathFinder PathFinder;
    private Entity HoveredEntity;
    private Entity SelectedEntity;
    private Vector2 MobileMoveInput;
    private float NextMobileMoveRequestAt;
    private const float MOBILE_MOVE_DEADZONE = 0.2f;
    private const float MOBILE_MOVE_REQUEST_INTERVAL = 0.08f;


    private void Awake() {
        CursorRenderer = FindObjectOfType<CursorRenderer>();
        GridRenderer = FindObjectOfType<GridRenderer>();
        PathFinder = FindObjectOfType<PathFinder>();
        MainCamera = Camera.main;
    }

    void Start() {
        GroundMask = LayerMask.GetMask("Ground");
        EntityMask = LayerMask.GetMask("NPC", "Monsters", "Items", "Characters");
    }

    // Centralized pointer handling: one physics raycast per frame instead of
    // one raycast per visible Entity plus two more here.
    void Update() {
        ResolveRuntimeReferences();
        ProcessKeyboardInput();
        ProcessMobileMoveInput();

        if (MainCamera == null || GridRenderer == null || PathFinder == null || CursorRenderer == null) {
            SetHoveredEntity(null);
            return;
        }

        if (!PointerInput.TryGetPrimaryPointer(out var pointer)) {
            SetHoveredEntity(null);
            GridRenderer.Hide();
            return;
        }

        if (PointerInput.IsPointerOverUI(pointer.PointerId)) {
            SetHoveredEntity(null);
            GridRenderer.Hide();
            return;
        }

        var ray = MainCamera.ScreenPointToRay(pointer.Position);
        var didHitAnything = Physics.Raycast(ray, out var hit, 150, EntityMask | GroundMask);
        var isActionRequested = pointer.PressedThisFrame;

        if (!didHitAnything) {
            SetHoveredEntity(null);
            GridRenderer.Hide();

            if (isActionRequested && CurrentPendingAction is PendingAction.TargetSelection) {
                CurrentPendingAction = new PendingAction.None();
            }
            return;
        }

        var target = hit.collider.GetComponentInParent<SpriteEntityViewer>();
        SetHoveredEntity(target?.Entity);

        if (isActionRequested && CurrentPendingAction is PendingAction.TargetSelection && target == null) {
            CurrentPendingAction = new PendingAction.None();
        }

        if (target != null) {
            GridRenderer.Hide();

            if (CurrentPendingAction is PendingAction.None) {
                switch (target.Entity.Type) {
                    case EntityType.NPC:
                        CursorRenderer.SetAction(CursorAction.TALK, false);
                        break;
                    case EntityType.ITEM:
                        CursorRenderer.SetAction(CursorAction.PICK, true);
                        break;
                    case EntityType.MOB:
                        CursorRenderer.SetAction(CursorAction.ATTACK, false);
                        break;
                    case EntityType.WARP:
                        CursorRenderer.SetAction(CursorAction.WARP, false);
                        break;
                }
            }

            if (isActionRequested) {
                ProcessEntityClick(target.Entity);
            }
        } else if (CurrentPendingAction is PendingAction.TargetSelection) {
            GridRenderer.Hide();
            CursorRenderer.SetAction(CursorAction.TARGET, false);
        } else {
            GridRenderer.SetPointerWorldPosition(hit.point);
            CursorRenderer.SetAction(CursorAction.DEFAULT, true);

            if (!GridRenderer.IsCurrentPositionValid) {
                CursorRenderer.SetAction(CursorAction.INVALID, false);
            }

            if (isActionRequested) {
                Entity.RequestMove(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.z), 0);
            }
        }
    }

    private void ResolveRuntimeReferences() {
        if (GridRenderer == null) {
            GridRenderer = FindObjectOfType<GridRenderer>();
        }
        if (MainCamera == null) {
            MainCamera = Camera.main;
        }
        if (PathFinder == null) {
            PathFinder = FindObjectOfType<PathFinder>();
        }
        if (CursorRenderer == null) {
            CursorRenderer = FindObjectOfType<CursorRenderer>();
        }
    }

    private void SetHoveredEntity(Entity entity) {
        if (HoveredEntity == entity) {
            return;
        }

        HoveredEntity?.SetPointerHover(false);
        HoveredEntity = entity;
        HoveredEntity?.SetPointerHover(true);
    }

    private void OnDisable() {
        MobileMoveInput = Vector2.zero;
        SetHoveredEntity(null);
        GridRenderer?.Hide();
    }

    private void ProcessKeyboardInput() {
        // Event.current is only reliable during OnGUI. Input.GetKeyUp works in Update.
        if (Input.GetKeyUp(KeyCode.Insert)) {
            RequestSitStand();
        }
    }

    private void RequestSitStand() {
        new CZ.REQUEST_ACT2() {
            action = Entity.EntityViewer.State == SpriteState.Sit ? EntityActionType.STAND : EntityActionType.SIT,
            TargetID = Entity.GID
        }.Send();
    }

    private void ProcessEntityClick(Entity target) {
        if (target == null) {
            return;
        }

        SelectedEntity = target;

        switch (target.Type) {
            case EntityType.NPC:
                new CZ.CONTACTNPC() {
                    NAID = target.AID,
                    Type = 1
                }.Send();
                break;
            case EntityType.ITEM:
                CursorRenderer.SetAction(CursorAction.PICK, false, 2);

                OutPacket pickPacket = new CZ.ITEM_PICKUP2() { ID = (int) target.AID };
                if (Vector3.Distance(transform.position, target.transform.position) > 2) {
                    Entity.AfterMoveAction = delegate {
                        pickPacket.Send();
                    };

                    new CZ.REQUEST_MOVE2() {
                        x = (short) target.transform.position.x,
                        y = (short) target.transform.position.z,
                        dir = 0
                    }.Send();

                    break;
                }

                pickPacket.Send();
                Entity.LookTo(target.transform.position);
                break;
            case EntityType.MOB:
                // TODO render lock arrow

                List<PathNode> path;
                OutPacket actionPacket;

                if (CurrentPendingAction is PendingAction.TargetSelection TargetSelection) {
                    path = PathFinder.GetPath(Entity.transform.position, target.transform.position, TargetSelection.SkillInfo.AttackRange + 1);
                    actionPacket = new CZ.USE_SKILL2() {
                        SkillId = TargetSelection.SkillInfo.SkillID,
                        SelectedLevel = TargetSelection.Level,
                        TargetId = (int) target.AID
                    };
                } else {
                    path = PathFinder.GetPath(Entity.transform.position, target.transform.position, Entity.GetBaseStatus().attackRange + 1);
                    actionPacket = new CZ.REQUEST_ACT2() {
                        TargetID = target.AID,
                        action = EntityActionType.CONTINUOUS_ATTACK
                    };
                }

                Action actionDelegate = delegate {
                    actionPacket.Send();
                    CurrentPendingAction = new PendingAction.None();
                };

                if (path.Count == 0) {
                    return;
                } else if (path.Count <= 1) {
                    actionDelegate.Invoke();
                } else {
                    PathNode endNode = path[path.Count - 1];

                    Entity.AfterMoveAction = actionDelegate;

                    new CZ.REQUEST_MOVE2() {
                        x = (short) endNode.x,
                        y = (short) endNode.z,
                        dir = (byte) Entity.Direction
                    }.Send();
                }

                break;
            case EntityType.WARP:
                break;
        }

    }

    public void SetMobileMoveInput(Vector2 input) {
        MobileMoveInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void RequestBasicAttackSelected() {
        if (SelectedEntity == null || !SelectedEntity.gameObject.activeInHierarchy || SelectedEntity.Type != EntityType.MOB) {
            return;
        }

        // Attack button explicitly cancels a pending targeted skill instead of accidentally casting it.
        CurrentPendingAction = new PendingAction.None();
        ProcessEntityClick(SelectedEntity);
    }

    private void ProcessMobileMoveInput() {
        if (MobileMoveInput.sqrMagnitude < MOBILE_MOVE_DEADZONE * MOBILE_MOVE_DEADZONE) {
            return;
        }

        if (Entity == null || Entity.IsWalking || PathFinder == null || MainCamera == null) {
            return;
        }

        if (Time.unscaledTime < NextMobileMoveRequestAt) {
            return;
        }
        NextMobileMoveRequestAt = Time.unscaledTime + MOBILE_MOVE_REQUEST_INTERVAL;

        var forward = MainCamera.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) {
            forward = Vector3.forward;
        } else {
            forward.Normalize();
        }

        var right = MainCamera.transform.right;
        right.y = 0f;
        if (right.sqrMagnitude < 0.001f) {
            right = Vector3.right;
        } else {
            right.Normalize();
        }

        var desired = (right * MobileMoveInput.x + forward * MobileMoveInput.y).normalized;
        int startX = Mathf.RoundToInt(Entity.transform.position.x);
        int startY = Mathf.RoundToInt(Entity.transform.position.z);
        int targetX = startX + Mathf.RoundToInt(desired.x * 2f);
        int targetY = startY + Mathf.RoundToInt(desired.z * 2f);

        if (targetX == startX && targetY == startY) {
            return;
        }

        List<PathNode> path;
        try {
            path = PathFinder.GetPath(startX, startY, targetX, targetY);
        } catch (System.Exception) {
            return;
        }

        if (path == null || path.Count == 0) {
            return;
        }

        // Keep joystick requests short so changing direction feels responsive and does not spam long paths.
        var endNode = path[Mathf.Min(path.Count - 1, 1)];
        Entity.RequestMove(endNode.x, endNode.z, (int) Entity.Direction);
    }

    internal void UseSkill(SkillInfo skillInfo, short level) {
        if ((skillInfo.SkillType & (int) SkillTargetType.Self) > 0) {
            new CZ.USE_SKILL2() {
                SkillId = skillInfo.SkillID,
                SelectedLevel = level,
                TargetId = (int) Entity.GID
            }.Send();
        }

        if ((skillInfo.SkillType & (int) SkillTargetType.Target) > 0) {
            CurrentPendingAction = new PendingAction.TargetSelection(skillInfo, level);
        }
    }

    public partial class PendingAction {

        public class None : PendingAction { }

        public class TargetSelection : PendingAction {
            public SkillInfo SkillInfo;
            public short Level;

            public TargetSelection(SkillInfo skillInfo, short level) {
                SkillInfo = skillInfo;
                Level = level;
            }
        }

    }

}
