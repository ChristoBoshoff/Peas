using UnityEngine;

namespace Riftwake
{
    public static class PrimitiveFactory
    {
        public static Material MakeColorMat(Color color, bool transparent = false)
        {
            var shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Diffuse");
            var mat = new Material(shader);
            mat.color = color;
            if (transparent)
            {
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            return mat;
        }

        public static GameObject CapsuleActor(string name, Color color, Vector3 position, float heightScale = 1.8f)
        {
            var root = new GameObject(name);
            root.transform.position = position;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, heightScale * 0.5f, 0f);
            body.transform.localScale = new Vector3(0.9f, heightScale * 0.5f, 0.9f);
            body.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMat(color);
            Object.Destroy(body.GetComponent<Collider>());

            // Facing marker
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Facing";
            nose.transform.SetParent(root.transform, false);
            nose.transform.localPosition = new Vector3(0f, heightScale * 0.7f, 0.55f);
            nose.transform.localScale = new Vector3(0.25f, 0.2f, 0.4f);
            nose.GetComponent<MeshRenderer>().sharedMaterial = MakeColorMat(Color.Lerp(color, Color.white, 0.35f));
            Object.Destroy(nose.GetComponent<Collider>());

            return root;
        }

        public static void AddCharacterController(GameObject root, float height = 1.8f, float radius = 0.4f)
        {
            var cc = root.AddComponent<CharacterController>();
            cc.height = height;
            cc.radius = radius;
            cc.center = new Vector3(0f, height * 0.5f, 0f);
            cc.skinWidth = 0.08f;
        }
    }
}
