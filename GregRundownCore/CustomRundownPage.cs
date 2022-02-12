using CellMenu;
using GTFO.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GregRundownCore
{
    class CustomRundownPage
    {
        public static void Setup(CM_PageRundown_New page)
        {
            Rundown = page.transform.FindChild("MovingContent/Rundown");
            Rundown.localPosition = new(5000, 460, 0);

            foreach (var expeditionButton in Rundown.GetComponentsInChildren<CM_ExpeditionIcon_New>())
            {
                expeditionButton.transform.SetParent(Rundown);
            }

            var tier1 = Rundown.FindChild("GUIX_layer_Tier_1");
            tier1.localPosition = new(0, -460, 800);
            tier1.localScale = Vector3.one * 5;
            tier1.localEulerAngles = Vector3.zero;
            tier1.gameObject.active = true;

            var tier2 = Rundown.FindChild("GUIX_layer_Tier_2");
            tier2.localPosition = new(0, -460, 400);
            tier2.localScale = Vector3.one * 2;
            tier2.localEulerAngles = Vector3.zero;
            tier2.gameObject.active = true;

            var tier3 = Rundown.FindChild("GUIX_layer_Tier_3");
            tier3.localPosition = new(0, -460, 500);
            tier3.localScale = Vector3.one;
            tier3.localEulerAngles = Vector3.zero;
            tier3.gameObject.active = true;

            var background = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            UnityEngine.Object.Destroy(background.GetComponent<Collider>());
            background.transform.parent = Rundown;
            background.transform.localPosition = Vector3.zero;
            background.transform.localEulerAngles = new(5, 0, -5);
            background.transform.localScale = new(3000, 3000, 3000);
            background.layer = LayerMask.NameToLayer("UI");
            BGRenderer = background.GetComponent<Renderer>();
            BGRenderer.sharedMaterial.shader = Shader.Find("Hidden/Internal-GUITexture");
            BGRenderer.sharedMaterial.mainTextureScale = new(3, 3);
            BGRenderer.sharedMaterial.mainTexture = AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/BGGrid.png").TryCast<Texture2D>();
            background.AddComponent<RundownBGRotation>();
            CoroutineHandler.Add(HideBlueShit());

            page.m_rundownIntroIsDone = true;

            SpriteRenderer spriteRenderer;
            var levels = Rundown.GetComponentsInChildren<CM_ExpeditionIcon_New>();
            foreach (var level in levels)
            {
                level.transform.FindChild("Root/Icon Text").gameObject.active = false;
                level.transform.FindChild("Root/Box").gameObject.active = false;
                level.transform.FindChild("Root/ArtifactHeat").gameObject.active = false;
                level.transform.FindChild("Root/Status").gameObject.active = false;
                var name = level.transform.FindChild("Root/PublicName");
                name.localPosition = new(-192, 27.6f, -1.3656f);
                name.localScale = new(1.25f, 1.25f, 1.25f);

                var sprite = new GameObject();
                sprite.name = "thumbnail";
                sprite.layer = LayerMask.NameToLayer("UI");
                sprite.transform.parent = level.transform;
                sprite.transform.localPosition = new(-70, 0, 0);
                sprite.transform.localScale = new(0.3f, 0.3f, 0.3f);

                spriteRenderer = sprite.AddComponent<SpriteRenderer>();
            }

            levels[0].transform.FindChild("thumbnail").GetComponent<SpriteRenderer>().sprite = Sprite.Create
            (
                AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/LevelIcon_DigSite.png").TryCast<Texture2D>(),
                new Rect(new(0,0), new(512,512)),
                new Vector2(0,0),
                1
            );
            levels[1].transform.FindChild("thumbnail").GetComponent<SpriteRenderer>().sprite = Sprite.Create
            (
                AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/LevelIcon_Refinery.png").TryCast<Texture2D>(),
                new Rect(new(0, 0), new(512, 512)),
                new Vector2(0, 0),
                1
            );
            levels[2].transform.FindChild("thumbnail").GetComponent<SpriteRenderer>().sprite = Sprite.Create
            (
                AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/LevelIcon_Storage.png").TryCast<Texture2D>(),
                new Rect(new(0, 0), new(512, 512)),
                new Vector2(0, 0),
                1
            );
            levels[3].transform.FindChild("thumbnail").GetComponent<SpriteRenderer>().sprite = Sprite.Create
            (
                AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/LevelIcon_Lab.png").TryCast<Texture2D>(),
                new Rect(new(0, 0), new(512, 512)),
                new Vector2(0, 0),
                1
            );
            levels[4].transform.FindChild("thumbnail").GetComponent<SpriteRenderer>().sprite = Sprite.Create
            (
                AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/LevelIcon_Data.png").TryCast<Texture2D>(),
                new Rect(new(0, 0), new(512, 512)),
                new Vector2(0, 0),
                1
            );
            levels[5].transform.FindChild("thumbnail").GetComponent<SpriteRenderer>().sprite = Sprite.Create
            (
                AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/LevelIcon_Floodways.png").TryCast<Texture2D>(),
                new Rect(new(0, 0), new(512, 512)),
                new Vector2(0, 0),
                1
            );
        }

        public static IEnumerator HideBlueShit()
        {
            yield return new WaitForSeconds(1);

            Rundown.FindChild("GUIX_layer_Tier_1/Rundown_Tier_1").gameObject.active = false;
            Rundown.FindChild("GUIX_layer_Tier_2/Rundown_Tier_2").gameObject.active = false;
            Rundown.FindChild("GUIX_layer_Tier_3/Rundown_Tier_3").gameObject.active = false;
            Rundown.FindChild("GUIX_layer_Tier_4/Rundown_Tier_4").gameObject.active = false;
            Rundown.FindChild("VerticalArrow").gameObject.active = false;

            Rundown.localPosition = new(0, 460, 0);
        }

        public static Renderer BGRenderer;
        public static Transform Rundown;
    }
}
