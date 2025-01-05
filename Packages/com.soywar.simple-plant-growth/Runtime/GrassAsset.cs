using System;
using SoyWar.SimplePlantGrowth.Utils;
using UnityEngine;

namespace SoyWar.SimplePlantGrowth
{
    
    [CreateAssetMenu(fileName = "Grass Growth Phase", menuName = "Simple Plant Growth/Grass", order = 1001)]
    public sealed class GrassAsset : PlantAsset<GrassAsset>
    {
        internal static readonly Color32 DefaultHealthyColor = new Color(0.2627451f, 0.9764706f, 0.16470589f, 1f);
        internal static readonly Color32 DefaultDryColor = new Color(0.8039216f, 0.7372549f, 0.101960786f, 1f);
        [SerializeField] private Texture2D _texture;
        [SerializeField] private float _noiseSpread = 0.1f;
        [SerializeField] private DetailRenderMode _renderMode = DetailRenderMode.Grass;
        [SerializeField] private bool _usePrototypeMesh;
        [SerializeField] private Color32 _healthyColor = DefaultHealthyColor;
        [SerializeField] private Color32 _dryColor = DefaultDryColor;
        [SerializeField] private bool _randomColor = true;
        
#if UNITY_2021_2_OR_NEWER
        private static readonly System.Random Random = new System.Random();
        
        [SerializeField] private int _noiseSeed = Random.Next();
        [SerializeField] private bool _useInstancing;
#endif
        
#if UNITY_2020_2_OR_NEWER
        [SerializeField] private float _holeEdgePadding;
#endif
        
#if UNITY_2022_2_OR_NEWER
        [SerializeField] private float _targetCoverage = 1;
        [SerializeField] private float _alignToGround;
        [SerializeField] private float _positionJitter;
        [SerializeField] private float _density = 1;
        [SerializeField] private bool _useDensityScaling;
#endif

        [DocFXIgnore]
        public override GameObject Prototype { 
            get => base.Prototype;
            set
            {
                if (UsePrototypeMesh)
                {
                    base.Prototype = value;
                }
            } 
        }

        /// <inheritdoc cref="DetailPrototype.prototypeTexture"/>
        public Texture2D Texture
        {
            get => _texture;
            set
            {
                if (!UsePrototypeMesh)
                {
                    _texture = value;
                }
            }
        }
        
        /// <inheritdoc cref="DetailPrototype.noiseSpread"/>
        public float NoiseSpread
        {
            get => _noiseSpread;
            set => _noiseSpread = Mathf.Max(value, 0);
        }
        
        /// <inheritdoc cref="DetailPrototype.renderMode"/>
        public DetailRenderMode RenderMode
        {
            get => _renderMode;
            set
            {
#if UNITY_2021_2_OR_NEWER
                bool useInstancing = UseInstancing;
#else
                bool useInstancing = false;
#endif
                
                if (value == DetailRenderMode.Grass || 
                    value == DetailRenderMode.VertexLit && UsePrototypeMesh && !useInstancing || 
                    value == DetailRenderMode.GrassBillboard && !UsePrototypeMesh)
                {
                    _renderMode = value;
                }
            }
        }
        
        /// <summary>
        /// Checks whether prototype must be used instead of texture.
        /// </summary>
        public bool UsePrototypeMesh
        {
            get => _usePrototypeMesh;
            set
            {
                if (value)
                {
                    base.Prototype = null;
                    
                    if (_renderMode == DetailRenderMode.GrassBillboard)
                    {
                        _renderMode = DetailRenderMode.Grass;
                    }
                }
                else
                {
                    _texture = null;
#if UNITY_2021_2_OR_NEWER
                    _useInstancing = false;
#endif
                    
                    if (_renderMode == DetailRenderMode.VertexLit)
                    {
                        _renderMode = DetailRenderMode.Grass;
                    }
                }
                
                _usePrototypeMesh = value;
            }
        }
        
        /// <summary>
        /// Activates random tint.
        /// </summary>
        public bool RandomTint
        {
            get => _randomColor;
            set => _randomColor = value;
        }
        
        /// <summary>
        /// Grass tint
        /// </summary>
        /// <remarks>
        /// When a value is set, disables random tint.
        /// </remarks>
        public Color Tint
        {
            get => _healthyColor;
            set
            {
                _randomColor = false;
                _healthyColor = _dryColor = value;
            }
        }
        
        /// <summary>
        /// Tint when the grass is "healthy".
        /// </summary>
        /// <remarks>
        /// When a value is set, enables random tint.
        /// </remarks>
        public Color HealthyTint
        {
            get => _healthyColor;
            set 
            {
                _randomColor = true;
                _healthyColor = value;
            }
        }
        
        /// <summary>
        /// Tint when the grass is "dry".
        /// </summary>
        /// <remarks>
        /// When a value is set, enables random tint.
        /// </remarks>
        public Color DryTint
        {
            get => _dryColor;
            set
            {
                _randomColor = true;
                _dryColor = value;
            }
        }

#if UNITY_2021_2_OR_NEWER
        /// <inheritdoc cref="DetailPrototype.noiseSeed"/>
        public int NoiseSeed
        {
            get => _noiseSeed;
            set => _noiseSeed = value;
        }
        
        /// <inheritdoc cref="DetailPrototype.useInstancing"/>
        public bool UseInstancing
        {
            get => _useInstancing;
            set
            {
                if (UsePrototypeMesh)
                {
                    _useInstancing = value;

                    if (value && _renderMode == DetailRenderMode.Grass)
                    {
                        _renderMode = DetailRenderMode.VertexLit;
                    }
                }
            }
        }
#endif
        
#if UNITY_2020_2_OR_NEWER
        /// <inheritdoc cref="DetailPrototype.holeEdgePadding"/>
        public float HoleEdgePadding
        {
            get => _holeEdgePadding;
            set => _holeEdgePadding = Mathf.Clamp(value, 0, 1);
        }
#endif
        
#if UNITY_2022_2_OR_NEWER
        /// <inheritdoc cref="DetailPrototype.targetCoverage"/>
        public float TargetCoverage
        {
            get => _targetCoverage;
            set => _targetCoverage = Math.Max(value, 0);
        }
        
        /// <inheritdoc cref="DetailPrototype.alignToGround"/>
        public float AlignToGround
        {
            get => _alignToGround;
            set => _alignToGround = Mathf.Clamp(value, 0, 1);
        }
        
        /// <inheritdoc cref="DetailPrototype.positionJitter"/>
        public float PositionJitter
        {
            get => _positionJitter;
            set => _positionJitter = Mathf.Clamp(value, 0, 1);
        }
        
        /// <inheritdoc cref="DetailPrototype.density"/>
        public float DetailDensity
        {
            get => _density;
            set => _density = Mathf.Max(value, 0);
        }

        /// <inheritdoc cref="DetailPrototype.useDensityScaling"/>
        public bool UseDensityScaling
        {
            get => _useDensityScaling;
            set => _useDensityScaling = value;
        }
#endif

        internal protected override void CopyFrom(GrassAsset asset)
        {
            base.CopyFrom(asset);
            
            _texture = asset._texture;
            _noiseSpread = asset._noiseSpread;
            _renderMode = asset._renderMode;
            _usePrototypeMesh = asset._usePrototypeMesh;
            _healthyColor = asset._healthyColor;
            _dryColor = asset._dryColor;
            _randomColor = asset._randomColor;
        
#if UNITY_2021_2_OR_NEWER
            _noiseSeed = asset._noiseSeed;
            _useInstancing = asset._useInstancing;
#endif
        
#if UNITY_2020_2_OR_NEWER
            _holeEdgePadding = asset._holeEdgePadding;
#endif
            
#if UNITY_2022_2_OR_NEWER
            _targetCoverage = asset._targetCoverage;
            _alignToGround = asset._alignToGround;
            _positionJitter = asset._positionJitter;
            _density = asset._density;
            _useDensityScaling = asset._useDensityScaling;
#endif
        }

        [DocFXIgnore]
        public override bool Validate(out string errorMessage)
        {
#if UNITY_2020_2_OR_NEWER
            DetailPrototype detailPrototype = this;

            return detailPrototype.Validate(out errorMessage);
#else
            
            if (UsePrototypeMesh)
            {
                if (!Prototype)
                {
                    errorMessage = "Prototype is missing Game Object.";
                    return false;
                }
            }
            else
            {
                if (!Texture)
                {
                    errorMessage = "Prototype is missing texture.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
#endif
        }

        /// <summary>
        /// Convert DetailPrototype to GrassAsset.
        /// </summary>
        public static implicit operator GrassAsset(DetailPrototype detailPrototype)
        {
            GrassAsset grassAsset;

            if (detailPrototype == null)
            {
                return null;
            }
            
            grassAsset = CreateInstance<GrassAsset>();
            
#if UNITY_2020_2_OR_NEWER || UNITY_2019_4
            grassAsset.UsePrototypeMesh = detailPrototype.usePrototypeMesh;
#else
            grassAsset.UsePrototypeMesh = detailPrototype.prototype;
#endif
            
#if UNITY_2020_2_OR_NEWER
            grassAsset.HoleEdgePadding = detailPrototype.holeEdgePadding;
#endif
            
#if UNITY_2021_2_OR_NEWER
            grassAsset.NoiseSeed = detailPrototype.noiseSeed;
            grassAsset.UseInstancing = detailPrototype.useInstancing;
#endif
            
#if UNITY_2022_2_OR_NEWER
            grassAsset.TargetCoverage = detailPrototype.targetCoverage;
            grassAsset.AlignToGround = detailPrototype.alignToGround;
            grassAsset.PositionJitter = detailPrototype.positionJitter;
            grassAsset.DetailDensity = detailPrototype.density;
            grassAsset.UseDensityScaling = detailPrototype.useDensityScaling;
#endif
            
            grassAsset.Prototype = detailPrototype.prototype;
            grassAsset.Texture = detailPrototype.prototypeTexture;
            
            grassAsset.SetMinMaxHeight(detailPrototype.minHeight, detailPrototype.maxHeight);
            grassAsset.SetMinMaxWidth(detailPrototype.minWidth, detailPrototype.maxWidth);
            
            grassAsset.NoiseSpread = detailPrototype.noiseSpread;
            grassAsset.HealthyTint = detailPrototype.healthyColor;
            grassAsset.DryTint = detailPrototype.dryColor;
            grassAsset.RenderMode = detailPrototype.renderMode;
            
            return grassAsset;
        }

        /// <summary>
        /// Convert GrassAsset to DetailPrototype.
        /// </summary>
        public static implicit operator DetailPrototype(GrassAsset grassAsset)
        {
            if (!grassAsset)
            {
                return null;
            }
            
            return new DetailPrototype
            {
                prototype = grassAsset.Prototype,
                prototypeTexture = grassAsset.Texture,
                minWidth = grassAsset.MinWidth,
                maxWidth = grassAsset.MaxWidth,
                minHeight = grassAsset.MinHeight,
                maxHeight = grassAsset.MaxHeight,
                noiseSpread = grassAsset.NoiseSpread,
                healthyColor = grassAsset.HealthyTint,
                dryColor = grassAsset.DryTint,
                renderMode = grassAsset.RenderMode,
                
#if UNITY_2021_2_OR_NEWER
                noiseSeed = grassAsset.NoiseSeed,
                useInstancing = grassAsset.UseInstancing,
#endif
                
#if UNITY_2020_2_OR_NEWER || UNITY_2019_4
                usePrototypeMesh = grassAsset.UsePrototypeMesh,
#endif
                
#if UNITY_2020_2_OR_NEWER
                holeEdgePadding = grassAsset.HoleEdgePadding,
#endif
                
#if UNITY_2022_2_OR_NEWER
                targetCoverage = grassAsset.TargetCoverage,
                alignToGround = grassAsset.AlignToGround,
                positionJitter = grassAsset.PositionJitter,
                density = grassAsset.DetailDensity,
                useDensityScaling = grassAsset.UseDensityScaling,
#endif
            };
        }
        
        [DocFXIgnore]
        public static bool operator ==(GrassAsset x, GrassAsset y)
        {
            return Equals(x, y);
        }

        [DocFXIgnore]
        public static bool operator !=(GrassAsset x, GrassAsset y)
        {
            return !(x == y);
        }
        
        [DocFXIgnore]
        public static bool operator ==(DetailPrototype x, GrassAsset y)
        {
            return Equals((GrassAsset)x, y);
        }

        [DocFXIgnore]
        public static bool operator !=(DetailPrototype x, GrassAsset y)
        {
            return !(x == y);
        }
        
        [DocFXIgnore]
        public static bool operator ==(GrassAsset x, DetailPrototype y)
        {
            return Equals(x, (GrassAsset)y);
        }

        [DocFXIgnore]
        public static bool operator !=(GrassAsset x, DetailPrototype y)
        {
            return !(x == y);
        }

        [DocFXIgnore]
        public override bool Equals(object other)
        {
            if (other is GrassAsset grassAsset)
            {
                return Equals(grassAsset);
            }
            if (other is DetailPrototype detailPrototype)
            {
                return Equals(detailPrototype);
            }

            return false;
        }

        private bool Equals(GrassAsset grassAsset)
        {
            if (!grassAsset) return false;
            
            bool checkTexture = UsePrototypeMesh || grassAsset.Texture == Texture;
            bool checkMesh = !UsePrototypeMesh || grassAsset.Prototype == Prototype;
            bool checkMinWidth = Mathf.Approximately(grassAsset.MinWidth, MinWidth);
            bool checkMaxWidth = Mathf.Approximately(grassAsset.MaxWidth, MaxWidth);
            bool checkMinHeight = Mathf.Approximately(grassAsset.MinHeight, MinHeight);
            bool checkMaxHeight = Mathf.Approximately(grassAsset.MaxHeight, MaxHeight);
            bool checkNoiseSpread = Mathf.Approximately(grassAsset.NoiseSpread, NoiseSpread);
            bool checkRenderMode = RenderMode == grassAsset.RenderMode;
            
#if UNITY_2021_2_OR_NEWER
            bool useInstancing = UseInstancing;
            bool checkNoiseSeed = grassAsset.NoiseSeed == NoiseSeed;
            bool checkUseInstancing = UseInstancing == grassAsset.UseInstancing;
#else
            bool useInstancing = false;
            bool checkNoiseSeed = true;
            bool checkUseInstancing = true;
#endif
            
            bool checkHealthyColor = useInstancing || ColorUtility.ToHtmlStringRGBA(grassAsset.HealthyTint) == ColorUtility.ToHtmlStringRGBA(HealthyTint);
            bool checkDryColor = useInstancing || ColorUtility.ToHtmlStringRGBA(grassAsset.DryTint) == ColorUtility.ToHtmlStringRGBA(DryTint);

#if UNITY_2020_2_OR_NEWER || UNITY_2019_4
            bool checkUseMesh = UsePrototypeMesh == grassAsset.UsePrototypeMesh;
#else
            bool checkUseMesh = true;
#endif
            
#if UNITY_2020_2_OR_NEWER
            bool checkHoleEdgePadding = Mathf.Approximately(grassAsset.HoleEdgePadding, HoleEdgePadding);
#else
            bool checkHoleEdgePadding = true;
#endif
            
              
#if UNITY_2022_2_OR_NEWER
            bool checkTargetCoverage = Mathf.Approximately(grassAsset.TargetCoverage, TargetCoverage);
            bool checkAlignToGround = Mathf.Approximately(grassAsset.AlignToGround, AlignToGround);
            bool checkPositionJitter = UsePrototypeMesh && QualitySettings.useLegacyDetailDistribution || Mathf.Approximately(grassAsset.PositionJitter, PositionJitter);
            bool checkDensity = Mathf.Approximately(grassAsset.DetailDensity, DetailDensity);
            bool checkUseDensityScaling = grassAsset.UseDensityScaling == UseDensityScaling;
#else
            bool checkTargetCoverage = true;
            bool checkAlignToGround = true;
            bool checkPositionJitter = true;
            bool checkDensity = true;
            bool checkUseDensityScaling = true;
#endif
            
            return checkUseInstancing && checkUseMesh && checkTexture && checkMesh && checkMinWidth && checkMaxWidth && checkMinHeight && checkMaxHeight &&
                   checkNoiseSeed && checkNoiseSpread && checkHoleEdgePadding && checkHealthyColor && checkDryColor && checkRenderMode && checkTargetCoverage && checkAlignToGround && checkPositionJitter && checkDensity && checkUseDensityScaling;
        }

        [DocFXIgnore]
        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(Prototype);
            hashCode.Add(MinHeight);
            hashCode.Add(MaxHeight);
            hashCode.Add(MinWidth);
            hashCode.Add(MaxWidth);
            
            hashCode.Add(Texture);
            hashCode.Add(NoiseSpread);
            hashCode.Add(RenderMode);
            
#if UNITY_2021_2_OR_NEWER
            hashCode.Add(NoiseSeed);
            hashCode.Add(UseInstancing);
            
            if (!UseInstancing)
#endif
            {
                hashCode.Add(ColorUtility.ToHtmlStringRGBA(HealthyTint));
                hashCode.Add(ColorUtility.ToHtmlStringRGBA(DryTint));
            }

#if UNITY_2020_2_OR_NEWER || UNITY_2019_4
            hashCode.Add(UsePrototypeMesh);
#endif
            
#if UNITY_2020_2_OR_NEWER
            hashCode.Add(HoleEdgePadding);
#endif
            
#if UNITY_2022_2_OR_NEWER
            hashCode.Add(TargetCoverage);
            hashCode.Add(AlignToGround);

            if (!UsePrototypeMesh || !QualitySettings.useLegacyDetailDistribution)
            {
                hashCode.Add(PositionJitter);
            }

            hashCode.Add(DetailDensity);
            hashCode.Add(UseDensityScaling);
#endif

            return hashCode.ToHashCode();
        }
    }
}