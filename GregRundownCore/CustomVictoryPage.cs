using CellMenu;
using GTFO.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GregRundownCore
{
    class CustomVictoryPage
    {
        public static void Setup(CM_PageExpeditionSuccess page)
        {
            var environment = GameObject.Instantiate(AssetAPI.GetLoadedAsset("Assets/Bundle/GregRundown/Content/VictoryScreenArea.prefab").TryCast<GameObject>());
            environment.name = "environment";
            environment.transform.parent = page.transform.FindChild("Backgrounds");
            environment.transform.localPosition = new(0, -180, 0);
            environment.transform.localScale = Vector3.one * 100f;

            page.transform.FindChild("Backgrounds/Bottom").gameObject.active = false;
            page.transform.FindChild("Backgrounds/Middle").gameObject.active = false;
        }
    }
}
