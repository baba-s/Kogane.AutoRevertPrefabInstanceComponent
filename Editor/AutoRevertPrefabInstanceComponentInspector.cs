using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kogane.Internal
{
    [InitializeOnLoad]
    [CustomEditor( typeof( AutoRevertPrefabInstanceComponent ) )]
    internal sealed class AutoRevertPrefabInstanceComponentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
        }

        static AutoRevertPrefabInstanceComponentInspector()
        {
            EditorSceneManager.sceneOpened         -= OnSceneOpened;
            EditorSceneManager.sceneOpened         += OnSceneOpened;
            EditorSceneManager.sceneSaving         -= OnSceneSaving;
            EditorSceneManager.sceneSaving         += OnSceneSaving;
            ObjectFactory.componentWasAdded        -= OnComponentWasAdded;
            ObjectFactory.componentWasAdded        += OnComponentWasAdded;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnSceneOpened( Scene scene, OpenSceneMode mode )
        {
            if ( EditorApplication.isPlaying ) return;
            Revert();
        }

        private static void OnSceneSaving( Scene scene, string path )
        {
            if ( EditorApplication.isPlaying ) return;
            Revert();
        }

        private static void OnComponentWasAdded( Component component )
        {
            if ( EditorApplication.isPlaying ) return;
            Revert();
        }

        private static void OnPlayModeStateChanged( PlayModeStateChange change )
        {
            if ( change != PlayModeStateChange.ExitingEditMode ) return;
            Revert();
        }

        private static void Revert()
        {
            if ( EditorApplication.isPlaying ) return;

            var gameObjects = SceneManager
                    .GetActiveScene()
                    .GetRootGameObjects()
                    .SelectMany( x => x.GetComponentsInChildren<AutoRevertPrefabInstanceComponent>( true ) )
                    .Where( x => x != null )
                    .Select( x => x.gameObject )
                    .ToArray()
                ;

            if ( gameObjects.Length <= 0 ) return;

            foreach ( var gameObject in gameObjects )
            {
                if ( 0 < PrefabUtility.GetObjectOverrides( gameObject ).Count )
                {
                    Undo.RegisterFullObjectHierarchyUndo( gameObject, "Revert" );
                    PrefabUtility.RevertPrefabInstance( gameObject, InteractionMode.AutomatedAction );
                }

                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource( gameObject );
                // var gameObjectTransform   = gameObject.transform;
                // var prefabTransform       = prefab.transform;
                var isOverrideName = gameObject.name != prefab.name;
                // var isOverridePosition    = gameObjectTransform.localPosition != prefabTransform.localPosition;
                // var isOverrideEulerAngles = gameObjectTransform.localEulerAngles != prefabTransform.localEulerAngles;

                // if ( isOverrideName || isOverridePosition || isOverrideEulerAngles )
                if ( isOverrideName )
                {
                    Undo.RecordObject( gameObject, "Revert" );
                }

                if ( isOverrideName )
                {
                    gameObject.name = prefab.name;
                }

                // if ( isOverridePosition )
                // {
                //     gameObjectTransform.localPosition = prefabTransform.localPosition;
                // }
                //
                // if ( isOverrideEulerAngles )
                // {
                //     gameObjectTransform.localEulerAngles = prefabTransform.localEulerAngles;
                // }
            }
        }
    }
}