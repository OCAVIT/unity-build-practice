using UnityEngine;

namespace BuildPractice
{
    // Вращает куб: по движению сразу видно, что билд запущен и работает.
    public class Spin : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new(25f, 60f, 0f);

        void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
