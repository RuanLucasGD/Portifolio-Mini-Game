using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

namespace MMV.Editor
{
    public class MMV_VehicleEditor : UnityEditor.Editor
    {
        // --- handles colors

        protected Color suspensionColorHandle = Color.green;
        protected Color springPosColorHandle = Color.yellow;
        protected Color wheelCircleColorHandle = Color.green;
        protected Color accelerationDirectionColorHandle = Color.yellow;
        protected Color centerOfMassColorHandle = Color.yellow;

        //--- vehicle editor constants

        protected const float RIGID_BODY_DEFAULT_MASS = 1;
        protected const float VEHICLE_DEFAULT_MASS = 1000;

        //--- editor variables

        private int currentEditorTab;
        private bool engineSoundExpanded;
        private bool wheelsLeftWheelsExpanded;
        private bool wheelsRightWheelsExpanded;

        private bool vehicleStatusExpanded;

        protected Action onDrawEngineTab;
        protected Action onDrawWheelsTab;
        protected Action onDrawStabilityTab;
        protected Action onSaveEditorData;
        protected Action onLoadEditorData;

        protected Action onDrawVehicleStatus;

        protected int CurrentTab => currentEditorTab;
        protected MMV_Vehicle Vehicle => (MMV_Vehicle)target;

        private void GetVehicleByType(out MMV_TrackedVehicle trackedVehicle, out MMV_WheeledVehicle wheeledVehicle)
        {
            var _isTrackedVehicle = Vehicle is MMV_TrackedVehicle;
            trackedVehicle = _isTrackedVehicle ? (MMV_TrackedVehicle)Vehicle : null;
            wheeledVehicle = !_isTrackedVehicle ? (MMV_WheeledVehicle)Vehicle : null;
        }

        protected virtual void SetupEditor()
        {
            if (onDrawVehicleStatus == null) onDrawVehicleStatus = () => { };

            if (onDrawEngineTab == null) onDrawEngineTab = () => { };
            if (onDrawWheelsTab == null) onDrawWheelsTab = () => { };
            if (onDrawStabilityTab == null) onDrawStabilityTab = () => { };
            if (onSaveEditorData == null) onSaveEditorData = () => { };
            if (onLoadEditorData == null) onLoadEditorData = () => { };
        }

        private void OnEnable()
        {
            SetupEditor();

            // configure vehicle on add this component 
            {
                var _rb = Vehicle.GetComponentInChildren<Rigidbody>();
                if (!_rb) _rb = Vehicle.gameObject.AddComponent<Rigidbody>();

                if (_rb.mass == RIGID_BODY_DEFAULT_MASS) _rb.mass = VEHICLE_DEFAULT_MASS;
            }

            // --- load data
            LoadEditorData();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // tabs selector 
            EditorGUI.BeginChangeCheck();
            {
                currentEditorTab = GUILayout.Toolbar(CurrentTab, new string[] { "Engine", "Wheels", "Stability" });
            }

            if (EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
            }


            //--- drawind editors

            if (CurrentTab == 0) onDrawEngineTab();
            else if (CurrentTab == 1) onDrawWheelsTab();
            else if (CurrentTab == 2) onDrawStabilityTab();

            vehicleStatusExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(vehicleStatusExpanded, "Vehicle Status");
            if (vehicleStatusExpanded)
            {
                EditorGUI.indentLevel++;
                onDrawVehicleStatus();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // apply all modifications
            SaveEditorData();
            serializedObject.ApplyModifiedProperties();

            if (EditorUtility.IsDirty(Vehicle))
            {
                EditorUtility.SetDirty(Vehicle);
            }
        }

        protected virtual void DrawEngineAccelerationField(MMV_Engine engine)
        {
            engine.EngineSettings = (MMV_EngineSettings)EditorGUILayout.ObjectField("Engine Settings", engine.EngineSettings, typeof(MMV_EngineSettings), allowSceneObjects: false);
        }

        protected void DrawEngineSoundField(MMV_Engine engine)
        {
            engineSoundExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(engineSoundExpanded, "Engine Sound");

            if (engineSoundExpanded)
            {
                const float OFFSET_SOUND = 0.1f;

                EditorGUILayout.Separator();
                EditorGUI.indentLevel++;

                var _audioPlayer = (AudioSource)EditorGUILayout.ObjectField("Audio Source", engine.EngineSound.AudioPlayer, typeof(AudioSource), true);

                if (engine.EngineSound.AudioPlayer)
                {
                    var _audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", engine.EngineSound.Sound, typeof(AudioClip), true);

                    EditorGUILayout.Separator();

                    var _basePitch = EditorGUILayout.Slider("Min Pitch", engine.EngineSound.BasePitch, MMV_Engine.MIN_SOUND_PITCH, engine.EngineSound.MaxPitch - OFFSET_SOUND);
                    var _maxPitch = EditorGUILayout.Slider("Max Pitch", engine.EngineSound.MaxPitch, _basePitch + OFFSET_SOUND, MMV_Engine.MAX_SOUND_PITCH);

                    engine.EngineSound.BasePitch = _basePitch;
                    engine.EngineSound.MaxPitch = _maxPitch;
                    engine.EngineSound.Sound = _audioClip;
                }

                EditorGUI.indentLevel--;

                engine.EngineSound.AudioPlayer = _audioPlayer;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected virtual void DrawWheelsListField(ReorderableList leftWheelsList, ReorderableList rightWheelsList)
        {
            wheelsLeftWheelsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsLeftWheelsExpanded, "Left wheels");

            if (wheelsLeftWheelsExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Separator();
                leftWheelsList.DoLayoutList();  // drawing list
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            wheelsRightWheelsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsRightWheelsExpanded, "Right wheels");

            if (wheelsRightWheelsExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Separator();
                rightWheelsList.DoLayoutList(); // drawing list
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected virtual void DrawStabilityField(MMV_Engine engine)
        {
            EditorGUILayout.HelpBox("This setting is only applied at start", MessageType.Info);
            engine.DecelerationByAngle = EditorGUILayout.FloatField("Angle deceleration", engine.DecelerationByAngle);
            engine.AngleDecelerationByAngleCurve = EditorGUILayout.CurveField("Angle deceleration curve", engine.AngleDecelerationByAngleCurve);
            Vehicle.CenterOfMassUp = EditorGUILayout.FloatField("COM Height", Vehicle.CenterOfMassUp);
            Vehicle.CenterOfMassForward = EditorGUILayout.FloatField("COM Forward", Vehicle.CenterOfMassForward);
        }

        protected virtual ReorderableList GetDrawableReordableList(ReorderableList list, string listName)
        {
            // list name
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, listName);
            };

            // drawing elements 
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                const int OFFSET = 5;

                rect.height = EditorGUIUtility.singleLineHeight;

                // get the elements of the list indexes
                var _wheel = list.serializedProperty.GetArrayElementAtIndex(index);

                // drawing element of the list

                rect.width = 50 + OFFSET;
                EditorGUI.LabelField(rect, "Label");
            };

            return list;
        }

        protected void DrawWheelsHandles(MMV_Vehicle vehicle, MMV_Wheel[] wheels, MMV_WheelSettings settings)
        {
            if (!settings)
            {
                return;
            }

            foreach (var w in wheels)
            {
                if (w.Mesh)
                {
                    var _wheelHeight = settings.SpringHeight;
                    var _wheelRadius = settings.WheelRadius;
                    var _wheelLength = settings.SpringLength;

                    var _colliderPos = w.Mesh.position;
                    var _wheelUp = vehicle.transform.up;
                    var _onGrounded = w.OnGronded;
                    var _hitPos = w.WheelHit.point;
                    var _springPos = _colliderPos + (vehicle.transform.up * settings.SpringHeight);

                    // --- wheel position

                    var _onAirPos = w.Mesh.position + (-_wheelUp * settings.SpringLength);
                    var _onGroundedPos = _colliderPos + (-_wheelUp * (w.WheelHit.distance - _wheelHeight - _wheelRadius));
                    var _finalWheelPos = _onGrounded ? _onGroundedPos : _onAirPos;
                    var _wheelHitPos = _finalWheelPos + (-_wheelUp * _wheelRadius);

                    // suspension start position 
                    Handles.color = springPosColorHandle;
                    Handles.DrawLine(w.Mesh.position, _springPos);

                    // spring length
                    Handles.color = suspensionColorHandle;
                    Handles.DrawLine(w.Mesh.position, _finalWheelPos);

                    // wheel circle
                    Handles.color = wheelCircleColorHandle;
                    Handles.DrawWireArc(_finalWheelPos, vehicle.transform.right, Vehicle.transform.forward, 360, _wheelRadius);

                    if (Application.isPlaying)
                    {
                        // Acceleration Direction
                        Handles.color = accelerationDirectionColorHandle;
                        Handles.DrawLine(_wheelHitPos, _wheelHitPos + w.WheelForward);
                    }
                }
            }
        }

        protected void DrawStabilityHandles()
        {
            if (!Vehicle.Rb)
            {
                return;
            }

            var _center = Vehicle.transform.position;

            if (Application.isPlaying)
            {
                _center = Vehicle.Rb.worldCenterOfMass;
            }

            Handles.color = centerOfMassColorHandle;
            Handles.DrawWireCube(_center, Vector3.one * 0.3f);
        }

        protected void ShowEngineAccelerationField(MMV_Engine engine)
        {
            EditorGUILayout.Separator();
            DrawEngineAccelerationField(engine);
        }

        private void SaveEditorData()
        {
            EditorPrefs.SetInt(nameof(Vehicle) + nameof(currentEditorTab), currentEditorTab);
            EditorPrefs.SetBool(nameof(Vehicle) + nameof(engineSoundExpanded), engineSoundExpanded);

            EditorPrefs.SetBool(nameof(Vehicle) + nameof(wheelsLeftWheelsExpanded), wheelsLeftWheelsExpanded);
            EditorPrefs.SetBool(nameof(Vehicle) + nameof(wheelsRightWheelsExpanded), wheelsRightWheelsExpanded);

            EditorPrefs.SetBool(nameof(Vehicle) + nameof(vehicleStatusExpanded), vehicleStatusExpanded);

            onSaveEditorData();
        }

        private void LoadEditorData()
        {
            currentEditorTab = EditorPrefs.GetInt(nameof(Vehicle) + nameof(currentEditorTab));
            engineSoundExpanded = EditorPrefs.GetBool(nameof(Vehicle) + nameof(engineSoundExpanded));

            wheelsLeftWheelsExpanded = EditorPrefs.GetBool(nameof(Vehicle) + nameof(wheelsLeftWheelsExpanded));
            wheelsRightWheelsExpanded = EditorPrefs.GetBool(nameof(Vehicle) + nameof(wheelsRightWheelsExpanded));

            vehicleStatusExpanded = EditorPrefs.GetBool(nameof(Vehicle) + nameof(vehicleStatusExpanded));

            onLoadEditorData();
        }

    }
}
