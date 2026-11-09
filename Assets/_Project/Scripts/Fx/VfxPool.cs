using System.Collections.Generic;
using UnityEngine;

public class VfxPool : MonoBehaviour
{
    [SerializeField] private Sprite[] slashFrames;

    private readonly Stack<SpriteVfx> pool = new Stack<SpriteVfx>();

    public void Construct(Sprite[] frames)
    {
        slashFrames = frames;
    }

    public SpriteVfx Play(Vector3 position)
    {
        SpriteVfx vfx = pool.Count > 0 ? pool.Pop() : CreateInstance();
        vfx.Play(position);
        return vfx;
    }

    private SpriteVfx CreateInstance()
    {
        var go = new GameObject("VfxSlash");
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        sr.sortingOrder = 150;

        var vfx = go.AddComponent<SpriteVfx>();
        vfx.Construct(slashFrames, OnVfxFinished);
        return vfx;
    }

    private void OnVfxFinished(SpriteVfx vfx)
    {
        pool.Push(vfx);
    }
}
