using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace MMV.Editor
{
    [CustomEditor(typeof(MMV_WheeledVehicle))]
    public class MMV_WheeledVehicleEditor : MMV_VehicleEditor
    {
        //--- properties
        private const string PROPERTY_NAME_WHEELS_LEFT = "wheels.wheelsLeft";
        private const string PROPERTY_NAME_WHEELS_RIGHT = "wheels.wheelsRight";

        // --- wheels lists

        private ReorderableList leftWheelsList;
        private ReorderableList rightWheelsList;

        // --- expanded fields
        private bool wheelsParticlesExpanded;

        //--- properties

        private SerializedProperty leftWheels;
        private SerializedProperty rightWheels;

        private new MMV_WheeledVehicle Vehicle => (MMV_WheeledVehicle)base.Vehicle;

        protected override void SetupEditor()
        {
            base.SetupEditor();

            // configure vehicle on add this component 
            {
                if (Vehicle.Engine == null) Vehicle.Engine = new MMV_WheeledEngine();
                if (Vehicle.Wheels == null) Vehicle.Wheels = new MMV_WheeledWheelManager();
            }

            // --- find properties

            leftWheels = serializedObject.FindProperty(PROPERTY_NAME_WHEELS_LEFT);
            rightWheels = serializedObject.FindProperty(PROPERTY_NAME_WHEELS_RIGHT);

            // --- create wheels lists editor
            leftWheelsList = new ReorderableList(serializedObject, leftWheels, true, true, true, true);
            rightWheelsList = new ReorderableList(serializedObject, rightWheels, true, true, true, true);

            leftWheelsList = GetDrawableReordableList(leftWheelsList, "left wheels");
            rightWheelsList = GetDrawableReordableList(rightWheelsList, "right wheels");

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

        private void OnSceneGUI()
        {
            if (Vehicle.Wheels.Settings)
            {
                DrawWheelsHandles(Vehicle, Vehicle.Wheels.WheelsLeft, Vehicle.Wheels.Settings);
                DrawWheelsHandles(Vehicle, Vehicle.Wheels.WheelsRight, Vehicle.Wheels.Settings);
            }

            DrawStabilityHandles();
        }

        private void OnDrawWheelsTab()
        {
            var _wheelsManager = Vehicle.Wheels;
            var _settings = (MMV_WheelSettings)EditorGUILayout.ObjectField("settings", _wheelsManager.Settings, typeof(MMV_WheelSettings), allowSceneObjects: false);

            DrawWheelsListField(leftWheelsList, rightWheelsList);
            DrawWheelsParticleField(Vehicle.Wheels);

            _wheelsManager.Settings = _settings;
        }

        private void OnDrawStabilityTab()
        {
            base.DrawStabilityField(Vehicle.Engine);
        }

        private void DrawWheelsParticleField(MMV_WheeledWheelManager wheels)
        {
            // synchronizes the wheel particle list size with the wheel list size, so there 
            // will always be one particle for each wheel
            void SyncPartcilesWithWheels(MMV_Wheel[] wheels, List<ParticleSystem> particles)
            {
                var _wheelsAmount = wheels.Length;
                var _particlesAmount = particles.Count;

                if (_wheelsAmount != _particlesAmount)
                {
                    while (_particlesAmount > _wheelsAmount)
                    {
                        particles.RemoveAt(_particlesAmount - 1);
                        _particlesAmount--;
                    }

                    while (_particlesAmount < _wheelsAmount)
                    {
                        particles.Add(null);
                        _particlesAmount++;
                    }
                }
            }

            if (Vehicle.Wheels.Particles.LeftWheelsParticles == null) Vehicle.Wheels.Particles.LeftWheelsParticles = new List<ParticleSystem>();
            if (Vehicle.Wheels.Particles.RightWheelsParticles == null) Vehicle.Wheels.Particles.RightWheelsParticles = new List<ParticleSystem>();

            SyncPartcilesWithWheels(Vehicle.Wheels.WheelsLeft, Vehicle.Wheels.Particles.LeftWheelsParticles);
            SyncPartcilesWithWheels(Vehicle.Wheels.WheelsRight, Vehicle.Wheels.Particles.RightWheelsParticles);

            wheelsParticlesExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(wheelsParticlesExpanded, "Wheels particles");

            if (wheelsParticlesExpanded)
            {
                void DrawWheelParticleField(MMV_Wheel[] wheels, List<ParticleSystem> particles)
                {
                    if (wheels.Length == 0)
                    {
                        EditorGUILayout.LabelField("Add some wheel to the vehicle to be able to complement particles");
                    }
                    else
                    {
                        for (int i = 0; i < wheels.Length; i++)
                        {
                            if (wheels[i].Mesh)
                            {
                                var _p = (ParticleSystem)EditorGUILayout.ObjectField(wheels[i].Mesh.name, particles[i], typeof(ParticleSystem), true);
                                particles[i] = _p;
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"Add a Transform for the index {i} wheel collider");
                            }

                        }
                    }
                }

                EditorGUILayout.LabelField("Dust particles", MMV_EditorStyle.LabelBold);
                EditorGUI.indentLevel++;

                wheels.Particles.EmissionIntensity = EditorGUILayout.Slider("Emission intensity", wheels.Particles.EmissionIntensity, 0, 10);

                EditorGUILayout.LabelField("Left wheels", MMV_EditorStyle.LabelBold);
                EditorGUI.indentLevel++;

                DrawWheelParticleField(Vehicle.Wheels.WheelsLeft, Vehicle.Wheels.Particles.LeftWheelsParticles);

                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Right wheels", MMV_EditorStyle.LabelBold);
                EditorGUI.indentLevel++;

                DrawWheelParticleField(Vehicle.Wheels.WheelsRight, Vehicle.Wheels.Particles.RightWheelsParticles);

                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OnSaveEditorData()
        {
            EditorPrefs.SetBool(nameof(Vehicle) + nameof(wheelsParticlesExpanded), wheelsParticlesExpanded);
        }

        private void OnLoadEditorData()
        {
            wheelsParticlesExpanded = EditorPrefs.GetBool(nameof(Vehicle) + nameof(wheelsParticlesExpanded));
        }

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
                rect.width = 100;
                EditorGUI.LabelField(rect, "accelerate");

                rect.x += rect.width - 30;
                rect.width = 100;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("applyAcceleration"), GUIContent.none);

                rect.x += rect.width - 70;
                rect.width = 100;
                EditorGUI.LabelField(rect, "brake");

                rect.x += rect.width - 55;
                rect.width = 100;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("applyBrake"), GUIContent.none);

                rect.x += rect.width - 70;
                rect.width = 70;
                EditorGUI.LabelField(rect, "steer angle");

                rect.x += rect.width + OFFSET;
                rect.width = 30;
                EditorGUI.PropertyField(rect, _wheel.FindPropertyRelative("maxSteerAngle"), GUIContent.none);
            };

            return list;
        }
    }
}
