#if UNITY_2021_2_OR_NEWER
using System.IO;
using SoyWar.SimplePlantGrowth.Editor.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace SoyWar.SimplePlantGrowth.Editor
{
    internal class HomePage : EditorWindow
    {
        private static readonly Color Dark;
        private static readonly Color Grey;

        static HomePage()
        {
            Dark = new Color32(33, 37, 41, 255);
            Grey = new Color32(76, 85, 93, 255);
        }

        private void OnEnable()
        {
            PackageInfo packageInfo = ProjectSettings.GetPackageInfo();
            
            string fontPath = Path.Join(packageInfo.assetPath, "InternalResources/Fonts/Signika SDF.asset");
            string backgroundPath = Path.Join(packageInfo.assetPath, "InternalResources/Background/HomePage.png");

            FontAsset font = AssetDatabase.LoadAssetAtPath<FontAsset>(fontPath);
            Texture2D backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(backgroundPath);

            Image background = new Image()
            {
                image = backgroundTexture,
                style =
                {
                    position = Position.Absolute,
                    width = 720,
                    height = 480,
                    backgroundColor = Color.white
                },
                scaleMode = ScaleMode.ScaleAndCrop
            }; 

            Label version = new Label(packageInfo.version)
            {
                style =
                {
                    color = Color.white,
                    position = Position.Absolute,
                    right = 16,
                    top = 8,
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            VisualElement documentation = CreateLinkButton("Documentation", "Get started and learn how to grow plants easily.", "https://simpleplantgrowth.soywar.com/");

            VisualElement webSite = CreateLinkButton("Web Site", "Visit our website to discover other Unity assets. Browse our selection of powerful tools and resources designed to streamline your workflow and take your projects to the next level.", "https://www.soywar.com/");

            VisualElement bugReport = CreateLinkButton("Bug Report", "If you encounter any issues, please submit a bug report on our GitLab page. Our development team will review your report and work to resolve any issues as quickly as possible.", "https://gitlab.com/soywar/assets/simple-plant-growth/");

            VisualElement rating = CreateLinkButton("Rating", "Share your feedback and help us improve our product. Visit our Unity Asset Store page to rate and review the asset, and let us know what you think. Your input is valuable to us and helps us deliver better products to our customers.", "https://assetstore.unity.com/packages/slug/250373#reviews");
            
            rootVisualElement.style.unityFontDefinition = new StyleFontDefinition(font);
            rootVisualElement.style.paddingBottom = 32;
            rootVisualElement.style.justifyContent = Justify.FlexEnd;
            rootVisualElement.style.alignItems = Align.Center;
            
            rootVisualElement.Add(background);
            rootVisualElement.Add(version);
            rootVisualElement.Add(documentation);
            rootVisualElement.Add(webSite);
            rootVisualElement.Add(bugReport);
            rootVisualElement.Add(rating);
            
            minSize = maxSize = new Vector2(720, 480);
        }

        private static VisualElement CreateLinkButton(string title, string description, string url)
        {
            Button button = new Button(() =>
            {
                Application.OpenURL(url);
            })
            {
                style =
                {
                    color = Dark,
                    width = 640,
                    height = 64,
                    marginBottom = 8,
                    marginTop = 8,
                    backgroundColor = Color.white,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderRightWidth = 0,
                    borderLeftWidth = 0
                }
            };
            
            Label titleElement = new Label(title)
            {
                style =
                {
                    width = Length.Percent(100),
                    unityTextAlign = TextAnchor.UpperCenter,
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    borderBottomColor = Dark,
                    borderBottomWidth = 1,
                }
            };
            
            Label descriptionElement = new Label(description)
            {
                style =
                {
                    width = Length.Percent(100),
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.UpperLeft,
                    marginTop = 4
                }
            };
            
            button.Add(titleElement);
            button.Add(descriptionElement);
            
            button.RegisterCallback<PointerEnterEvent>(_ =>
            {
                button.style.backgroundColor = Grey;
                button.style.color = Color.white;
                
                titleElement.style.borderBottomColor = Color.white;
            });
            
            button.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                button.style.backgroundColor = Color.white;
                button.style.color = Dark;
                
                titleElement.style.borderBottomColor = Dark;
            });
            
            return button;
        }

        [InitializeOnLoadMethod]
        private static void PreInitialize()
        {
            if (ProjectSettings.instance.Updated())
            {
                EditorApplication.delayCall += Initialize;
            }
        }

        private static void Initialize()
        {
            ShowWindow();
            EditorApplication.delayCall -= Initialize;
        }

        [MenuItem("Window/SoyWar/Simple Plant Growth")]
        private static void ShowWindow()
        {
            GetWindow<HomePage>(true, "Simple Plant Growth");
        }
    }
}
#endif