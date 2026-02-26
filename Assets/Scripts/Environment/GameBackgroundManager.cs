using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LottoDefense.Gameplay;

namespace LottoDefense.Environment
{
    /// <summary>
    /// Manages game background visuals including parallax layers, ambient effects,
    /// and dynamic environmental elements for enhanced visual appeal.
    /// </summary>
    public class GameBackgroundManager : MonoBehaviour
    {
        #region Singleton
        private static GameBackgroundManager _instance;
        public static GameBackgroundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameBackgroundManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameBackgroundManager");
                        _instance = go.AddComponent<GameBackgroundManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Serialized Fields
        [Header("Background Layers")]
        [SerializeField] private List<BackgroundLayer> backgroundLayers;
        [SerializeField] private Transform backgroundContainer;

        [Header("Sky Settings")]
        [SerializeField] private Gradient skyGradient;
        [SerializeField] private float skyTransitionDuration = 30f;
        [SerializeField] private SpriteRenderer skyRenderer;

        [Header("Cloud Settings")]
        [SerializeField] private GameObject cloudPrefab;
        [SerializeField] private int cloudCount = 5;
        [SerializeField] private float cloudSpeedMin = 0.5f;
        [SerializeField] private float cloudSpeedMax = 2f;
        [SerializeField] private float cloudHeightMin = 3f;
        [SerializeField] private float cloudHeightMax = 7f;

        [Header("Particle Effects")]
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private int particleCount = 20;
        [SerializeField] private Color particleColorDay = new Color(1f, 1f, 0.8f, 0.3f);
        [SerializeField] private Color particleColorNight = new Color(0.8f, 0.8f, 1f, 0.3f);

        [Header("Environmental Effects")]
        [SerializeField] private bool enableWindEffect = true;
        [SerializeField] private float windStrength = 1f;
        [SerializeField] private float windCycleTime = 10f;

        [Header("Theme Presets")]
        [SerializeField] private BackgroundTheme currentTheme = BackgroundTheme.Forest;
        #endregion

        #region Private Fields
        private List<Cloud> activeClouds = new List<Cloud>();
        private List<ParticleEffect> activeParticles = new List<ParticleEffect>();
        private float currentSkyTime = 0f;
        private float windPhase = 0f;
        private Camera mainCamera;
        #endregion

        #region Data Classes
        [System.Serializable]
        public class BackgroundLayer
        {
            public string layerName;
            public Sprite layerSprite;
            public float parallaxFactor = 0.5f; // 0 = no movement, 1 = full camera movement
            public float scrollSpeed = 0f; // Auto-scroll speed
            public Color tintColor = Color.white;
            public int sortingOrder = -10;
            public Vector2 offset = Vector2.zero;
            public bool tileHorizontally = true;
            public float scale = 1f;

            [HideInInspector]
            public GameObject layerObject;
            [HideInInspector]
            public SpriteRenderer renderer;
        }

        public enum BackgroundTheme
        {
            Forest,
            Desert,
            Mountain,
            City,
            Space,
            Underwater,
            Volcanic,
            Crystal
        }

        private class Cloud
        {
            public GameObject gameObject;
            public float speed;
            public float startX;
            public float endX;
        }

        private class ParticleEffect
        {
            public GameObject gameObject;
            public Vector3 velocity;
            public float lifeTime;
            public float maxLifeTime;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            mainCamera = Camera.main;
        }

        private void Start()
        {
            InitializeBackground();
            ApplyTheme(currentTheme);
        }

        private void Update()
        {
            UpdateSkyGradient();
            UpdateParallaxLayers();
            UpdateClouds();
            UpdateParticles();
            UpdateWindEffect();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize background system
        /// </summary>
        private void InitializeBackground()
        {
            // Create background container
            if (backgroundContainer == null)
            {
                backgroundContainer = new GameObject("BackgroundContainer").transform;
                backgroundContainer.SetParent(transform);
                backgroundContainer.localPosition = Vector3.zero;
            }

            // Create sky background if not assigned
            if (skyRenderer == null)
            {
                GameObject skyObject = new GameObject("SkyBackground");
                skyObject.transform.SetParent(backgroundContainer);
                skyRenderer = skyObject.AddComponent<SpriteRenderer>();
                skyRenderer.sortingOrder = -100;
                CreateSkySprite();
            }

            // Initialize layers
            CreateBackgroundLayers();

            // Create clouds
            SpawnClouds();

            // Create particles
            SpawnParticles();
        }

        /// <summary>
        /// Create sky gradient sprite
        /// </summary>
        private void CreateSkySprite()
        {
            Texture2D skyTexture = new Texture2D(256, 256);

            for (int y = 0; y < 256; y++)
            {
                float t = y / 255f;
                Color skyColor = skyGradient != null ?
                    skyGradient.Evaluate(t) :
                    Color.Lerp(new Color(0.5f, 0.7f, 1f), new Color(0.1f, 0.2f, 0.4f), t);

                for (int x = 0; x < 256; x++)
                {
                    skyTexture.SetPixel(x, y, skyColor);
                }
            }

            skyTexture.Apply();
            Sprite skySprite = Sprite.Create(skyTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 100f);
            skyRenderer.sprite = skySprite;

            // Scale to cover screen
            if (mainCamera != null)
            {
                float height = mainCamera.orthographicSize * 2f;
                float width = height * mainCamera.aspect;
                skyRenderer.transform.localScale = new Vector3(width / 2.56f, height / 2.56f, 1f);
            }
        }

        /// <summary>
        /// Create background layers
        /// </summary>
        private void CreateBackgroundLayers()
        {
            if (backgroundLayers == null || backgroundLayers.Count == 0)
            {
                // Create default layers if none specified
                backgroundLayers = CreateDefaultLayers();
            }

            foreach (var layer in backgroundLayers)
            {
                CreateLayer(layer);
            }
        }

        /// <summary>
        /// Create a single background layer
        /// </summary>
        private void CreateLayer(BackgroundLayer layer)
        {
            if (layer.layerSprite == null) return;

            layer.layerObject = new GameObject($"Layer_{layer.layerName}");
            layer.layerObject.transform.SetParent(backgroundContainer);
            layer.layerObject.transform.localPosition = new Vector3(layer.offset.x, layer.offset.y, 0);

            layer.renderer = layer.layerObject.AddComponent<SpriteRenderer>();
            layer.renderer.sprite = layer.layerSprite;
            layer.renderer.sortingOrder = layer.sortingOrder;
            layer.renderer.color = layer.tintColor;

            // Apply scale
            layer.layerObject.transform.localScale = Vector3.one * layer.scale;

            // If tiling horizontally, create additional sprites
            if (layer.tileHorizontally && mainCamera != null)
            {
                float spriteWidth = layer.layerSprite.bounds.size.x * layer.scale;
                float screenWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;
                int tilesNeeded = Mathf.CeilToInt(screenWidth / spriteWidth) + 2;

                for (int i = 1; i < tilesNeeded; i++)
                {
                    GameObject tileObject = new GameObject($"Layer_{layer.layerName}_Tile{i}");
                    tileObject.transform.SetParent(layer.layerObject.transform);
                    tileObject.transform.localPosition = new Vector3(spriteWidth * i, 0, 0);

                    SpriteRenderer tileRenderer = tileObject.AddComponent<SpriteRenderer>();
                    tileRenderer.sprite = layer.layerSprite;
                    tileRenderer.sortingOrder = layer.sortingOrder;
                    tileRenderer.color = layer.tintColor;
                }
            }
        }
        #endregion

        #region Theme Management
        /// <summary>
        /// Apply a background theme
        /// </summary>
        public void ApplyTheme(BackgroundTheme theme)
        {
            currentTheme = theme;

            // Clear existing layers
            ClearBackgroundLayers();

            // Get theme-specific layers
            backgroundLayers = GetThemeLayers(theme);

            // Recreate layers
            CreateBackgroundLayers();

            // Adjust sky gradient
            skyGradient = GetThemeSkyGradient(theme);
            CreateSkySprite();

            // Adjust particle colors
            UpdateParticleColors(theme);

            Debug.Log($"[GameBackgroundManager] Applied theme: {theme}");
        }

        /// <summary>
        /// Get layers for specific theme
        /// </summary>
        private List<BackgroundLayer> GetThemeLayers(BackgroundTheme theme)
        {
            List<BackgroundLayer> layers = new List<BackgroundLayer>();

            switch (theme)
            {
                case BackgroundTheme.Forest:
                    layers.Add(new BackgroundLayer
                    {
                        layerName = "Mountains",
                        parallaxFactor = 0.2f,
                        tintColor = new Color(0.6f, 0.7f, 0.8f),
                        sortingOrder = -50
                    });
                    layers.Add(new BackgroundLayer
                    {
                        layerName = "Trees_Far",
                        parallaxFactor = 0.4f,
                        tintColor = new Color(0.4f, 0.6f, 0.4f),
                        sortingOrder = -40
                    });
                    layers.Add(new BackgroundLayer
                    {
                        layerName = "Trees_Near",
                        parallaxFactor = 0.6f,
                        tintColor = new Color(0.3f, 0.5f, 0.3f),
                        sortingOrder = -30
                    });
                    break;

                case BackgroundTheme.Desert:
                    layers.Add(new BackgroundLayer
                    {
                        layerName = "Dunes_Far",
                        parallaxFactor = 0.3f,
                        tintColor = new Color(1f, 0.9f, 0.7f),
                        sortingOrder = -50
                    });
                    layers.Add(new BackgroundLayer
                    {
                        layerName = "Dunes_Near",
                        parallaxFactor = 0.5f,
                        tintColor = new Color(0.9f, 0.8f, 0.6f),
                        sortingOrder = -40
                    });
                    break;

                case BackgroundTheme.Space:
                    layers.Add(new BackgroundLayer
                    {
                        layerName = "Stars",
                        parallaxFactor = 0.1f,
                        scrollSpeed = 0.02f,
                        tintColor = Color.white,
                        sortingOrder = -60
                    });
                    layers.Add(new BackgroundLayer
                    {
                        layerName = "Nebula",
                        parallaxFactor = 0.2f,
                        tintColor = new Color(0.8f, 0.5f, 1f, 0.5f),
                        sortingOrder = -50
                    });
                    break;

                default:
                    return CreateDefaultLayers();
            }

            return layers;
        }

        /// <summary>
        /// Get sky gradient for theme
        /// </summary>
        private Gradient GetThemeSkyGradient(BackgroundTheme theme)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys;
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            switch (theme)
            {
                case BackgroundTheme.Forest:
                    colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0f),
                        new GradientColorKey(new Color(0.9f, 0.9f, 0.7f), 0.5f),
                        new GradientColorKey(new Color(0.3f, 0.5f, 0.8f), 1f)
                    };
                    break;

                case BackgroundTheme.Desert:
                    colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(new Color(1f, 0.7f, 0.4f), 0f),
                        new GradientColorKey(new Color(1f, 0.9f, 0.6f), 0.5f),
                        new GradientColorKey(new Color(0.8f, 0.5f, 0.3f), 1f)
                    };
                    break;

                case BackgroundTheme.Space:
                    colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(new Color(0f, 0f, 0.1f), 0f),
                        new GradientColorKey(new Color(0.1f, 0f, 0.2f), 0.5f),
                        new GradientColorKey(new Color(0f, 0f, 0.05f), 1f)
                    };
                    break;

                default:
                    colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0f),
                        new GradientColorKey(new Color(0.2f, 0.4f, 0.8f), 1f)
                    };
                    break;
            }

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }

        /// <summary>
        /// Create default background layers
        /// </summary>
        private List<BackgroundLayer> CreateDefaultLayers()
        {
            return new List<BackgroundLayer>
            {
                new BackgroundLayer
                {
                    layerName = "Default_Far",
                    parallaxFactor = 0.3f,
                    tintColor = new Color(0.7f, 0.8f, 0.9f),
                    sortingOrder = -50
                },
                new BackgroundLayer
                {
                    layerName = "Default_Mid",
                    parallaxFactor = 0.5f,
                    tintColor = new Color(0.8f, 0.85f, 0.9f),
                    sortingOrder = -40
                },
                new BackgroundLayer
                {
                    layerName = "Default_Near",
                    parallaxFactor = 0.7f,
                    tintColor = new Color(0.85f, 0.9f, 0.95f),
                    sortingOrder = -30
                }
            };
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Update sky gradient based on time
        /// </summary>
        private void UpdateSkyGradient()
        {
            if (skyGradient == null || skyRenderer == null) return;

            currentSkyTime += Time.deltaTime / skyTransitionDuration;
            if (currentSkyTime > 1f) currentSkyTime -= 1f;

            // Animate sky color through the gradient
            float t = Mathf.PingPong(currentSkyTime * 2f, 1f);

            // Update sky color (would need proper gradient animation)
            // This is simplified - in production you'd properly animate the gradient
        }

        /// <summary>
        /// Update parallax layers based on camera movement
        /// </summary>
        private void UpdateParallaxLayers()
        {
            if (backgroundLayers == null || mainCamera == null) return;

            Vector3 cameraPos = mainCamera.transform.position;

            foreach (var layer in backgroundLayers)
            {
                if (layer.layerObject == null) continue;

                // Apply parallax effect
                float parallaxX = cameraPos.x * layer.parallaxFactor;
                float parallaxY = cameraPos.y * layer.parallaxFactor * 0.5f; // Less vertical parallax

                // Apply auto-scroll
                float scrollX = Time.time * layer.scrollSpeed;

                layer.layerObject.transform.position = new Vector3(
                    layer.offset.x + parallaxX + scrollX,
                    layer.offset.y + parallaxY,
                    layer.layerObject.transform.position.z
                );
            }
        }

        /// <summary>
        /// Update cloud positions
        /// </summary>
        private void UpdateClouds()
        {
            foreach (var cloud in activeClouds)
            {
                if (cloud.gameObject == null) continue;

                // Move cloud
                cloud.gameObject.transform.position += Vector3.right * cloud.speed * Time.deltaTime;

                // Wrap around when reaching edge
                if (cloud.gameObject.transform.position.x > cloud.endX)
                {
                    Vector3 pos = cloud.gameObject.transform.position;
                    pos.x = cloud.startX;
                    cloud.gameObject.transform.position = pos;
                }
            }
        }

        /// <summary>
        /// Update particle effects
        /// </summary>
        private void UpdateParticles()
        {
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                var particle = activeParticles[i];
                if (particle.gameObject == null)
                {
                    activeParticles.RemoveAt(i);
                    continue;
                }

                // Update position
                particle.gameObject.transform.position += particle.velocity * Time.deltaTime;

                // Update lifetime
                particle.lifeTime += Time.deltaTime;

                // Fade out
                SpriteRenderer sr = particle.gameObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color color = sr.color;
                    color.a = Mathf.Lerp(0.3f, 0f, particle.lifeTime / particle.maxLifeTime);
                    sr.color = color;
                }

                // Respawn if expired
                if (particle.lifeTime > particle.maxLifeTime)
                {
                    RespawnParticle(particle);
                }
            }
        }

        /// <summary>
        /// Update wind effect on elements
        /// </summary>
        private void UpdateWindEffect()
        {
            if (!enableWindEffect) return;

            windPhase += Time.deltaTime / windCycleTime * Mathf.PI * 2f;
            float windOffset = Mathf.Sin(windPhase) * windStrength;

            // Apply wind to clouds
            foreach (var cloud in activeClouds)
            {
                if (cloud.gameObject != null)
                {
                    Vector3 pos = cloud.gameObject.transform.position;
                    pos.y += windOffset * Time.deltaTime * 0.1f;
                    cloud.gameObject.transform.position = pos;
                }
            }
        }
        #endregion

        #region Cloud Management
        /// <summary>
        /// Spawn cloud objects
        /// </summary>
        private void SpawnClouds()
        {
            if (mainCamera == null) return;

            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            for (int i = 0; i < cloudCount; i++)
            {
                Cloud cloud = new Cloud();

                cloud.gameObject = CreateCloudObject($"Cloud_{i}");
                cloud.speed = Random.Range(cloudSpeedMin, cloudSpeedMax);
                cloud.startX = -screenWidth * 0.6f;
                cloud.endX = screenWidth * 0.6f;

                // Random position
                float x = Random.Range(cloud.startX, cloud.endX);
                float y = Random.Range(cloudHeightMin, cloudHeightMax);
                cloud.gameObject.transform.position = new Vector3(x, y, 0);

                activeClouds.Add(cloud);
            }
        }

        /// <summary>
        /// Create a cloud object
        /// </summary>
        private GameObject CreateCloudObject(string name)
        {
            GameObject cloudObj = new GameObject(name);
            cloudObj.transform.SetParent(backgroundContainer);

            SpriteRenderer sr = cloudObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCloudSprite();
            sr.sortingOrder = -20;
            sr.color = new Color(1f, 1f, 1f, 0.7f);

            float scale = Random.Range(0.8f, 1.5f);
            cloudObj.transform.localScale = Vector3.one * scale;

            return cloudObj;
        }

        /// <summary>
        /// Create a simple cloud sprite
        /// </summary>
        private Sprite CreateCloudSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                    float alpha = 1f - (dist / (size / 2));
                    alpha = Mathf.Clamp01(alpha);
                    alpha = Mathf.Pow(alpha, 2f); // Soften edges

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
        #endregion

        #region Particle Management
        /// <summary>
        /// Spawn particle effects
        /// </summary>
        private void SpawnParticles()
        {
            if (mainCamera == null) return;

            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            for (int i = 0; i < particleCount; i++)
            {
                ParticleEffect particle = new ParticleEffect();

                particle.gameObject = CreateParticleObject($"Particle_{i}");
                particle.velocity = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-0.5f, -2f),
                    0
                );
                particle.maxLifeTime = Random.Range(5f, 10f);
                particle.lifeTime = Random.Range(0f, particle.maxLifeTime);

                // Random position
                float x = Random.Range(-screenWidth * 0.5f, screenWidth * 0.5f);
                float y = Random.Range(-screenHeight * 0.5f, screenHeight * 0.5f);
                particle.gameObject.transform.position = new Vector3(x, y, 0);

                activeParticles.Add(particle);
            }
        }

        /// <summary>
        /// Create a particle object
        /// </summary>
        private GameObject CreateParticleObject(string name)
        {
            GameObject particleObj = new GameObject(name);
            particleObj.transform.SetParent(backgroundContainer);

            SpriteRenderer sr = particleObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateParticleSprite();
            sr.sortingOrder = -15;
            sr.color = particleColorDay;

            float scale = Random.Range(0.1f, 0.3f);
            particleObj.transform.localScale = Vector3.one * scale;

            return particleObj;
        }

        /// <summary>
        /// Create a simple particle sprite
        /// </summary>
        private Sprite CreateParticleSprite()
        {
            int size = 16;
            Texture2D texture = new Texture2D(size, size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                    float alpha = 1f - (dist / (size / 2));
                    alpha = Mathf.Clamp01(alpha);

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        /// <summary>
        /// Respawn a particle at a new position
        /// </summary>
        private void RespawnParticle(ParticleEffect particle)
        {
            if (mainCamera == null) return;

            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            particle.lifeTime = 0f;
            particle.velocity = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.5f, -2f),
                0
            );

            float x = Random.Range(-screenWidth * 0.5f, screenWidth * 0.5f);
            float y = screenHeight * 0.6f;
            particle.gameObject.transform.position = new Vector3(x, y, 0);
        }

        /// <summary>
        /// Update particle colors based on theme
        /// </summary>
        private void UpdateParticleColors(BackgroundTheme theme)
        {
            Color newColor = particleColorDay;

            switch (theme)
            {
                case BackgroundTheme.Forest:
                    newColor = new Color(0.8f, 1f, 0.6f, 0.3f); // Light green
                    break;
                case BackgroundTheme.Desert:
                    newColor = new Color(1f, 0.9f, 0.6f, 0.3f); // Sandy
                    break;
                case BackgroundTheme.Space:
                    newColor = new Color(0.8f, 0.8f, 1f, 0.5f); // Starlight
                    break;
                case BackgroundTheme.Volcanic:
                    newColor = new Color(1f, 0.4f, 0.2f, 0.4f); // Embers
                    break;
            }

            foreach (var particle in activeParticles)
            {
                if (particle.gameObject != null)
                {
                    SpriteRenderer sr = particle.gameObject.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = newColor;
                    }
                }
            }
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Clear all background layers
        /// </summary>
        private void ClearBackgroundLayers()
        {
            if (backgroundLayers != null)
            {
                foreach (var layer in backgroundLayers)
                {
                    if (layer.layerObject != null)
                    {
                        Destroy(layer.layerObject);
                    }
                }
                backgroundLayers.Clear();
            }
        }

        /// <summary>
        /// Clean up all background elements
        /// </summary>
        public void CleanupBackground()
        {
            ClearBackgroundLayers();

            foreach (var cloud in activeClouds)
            {
                if (cloud.gameObject != null)
                    Destroy(cloud.gameObject);
            }
            activeClouds.Clear();

            foreach (var particle in activeParticles)
            {
                if (particle.gameObject != null)
                    Destroy(particle.gameObject);
            }
            activeParticles.Clear();
        }

        private void OnDestroy()
        {
            CleanupBackground();
        }
        #endregion
    }
}