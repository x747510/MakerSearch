using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using CharaCustom;
using XUnity.AutoTranslator.Plugin.Core;

namespace HS2_MakerSearch
{
    public static class Tools
    {
        private static readonly InputField[] fields = new InputField[2];
        
        private static readonly string[] targets =
        {
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting01",     // Clothes
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinHair/H_Hair/Setting/Setting01"                       // Hair
        };
        
        public static void CreateUI()
        {
            var orig = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_ShapeWhole/Scroll View/Viewport/Content/SliderSet/SldInputField");

            var i = 0;
            foreach (var targetStr in targets)
            {
                var target = GameObject.Find(targetStr);
                
                var cp = UnityEngine.Object.Instantiate(orig, target.transform);
                cp.name = "Search";

                var placeholderObj = cp.transform.Find("Placeholder");
                var placeholder = placeholderObj.GetComponent<Text>();
                placeholder.text = "Search";

                UnityEngine.Object.Destroy(cp.transform.Find("SldInputField Input Caret"));

                var rect = cp.GetComponent<RectTransform>();

                if (i == 0)
                {
                    rect.offsetMin = new Vector2(-250, 3);
                    rect.offsetMax = new Vector2(0, -383);
                } 
                else if (i == 1)
                {
                    rect.offsetMin = new Vector2(-420, 3);
                    rect.offsetMax = new Vector2(0, -383);

                    var box = target.transform.Find("SelectBox");
                    box.GetComponent<RectTransform>().offsetMin = new Vector2(0, -372);

                    var scrollview = box.Find("Scroll View");
                    scrollview.GetComponent<RectTransform>().offsetMin = new Vector2(0, -372);
                }
                
                var input = cp.GetComponent<InputField>();
                input.contentType = InputField.ContentType.Standard;
                input.characterLimit = 64;

                input.onValueChanged.RemoveAllListeners();
                input.onEndEdit.RemoveAllListeners();

                input.onValueChanged.AddListener(delegate(string text) { placeholder.enabled = text == ""; });
                input.onEndEdit.AddListener(delegate(string text)
                {
                    HS2_MakerSearch.searchString = text;
                    HS2_MakerSearch.Search();
                });

                fields[i] = input;

                i++;
            }
        }
        
        private static string GetSearchInData(CustomSelectInfo data)
        {
            var searchIn = "";
            string name;

            switch (HS2_MakerSearch.searchBy.Value)
            {
                case SearchBy.Name:
                    searchIn = data.name;
                    
                    if (HS2_MakerSearch.useTranslatedCache.Value)
                        AutoTranslator.Default.TranslateAsync(data.name, result => { searchIn = result.Succeeded ? result.TranslatedText : data.name; });

                    break;
                case SearchBy.AssetName:
                    searchIn = data.assetName;
                    break;
                case SearchBy.Id:
                    searchIn = data.id.ToString();
                    break;
                case SearchBy.AllButId:
                    name = data.name;
                    
                    if (HS2_MakerSearch.useTranslatedCache.Value)
                        AutoTranslator.Default.TranslateAsync(data.name, result => { name = result.Succeeded ? result.TranslatedText : data.name; });

                    searchIn = name + "\n" + data.assetName + "\n";
                    break;
                case SearchBy.All:
                    name = data.name;
                    
                    if (HS2_MakerSearch.useTranslatedCache.Value)
                        AutoTranslator.Default.TranslateAsync(data.name, result => { name = result.Succeeded ? result.TranslatedText : data.name; });

                    searchIn = name + "\n" + data.assetName + "\n" + data.id + "\n";
                    break;
            }

            return searchIn;
        }
        
        public static bool ItemMatchesSearch(CustomSelectInfo data, string searchStr)
        {
            var searchIn = GetSearchInData(data);

            var rule = StringComparison.Ordinal;
            if (!HS2_MakerSearch.caseSensitive.Value)
            {
                searchStr = searchStr.ToLowerInvariant();
                rule = StringComparison.OrdinalIgnoreCase;
            }
            
            var splitSearchStr = searchStr.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return splitSearchStr.All(s => searchIn.IndexOf(s, rule) >= 0);
        }
        
        public static bool UpdateUI(SearchCategory category)
        {
            switch (category)
            {
                case SearchCategory.Face:
                    return false;
                case SearchCategory.Body:
                    return false;
                case SearchCategory.Hair:
                    HS2_MakerSearch.cvsHair.UpdateHairList();
                    HS2_MakerSearch.cvsHair.UpdateCustomUI();
                    break;
                case SearchCategory.Clothes:
                    HS2_MakerSearch.cvsClothes.UpdateClothesList();
                    HS2_MakerSearch.cvsClothes.UpdateCustomUI();
                    break;
                case SearchCategory.Accessories:
                    return false;
                case SearchCategory.Extra:
                    return false;
                case SearchCategory.None:
                    return false;
                default:
                    return false;
            }

            return true;
        }

        public static void ResetSearch()
        {
            if (HS2_MakerSearch.searchString == "") 
                return;
            
            UpdateUI(HS2_MakerSearch.category);
            HS2_MakerSearch.searchString = "";

            foreach (var field in fields.Where(field => field != null))
                field.text = "";
        }
        
        public enum SearchBy
        {
            Name,
            AssetName,
            Id,
            AllButId,
            All
        }

        public enum SearchCategory
        {
            Face,
            Body,
            Hair,
            Clothes,
            Accessories,
            Extra,
            None
        }
    }
}