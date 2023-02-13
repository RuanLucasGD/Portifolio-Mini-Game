using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace MMV.Editor
{
    [CustomEditor(typeof(MMV_TrackedVehicle))]
    public class MMV_TrackedVehicleEditor : MMV_VehicleEditor
    {
        //--- properties
        private const string PROPERTY_NAME_WHEELS_LEFT = "wheels.wheelsLeft";
        private const string PROPERTY_NAME_WHEELS_RIGHT = "wheels.wheelsRight";
        private const string PROPERTY_NAME_ADDITIONAL_WHEELS_LEFT = "wheels.leftAdditionalWheelsRenderers";
        private const string PROPERTY_NAME_ADDITIONAL_WHEELS_RIGHT = "wheels.rightAdditionalWheelsRenderers";

        // --- wheels lists

        private ReorderableList leftWheelsList;
        private ReorderableList rightWheelsList;

        //--- editor variables

        private bool wheelsTracksExpanded;
        private bool wheelsParticlesExpanded;

        //--- properties

        private SerializedProperty leftWheels;
        private SerializedProperty rightWheels;
        private SerializedProperty leftAdditionalWheels;
        private SerializedProperty rightAdditionalWheels;

        private new MMV_TrackedVehicle Vehicle => (MMV_TrackedVehicle)base.Vehicle;

        protected override void SetupEditor()
        {
            base.SetupEditor();

            // configure default component properties
            {
                if (Vehicle.Engine == null) Vehicle.Engine = new MMV_TrackedEngine();
                if (Vehicle.Wheels == null) Vehicle.Wheels = new MMV_TrackedWheelManager();
            }

            // --- find properties

            leftWheels = serializedObject.FindProperty(PROPERTY_NAME_WHEELS_LEFT);
            rightWheels = serializedObject.FindProperty(PROPERTY_NAME_WHEELS_RIGHT);
            leftAdditionalWheels = serializedObject.FindProperty(PROPERTY_NAME_ADDITIONAL_WHEELS_LEFT);
            rightAdditionalWheels = serializedObject.FindProperty(PROPERTY_NAME_ADDITIONAL_WHEELS_RIGHT);

            // --- create wheels lists editor
            leftWheelsList = new ReorderableList(serializedObject, leftWheels, true, true, true, true);
            rightWheelsList = new ReorderableList(serializedObject, rightWheels, true, true, true, true);

            // --- how wheel lists should be rendered in the editor
            leftWheelsList = GetDrawableReordableList(leftWheelsList, "Left wheels");
            rightWheelsList = GetDrawableReordableList(rightWheelsList, "Right wheels");

            //-------------------------

            onDrawEngineTab = OnDrawEngineTab;
            onDrawWheelsTab = OnDrawWheelsTab;
            onDrawStabilityTab = OnDrawStabilityTab;
            onSaveEditorData = OnSaveEditorData;
            onLoadEditorData = OnLoadEditorData;

            onDrawVehicleStatus = () =>
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Velocity & Brake", MMV_EditorStyle.LabelBold);
                EditorGUILayout.LabelField($"Forward Velocity: {(int)Vehicle.VelocityKMH}");
                EditorGUILayout.LabelField($"Wheels Velocity: {(int)Vehicle.Wheels.WheelsVelocity.z}");
                EditorGUILayout.LabelField($"Acceleration Force: {(int)Vehicle.Engine.CurrentAcceleration}");
                EditorGUILayout.LabelField($"Brake Force: {(int)Vehicle.Engine.CurrentBrakeForce}");

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Movement", MMV_EditorStyle.LabelBold);
                EditorGUILayout.LabelField($"Is accelerating: {Vehicle.IsAccelerating}");
                EditorGUILayout.LabelField($"Is braking: {Vehicle.IsBraking}");
                EditorGUILayout.LabelField($"Is turning: {Vehicle.IsTurning}");
                EditorGUILayout.LabelField($"Is turning stoped: {Vehicle.IsTurningStoped}");

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Controls", MMV_EditorStyle.LabelBold);
                EditorGUILayout.LabelField($"Controls Enabled: {Vehicle.VehicleControlsEnabled}");
                EditorGUILayout.LabelField($"Vertical Input: {Vehicle.VerticalInput.ToString("0.0")}");
                EditorGUILayout.LabelField($"Horizontal Input: {Vehicle.HorizontalInput.ToString("0.0")}");

                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("AI", MMV_EditorStyle.LabelBold);
                EditorGUILayout.LabelField($"Is Stranded: {Vehicle.IsStranded}");

            };
        }

        private void OnDrawEngineTab()
        {
            DrawEngineAccelerationField(Vehicle.Engine);
            DrawEngineSoundField(Vehicle.Engine);
        }

        private void OnDrawWheelsTab()
        {
            var _wheelsManager = Vehicle.Wheels;
            _wheelsManager.Settings = (MMV_WheelSettings)EditorGUILayout.ObjectField("Wheel Settings", _wheelsManager.Settings, typeof(MMV_WheelSettings), allowSceneObjects: false);

            // tracks
            {
                wheelsTracksExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsTracksExpanded, "Tracks");

                if (wheelsTracksExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();

                    var _trackMoveSpeed = EditorGUILayout.FloatField("Multiply rotation velocity", _wheelsManager.TrackMoveSpeed);
                    var _leftTrack = (Renderer)EditorGUILayout.ObjectField("Left track", _wheelsManager.LeftTrack, typeof(SkinnedMeshRenderer), true);
                    var _rightTrack = (Renderer)EditorGUILayout.ObjectField("Right track", _wheelsManager.RightTrack, typeof(SkinnedMeshRenderer), true);

                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;

                    _wheelsManager.TrackMoveSpeed = _trackMoveSpeed;
                    _wheelsManager.LeftTrack = _leftTrack;
                    _wheelsManager.RightTrack = _rightTrack;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            DrawWheelsListField(leftWheelsList, rightWheelsList);

            // additional wheels
            {
                EditorGUILayout.PropertyField(leftAdditionalWheels);
                EditorGUILayout.PropertyField(rightAdditionalWheels);
            }

            // particles
            {
                wheelsParticlesExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsParticlesExpanded, "Wheels particles");

                if (wheelsParticlesExpanded)
                {
                    var _particleSystem = _wheelsManager.TracksParticles;

                    EditorGUI.indentLevel++;
                    EditorGUILayout.Separator();

                    var _leftParticle = (ParticleSystem)EditorGUILayout.ObjectField("Left particle", _particleSystem.LeftParticle, typeof(ParticleSystem), true);
                    var _rightParticle = (ParticleSystem)EditorGUILayout.ObjectField("Right particle", _particleSystem.RightParticle, typeof(ParticleSystem), true);
                    var _maxEmission = _particleSystem.MaxEmission;

                    if (_particleSystem.RightParticle || _particleSystem.LeftParticle)
                    {
                        _maxEmission = EditorGUILayout.FloatField("Max emission", _particleSystem.MaxEmission);
                    }

                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;

                    //------------------------------------------

                    _particleSystem.LeftParticle = _leftParticle;
                    _particleSystem.RightParticle = _rightParticle;
                    _particleSystem.MaxEmission = _maxEmission;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            Vehicle.Wheels = _wheelsManager;
        }

        protected override void DrawEngineAccelerationField(MMV_Engine engine)
        {
            base.DrawEngineAccelerationField(engine);
            var _trackedVehicleEngine = (MMV_TrackedEngine)engine;
            _trackedVehicleEngine.TurnSpeed = EditorGUILayout.Slider("Turn Speed", _trackedVehicleEngine.TurnSpeed, 0, 1);
        }

        private void OnDrawStabilityTab()
        {
            base.DrawStabilityField(Vehicle.Engine);
        }

        private void OnSceneGUI()
        {
            if (Vehicle.Wheels.Settings)
            {
                DrawWheelsHandles(Vehicle, Vehicle.Wheels.WheelsLeft, Vehicle.Wheels.Settings);
                DrawWheelsHandles(Vehicle, Vehicle.Wheels.WheelsRight, Vehicle.Wheels.Settings);
            }

            DrawStabilityHandles();
        }

        private void OnSaveEditorData()
        {
            EditorPrefs.SetBool(nameof(Vehicle) + nameof(wheelsTracksExpanded), wheelsTracksExpanded);
            EditorPrefs.SetBool(nameof(Vehicle) + nameof(wheelsParticlesExpanded), wheelsParticlesExpanded);
        }

        private void OnLoadEditorData()
        {
            wheelsTracksExpanded = EditorPrefs.GetBool(nameof(Vehicle) + nameof(wheelsTracksExpanded));
            wheelsParticlesExpanded = EditorPrefs.GetBool(nameof(Vehicle) + nameof(wheelsParticlesExpanded));
        }

        /// <summary>
        /// Describes how wheel lists should be rendered in the vehicle editor
        /// </summary>
        /// <param name="list">The list to be rendered</param>
        /// <param name="listName">Field name to be rendered</param>
        /// <returns></returns>
        protected override ReorderableList GetDrawableReordableList(ReorderableList list, string listName)
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

                rect.width = 0;

                rect.x += rect.width + OFFSET;
                rect.width = 40;
                EditorGUI.LabelField(rect, "mesh");

                rect.x += rect.width + OFFSET;
                rect.width = 100;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("mesh"), GUIContent.none);

                rect.x += rect.width + OFFSET;
                rect.width = 40;
                EditorGUI.LabelField(rect, "bone");

                rect.x += rect.width + OFFSET;
                rect.width = 100;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("bone"), GUIContent.none);
            };

            return list;
        }
    }
}