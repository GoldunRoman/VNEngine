#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;

namespace VNEngine.DS.Utilities
{
    public static class DSStyleUtility 
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (var className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetPaths)
        {
            foreach(string path in styleSheetPaths)
            {
                StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load(path);

                element.styleSheets.Add(styleSheet);
            }

            return element;
        }
    }
}
#endif