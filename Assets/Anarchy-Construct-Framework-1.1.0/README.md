# Anarchy Construct Framework

## Constructs: What Are They?

Constructs are best thought of as a noun inside of a world. I mean like a person, place, thing, or idea. In our use, constructs are used as a method of logically separating and organizing assets into understandable structures. For example, a drawbridge would be in the Drawbridge construct. It would hold everything about the drawbridge including (but not limited to) models, animations, scripts, shaders, materials, textures, audio, and so on.

## How to Work on Constructs

Constructs are made via prefabs that are worked on within scenes. Each construct is worked on in its own scene and loaded dynamically at runtime with addressables. This also works when it comes to multiple people working on a single construct. Every person works on their prefab within the construct, making it easily manageable and extendable.

## Using AnarchyBehaviour vs MonoBehaviour:

AnarchyBehaviour is a base class in the Anarchy.Core.Common namespace designed to enhance Unity behaviors by providing a clean and modular event handling mechanism. It extends the MonoBehaviour class and introduces methods for setting up event links using the EventLink struct. The class maintains a list of event listeners for proper cleanup during enabling and disabling of the object. To use AnarchyBehaviour, inherit from this class and leverage its event link methods (SetLink, SetLinks). This ensures a structured approach to associating Unity events with corresponding actions, promoting code readability and maintainability. Example usage is demonstrated as follows:

```cs
public class ExampleUsage : AnarchyBehaviour
{
    private void Start()
    {
        // Set up an EventLink for a non-generic event
        EventLink eventLink = new EventLink();
        SetLink(eventLink);

        // Set up multiple EventLinks for a generic event
        EventLink<int> eventLinkInt = new EventLink<int>();
        EventLink<string> eventLinkString = new EventLink<string>();
        SetLinks(eventLinkInt, eventLinkString);
    }
}
```

## Addressable Loading with ConstructLoader

The ConstructLoader class offers a set of static methods for the asynchronous loading and unloading of prefabs and scenes using Unity's Addressable Assets system. This utility enhances resource management by providing a convenient and efficient way to handle assets in Unity projects.

```csharp
public class ExampleLoaderUsage : MonoBehaviour
{
    private void Start()
    {
        // Example usage of loading a prefab
        ConstructLoader.LoadPrefabConstructAsync("Prefabs/ExamplePrefab", prefab =>
        {
            // Perform actions with the instantiated prefab
            Destroy(prefab, 5f);
        });

        // Example usage of loading a scene
        ConstructLoader.LoadSceneConstructAsync("Scenes/ExampleScene", loadedScene =>
        {
            // Scene is loaded, perform additional actions
            Debug.Log($"Loaded Scene: {loadedScene}");

            // Unload the scene after 10 seconds
            StartCoroutine(UnloadSceneAfterDelay(loadedScene, 10f));
        });
    }

    private IEnumerator UnloadSceneAfterDelay(string sceneAddress, float delay)
    {
        yield return new WaitForSeconds(delay);
        ConstructLoader.UnloadSceneConstruct(sceneAddress);
    }
}
