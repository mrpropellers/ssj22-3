using UnityEngine;

namespace BossFight
{
    /// <summary>
    /// Empty class for us to put all our required components to enable the full gameplay loop -- components should
    /// be tested in isolation but this can be added to a scene to instantiate the whole suite of "singletons"
    /// </summary>
    [RequireComponent(typeof(PlayerDeathHandler))]
    [DisallowMultipleComponent]
    public class GamePlayEventHandling : MonoBehaviour
    {
        // TODO: We could validate that all required components exist only as Components on this class
    }
}
