using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BossFight
{
    public static class GameplayHelpers
    {
        static IObjectPool<GameObject> m_GameObjectPool;

        public static IObjectPool<GameObject> GameObjectPool
        {
            get
            {
                if (m_GameObjectPool == null)
                {
                    m_GameObjectPool = new ObjectPool<GameObject>(
                        () => new GameObject("PooledObject"),
                        go => go.SetActive(true),
                        go => go.SetActive(false));
                }

                return m_GameObjectPool;
            }
        }

        /// <summary>
        /// Checks up the GameObject hierarchy to see if there is any objects in the tree holding a
        /// PlayerCharacterState component. If there is none - we assume this object doesn't belong to a player
        /// </summary>
        public static bool BelongsToPlayer(this GameObject self, out PlayerCharacterStateManager playerCharacterStateManager)
        {
            var parent = self.transform.parent;

            return self.TryGetComponent(out playerCharacterStateManager)
                || parent != null && BelongsToPlayer(parent.gameObject, out playerCharacterStateManager);
        }

        public static bool BelongsToPlayer(this GameObject self) => BelongsToPlayer(self, out _);

        public static bool BelongsToPlayer(this Component self) => self.gameObject.BelongsToPlayer(out _);

        public static bool BelongsToPlayer(this Component self, out PlayerCharacterStateManager playerCharacterStateManager) =>
            BelongsToPlayer(self.gameObject, out playerCharacterStateManager);

        static void AddChildTransforms(Transform currentChild, ICollection<Transform> allChildren)
        {
            allChildren.Add(currentChild);

            if (currentChild.childCount == 0)
            {
                return;
            }

            for (var i = 0; i < currentChild.childCount; ++i)
            {
                var baby = currentChild.GetChild(i);
                AddChildTransforms(baby, allChildren);
            }
        }

        public static void GetRigTargetFromChildren(this GameObject self, out Transform target, out List<Transform> children)
        {
            target = null;
            children = new List<Transform>();
            var tf = self.transform;
            for (var i = tf.childCount - 1; i >= 0; i--)
            {
                var child = tf.GetChild(i);
                if (child.CompareTag(Constants.Tags.RigTarget))
                {
                    target = child;
                }
                AddChildTransforms(child, children);
            }
        }


    }
}
