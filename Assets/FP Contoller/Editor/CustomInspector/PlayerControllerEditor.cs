using UnityEditor;
using UnityEngine;
/// <summary>
/// Draws all of the players stored information in tabs to not cover the inspector with usless variables
/// </summary>
[CustomEditor(typeof(PlayerController))]
[CanEditMultipleObjects]
public class PlayerControllerEditor : Editor
{
    /// <summary>
    /// Which tab is currently open
    /// </summary>
    private int toolbar;
    /// <summary>
    /// Which tab of the parkour toolbar is currently open
    /// </summary>
    private int parkourToolbar;
    
    #region Hitbox
    SerializedProperty colliderInfo;
    #endregion

    #region Speed

    SerializedProperty speedCap;
    SerializedProperty minSpeed;
    SerializedProperty acceleration;
    SerializedProperty belowMinAcceleration;
    SerializedProperty decelleration;
    SerializedProperty gravity;
    SerializedProperty jumpForce;

    #endregion

    #region Rotations

    SerializedProperty cameraFollower;
    SerializedProperty sensitivity;
    SerializedProperty minTurnTime;
    SerializedProperty forceRotateTime;

    #endregion

    #region Parkour

    #region Running
    SerializedProperty stepHeight;
    #endregion

    #region LedgeActions

    SerializedProperty pullUpSpeed;
    SerializedProperty shimmySpeed;
    SerializedProperty lowLedgeHeight;
    SerializedProperty ledgeSpaceRequire;
    SerializedProperty ledgeReach;
    SerializedProperty lowerGrabDist;

    #endregion

    #region WallRun
    SerializedProperty jumpOffPercent;
    SerializedProperty camAng;
    #endregion

    #region WallClimb
    SerializedProperty vertSpeedCancel;
    SerializedProperty floatTime;
    SerializedProperty maxDist;
    SerializedProperty minDist;
    SerializedProperty climbSpeed;
    SerializedProperty wallClimbAngle;
    #endregion

    #region
    SerializedProperty vaultHeight;
    SerializedProperty vaultDistance;
    #endregion

    #endregion

    #region Misc

    SerializedProperty speedText;
    SerializedProperty respawnPoint;
    SerializedProperty inputTime;

    #endregion
    /// <summary>
    /// Assign all the serialzable properties from the player
    /// </summary>
    private void OnEnable()
    {
        #region Hitbox
        colliderInfo = serializedObject.FindProperty("colInfo");
        #endregion

        #region Speed
        speedCap = serializedObject.FindProperty("direction");
        minSpeed = serializedObject.FindProperty("minSpeed");
        acceleration = serializedObject.FindProperty("acceleration");
        belowMinAcceleration = serializedObject.FindProperty("belowMinAcceleration");
        decelleration = serializedObject.FindProperty("decelleration");
        gravity = serializedObject.FindProperty("gravity");
        jumpForce = serializedObject.FindProperty("jumpForce");
        #endregion

        #region Rotation
        cameraFollower = serializedObject.FindProperty("camFol");
        sensitivity = serializedObject.FindProperty("sensitivity");
        minTurnTime = serializedObject.FindProperty("minTurnTime");
        forceRotateTime = serializedObject.FindProperty("forceRotateTime");
        #endregion

        #region Misc
        speedText = serializedObject.FindProperty("speedText");
        respawnPoint = serializedObject.FindProperty("respawnPosition");
        inputTime = serializedObject.FindProperty("inputTime");
        #endregion
        stepHeight = serializedObject.FindProperty("stepHeight");
        pullUpSpeed = serializedObject.FindProperty("pullUpSpeed");
        shimmySpeed = serializedObject.FindProperty("shimmySpeed");
        jumpOffPercent = serializedObject.FindProperty("jumpOffPercent");
        vertSpeedCancel = serializedObject.FindProperty("vertSpeedCancel");
        floatTime = serializedObject.FindProperty("floatTime");
        maxDist = serializedObject.FindProperty("maxDist");
        minDist = serializedObject.FindProperty("minDist");
        climbSpeed = serializedObject.FindProperty("climbSpeed");
        lowLedgeHeight = serializedObject.FindProperty("lowLedgeHeight");
        camAng = serializedObject.FindProperty("cameraAngle");
        wallClimbAngle = serializedObject.FindProperty("wallClimbAngle");
        ledgeSpaceRequire = serializedObject.FindProperty("openSpaceRequired");
        ledgeReach = serializedObject.FindProperty("atLedgeDistance");
        vaultDistance = serializedObject.FindProperty("vaultDistance");
        vaultHeight = serializedObject.FindProperty("playerVaultHeight");
        lowerGrabDist = serializedObject.FindProperty("lowerGrabDist");
    }
    /// <summary>
    /// Draw the inspector tabs with its specific stuff
    /// </summary>
    public override void OnInspectorGUI()
    {
        toolbar = GUILayout.Toolbar(toolbar, new string[] { "Hitbox", "Speed", "Rotation", "Parkour", "Misc"});

        serializedObject.Update();
        switch (toolbar)
        {
            case 0:
                EditorGUILayout.PropertyField(colliderInfo);
                break;
            case 1:
                EditorGUILayout.PropertyField(speedCap);
                EditorGUILayout.PropertyField(minSpeed);
                EditorGUILayout.PropertyField(acceleration);
                EditorGUILayout.PropertyField(belowMinAcceleration);
                EditorGUILayout.PropertyField(decelleration);
                EditorGUILayout.PropertyField(gravity);
                EditorGUILayout.PropertyField(jumpForce);
                break;
            case 2:
                EditorGUILayout.PropertyField(cameraFollower);
                EditorGUILayout.PropertyField(sensitivity);
                EditorGUILayout.PropertyField(minTurnTime);
                EditorGUILayout.PropertyField(forceRotateTime);
                break;
            case 3:

                parkourToolbar = GUILayout.Toolbar(parkourToolbar, new string[] { "Running", "Wall Run", "Wall Climb", "Vault&StepUp", "Ledge Info" });

                switch(parkourToolbar)
                {
                    case 0:
                        break;
                    case 1:
                        EditorGUILayout.PropertyField(jumpOffPercent);
                        EditorGUILayout.PropertyField(camAng);
                        break;
                    case 2:
                        EditorGUILayout.PropertyField(wallClimbAngle);
                        EditorGUILayout.PropertyField(climbSpeed);
                        EditorGUILayout.PropertyField(maxDist);
                        EditorGUILayout.PropertyField(minDist);
                        EditorGUILayout.PropertyField(floatTime);
                        EditorGUILayout.PropertyField(vertSpeedCancel);
                        break;
                    case 3:
                        EditorGUILayout.PropertyField(stepHeight);
                        EditorGUILayout.PropertyField(lowLedgeHeight);
                        EditorGUILayout.PropertyField(ledgeSpaceRequire);
                        EditorGUILayout.PropertyField(vaultDistance);
                        EditorGUILayout.PropertyField(vaultHeight);
                        break;
                    case 4:
                        EditorGUILayout.PropertyField(lowerGrabDist);
                        EditorGUILayout.PropertyField(pullUpSpeed);
                        EditorGUILayout.PropertyField(shimmySpeed);
                        EditorGUILayout.PropertyField(ledgeSpaceRequire);
                        EditorGUILayout.PropertyField(ledgeReach);
                        EditorGUILayout.PropertyField(stepHeight);
                        break;
                }

                break;
            case 4:
                EditorGUILayout.PropertyField(speedText);
                EditorGUILayout.PropertyField(respawnPoint);
                EditorGUILayout.PropertyField(inputTime);
                break;
            default:
                DrawDefaultInspector();
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
