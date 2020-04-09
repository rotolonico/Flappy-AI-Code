using UnityEngine;

namespace Game
{
    public class LeftForce : MonoBehaviour
    {
        private void Update()
        {
            var thisTransform = transform;
            var position = thisTransform.position;
            position = new Vector3(position.x - Time.deltaTime * Settings.Instance.gameSpeed, position.y, position.z);
            thisTransform.position = position;
        }
    }
}
