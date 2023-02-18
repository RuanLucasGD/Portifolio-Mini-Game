using UnityEngine;
using UnityEditor;

namespace MMV.Editor
{
    public class MMV_EditorStyle
    {
        private MMV_EditorStyle() { }

        public static GUIStyle Label
        {
            get
            {
                var _style = new GUIStyle(EditorStyles.label);
                _style.fontStyle = FontStyle.Normal;

                return _style;
            }

        }

        public static GUIStyle LabelBold
        {
            get
            {
                var _style = new GUIStyle(EditorStyles.label);
                _style.fontStyle = FontStyle.Bold;

                return _style;
            }

        }
    }
}