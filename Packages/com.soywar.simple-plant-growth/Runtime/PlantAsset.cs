using UnityEngine;

namespace SoyWar.SimplePlantGrowth
{
    /// <summary>
    /// Represents all kind of plants.
    /// </summary>
    public abstract class PlantAsset<T> : ScriptableObject where T : PlantAsset<T>
    {
        [SerializeField] private bool _enableRatio;
        [SerializeField] private float _ratio = 1;
        [SerializeField] private T _next;
        [SerializeField] private bool _destroy;
        [SerializeField] private GameObject _prototype;
        [SerializeField] private float _minWidth = 1;
        [SerializeField] private float _maxWidth = 1.1f;
        [SerializeField] private float _minHeight = 1;
        [SerializeField] private float _maxHeight = 1.1f;
        [SerializeField] private float _minTimeout = 60;
        [SerializeField] private float _maxTimeout = 60;
        [SerializeField] private bool _randomTimeout;
        [SerializeField] private bool _randomWidth = true;
        [SerializeField] private bool _randomHeight = true;
        [SerializeField] private int _requiredCollectibles = 5;
        [SerializeField] private int _currentCollectibles = 0;

        /// <summary>
        /// The next phase of plant growth.
        /// </summary>
        public T Next
        {
            get => _next;
            set
            {
                if (value)
                {
                    _destroy = false;
                }
                
                _next = value;
            }
        }

        /// <summary>
        /// Check whether the next phase leads to plant growth or destruction.
        /// </summary>
        public bool HasNextStep => Next || Destroy;

        /// <summary>
        /// Destroys the plant when moving on to the next phase.
        /// </summary>
        public bool Destroy
        {
            get => _destroy;
            set
            {
                if (value)
                {
                    _next = null;
                }

                _destroy = value;
            }
        }

        /// <summary>
        /// Activates random growth time.
        /// </summary>
        public bool RandomGrowthTime
        {
            get => _randomTimeout;
            set => _randomTimeout = value;
        }
        
        /// <summary>
        /// Growth time
        /// </summary>
        /// <remarks>
        /// When a value is set, disables random growth time.
        /// </remarks>
        public float GrowthTime
        {
            get => _minTimeout;
            set
            {
                _randomTimeout = false;
                _minTimeout = _maxTimeout = Mathf.Clamp(value, Mathf.Epsilon, float.MaxValue);
            }
        }
        
        /// <summary>
        /// Minimum growth time
        /// </summary>
        /// <remarks>
        /// When a value is set, enables random growth time.
        /// </remarks>
        public float MinGrowthTime
        {
            get => _minTimeout;
            set 
            {
                _randomTimeout = true;
                _minTimeout = Mathf.Clamp(value, Mathf.Epsilon, _maxTimeout);
            }
        }
        
        /// <summary>
        /// Maximum growth time
        /// </summary>
        /// <remarks>
        /// When a value is set, enables random growth time.
        /// </remarks>
        public float MaxGrowthTime
        {
            get => _maxTimeout;
            set
            {
                _randomTimeout = true;
                _maxTimeout = Mathf.Clamp(value, _minTimeout, float.MaxValue);
            }
        }
        
        /// <summary>
        /// Locks width to height.
        /// </summary>
        public bool LockWidthToHeight
        {
            get => _enableRatio;
            set => _enableRatio = value;
        }
        
        /// <summary>
        /// Ratio between width and height.
        /// </summary>
        /// <remarks>
        /// When a value is set, locks width to height.
        /// </remarks>
        public float Ratio
        {
            get => _enableRatio ? _ratio : -1;
            set
            {
                _enableRatio = true;
                _ratio = Mathf.Max(value, Mathf.Epsilon);
            }
        }
        
        /// <summary>
        /// GameObject associated with the plant.
        /// </summary>
        public virtual GameObject Prototype
        {
            get => _prototype;
            set => _prototype = value;
        }
        
        /// <summary>
        /// Activates random width.
        /// </summary>
        public bool RandomWidth
        {
            get => _randomWidth;
            set => _randomWidth = value;
        }
        
        /// <summary>
        /// Width
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a value is set and width doesn't locks to height, disables random width.
        /// </para>
        /// <para>
        /// When a value is set and width is locked to height, ratio is modified.
        /// </para>
        /// </remarks>
        public float Width
        {
            get => _enableRatio ? _minHeight * _ratio : _minWidth;
            set
            {
                if (_enableRatio)
                {
                    _ratio = value / _minHeight;
                }
                else
                {
                    _randomWidth = false;
                    _minWidth = _maxWidth = Mathf.Clamp(value, Mathf.Epsilon, float.MaxValue);
                }
            }
        }
        
        /// <summary>
        /// Minimum width
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a value is set and width doesn't locks to height, enables random width.
        /// </para>
        /// <para>
        /// When a value is set and width is locked to height, ratio is modified.
        /// </para>
        /// </remarks>
        public float MinWidth
        {
            get => _enableRatio ? _minHeight * _ratio : _minWidth;
            set 
            {
                if (_enableRatio)
                {
                    _ratio = value / _minHeight;
                }
                else
                {
                    _randomWidth = true;
                    _minWidth = Mathf.Clamp(value, Mathf.Epsilon, _maxWidth);
                }
            }
        }
        
        /// <summary>
        /// Maximum width
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a value is set and width doesn't locks to height, enables random width.
        /// </para>
        /// <para>
        /// When a value is set and width is locked to height, ratio is modified.
        /// </para>
        /// </remarks>
        public float MaxWidth
        {
            get => _enableRatio ? _maxHeight * _ratio : _maxWidth;
            set 
            {
                if (_enableRatio)
                {
                    _ratio = value / _maxHeight;
                }
                else
                {
                    _randomWidth = true;
                    _maxWidth = Mathf.Clamp(value, _minWidth, float.MaxValue);
                }
            }
        }
        
        /// <summary>
        /// Activates random height.
        /// </summary>
        public bool RandomHeight
        {
            get => _randomHeight;
            set => _randomHeight = value;
        }

        /// <summary>
        /// Height
        /// </summary>
        /// <remarks>
        /// When a value is set, disables random height.
        /// </remarks>
        public float Height
        {
            get => _minHeight;
            set
            {
                _randomHeight = false;
                _minHeight = _maxHeight = Mathf.Clamp(value, Mathf.Epsilon, float.MaxValue);
            }
        }
        
        /// <summary>
        /// Minimum height
        /// </summary>
        /// <remarks>
        /// When a value is set, enables random height.
        /// </remarks>
        public float MinHeight
        {
            get => _minHeight;
            set
            {
                _randomHeight = true;
                _maxHeight = Mathf.Clamp(value, Mathf.Epsilon, _maxHeight);
            }
        }
        
        /// <summary>
        /// Maximum height
        /// </summary>
        /// <remarks>
        /// When a value is set, enables random height.
        /// </remarks>
        public float MaxHeight
        {
            get => _maxHeight;
            set
            {
                _randomHeight = true;
                _maxHeight = Mathf.Clamp(value, _minHeight, float.MaxValue);
            }
        }
        
        /// <summary>
        /// Create a copy of asset.
        /// </summary>
        public T Duplicate()
        {
            T asset = CreateInstance<T>();
            
            asset.CopyFrom((T)this);

            return asset;
        }

        /// <summary>
        /// Whether the plant grows based on collectible count.
        /// </summary>

        /// <summary>
        /// Required number of collectibles for growth.
        /// </summary>
        public int RequiredCollectibles
        {
            get => _requiredCollectibles;
            set => _requiredCollectibles = Mathf.Max(0, value);
        }

        /// <summary>
        /// Current collected collectibles.
        /// </summary>
        public int CurrentCollectibles
        {
            get => _currentCollectibles;
            set => _currentCollectibles = Mathf.Max(0, value);
        }

        /// <summary>
        /// Checks if the plant can grow based on the current condition.
        /// </summary>
        public bool CanGrow()
        {
              return _currentCollectibles >= _requiredCollectibles;
        }

        /// <summary>
        /// Increment collectibles and check growth.
        /// </summary>
        public void AddCollectible()
        {
              _currentCollectibles++;
        }

        internal protected virtual void CopyFrom(T asset)
        {
            _next = asset._next;
            _destroy = asset._destroy;
            _prototype = asset._prototype;
            _minHeight = asset._minHeight;
            _maxHeight = asset._maxHeight;
            _minWidth = asset._minWidth;
            _maxWidth = asset._maxWidth;
            _minTimeout = asset._minTimeout;
            _maxTimeout = asset._maxTimeout;
            _randomTimeout = asset._randomTimeout;
            _randomWidth = asset._randomWidth;
            _randomHeight = asset._randomHeight;
            _enableRatio = asset._enableRatio;
            _ratio = asset._ratio;
            _requiredCollectibles = asset._requiredCollectibles;
        }
        
        /// <summary>
        /// Defines the random range of growth time.
        /// </summary>
        /// <param name="min">Minimum growth time.</param>
        /// <param name="max">Maximum growth time.</param>
        /// <remarks>Enables random growth time.</remarks>
        public void SetMinMaxGrowthTime(float min, float max)
        {
            _randomTimeout = true;
            _minTimeout = Mathf.Max(min, Mathf.Epsilon);
            MaxGrowthTime = max;
        }
        
        /// <summary>
        /// Defines the random range of height.
        /// </summary>
        /// <param name="min">Minimum height.</param>
        /// <param name="max">Maximum height.</param>
        /// <remarks>Enables random height.</remarks>
        public void SetMinMaxHeight(float min, float max)
        {
            _randomHeight = true;
            _minHeight = Mathf.Max(min, Mathf.Epsilon);
            MaxHeight = max;
        }

        /// <summary>
        /// Defines the random range of width.
        /// </summary>
        /// <param name="min">Minimum growth time.</param>
        /// <param name="max">Maximum growth time.</param>
        /// <remarks>Enables random width.</remarks>
        public void SetMinMaxWidth(float min, float max)
        {
            _randomWidth = true;
            _minWidth = Mathf.Max(min, Mathf.Epsilon);
            MaxWidth = max;
        }
        
        /// <summary>
        /// Checks whether asset is valid.
        /// </summary>
        public bool Validate()
        {
            return Validate(out _);
        }
        
        /// <summary>
        /// Checks whether asset is valid.
        /// </summary>
        /// <param name="errorMessage">Error message when asset is invalid.</param>
        public abstract bool Validate(out string errorMessage);
    }
}