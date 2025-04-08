using UnityEngine;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumItemMaterials : MonoBehaviour
    {
        [System.Serializable]
        public class ItemMaterialProperties
        {
            public Material baseMaterial;
            public Color primaryColor = Color.white;
            public Color secondaryColor = Color.white;
            public float emissionIntensity = 1f;
            public float pulseSpeed = 1f;
            public float pulseAmount = 0.5f;
        }

        [Header("Ring Properties")]
        public ItemMaterialProperties ringProperties = new ItemMaterialProperties
        {
            primaryColor = new Color(1f, 0.5f, 0f, 1f), // Orange
            secondaryColor = new Color(1f, 0.8f, 0f, 1f), // Gold
            emissionIntensity = 2f,
            pulseSpeed = 1.5f,
            pulseAmount = 0.7f
        };

        [Header("Puzzle Properties")]
        public ItemMaterialProperties puzzleProperties = new ItemMaterialProperties
        {
            primaryColor = new Color(0f, 0.5f, 1f, 1f), // Blue
            secondaryColor = new Color(0.5f, 0.8f, 1f, 1f), // Light Blue
            emissionIntensity = 1.5f,
            pulseSpeed = 1f,
            pulseAmount = 0.5f
        };

        [Header("Eye Properties")]
        public ItemMaterialProperties eyeProperties = new ItemMaterialProperties
        {
            primaryColor = new Color(1f, 0f, 0f, 1f), // Red
            secondaryColor = new Color(1f, 0.3f, 0.3f, 1f), // Light Red
            emissionIntensity = 2.5f,
            pulseSpeed = 2f,
            pulseAmount = 0.8f
        };

        [Header("Rod Properties")]
        public ItemMaterialProperties rodProperties = new ItemMaterialProperties
        {
            primaryColor = new Color(0.5f, 0f, 1f, 1f), // Purple
            secondaryColor = new Color(0.8f, 0.5f, 1f, 1f), // Light Purple
            emissionIntensity = 1.8f,
            pulseSpeed = 1.2f,
            pulseAmount = 0.6f
        };

        [Header("Key Properties")]
        public ItemMaterialProperties keyProperties = new ItemMaterialProperties
        {
            primaryColor = new Color(0f, 1f, 0.5f, 1f), // Green
            secondaryColor = new Color(0.5f, 1f, 0.8f, 1f), // Light Green
            emissionIntensity = 1.6f,
            pulseSpeed = 1.1f,
            pulseAmount = 0.5f
        };

        [Header("Scale Properties")]
        public ItemMaterialProperties scaleProperties = new ItemMaterialProperties
        {
            primaryColor = new Color(1f, 1f, 0f, 1f), // Yellow
            secondaryColor = new Color(1f, 0.8f, 0.5f, 1f), // Light Yellow
            emissionIntensity = 1.7f,
            pulseSpeed = 1.3f,
            pulseAmount = 0.6f
        };

        [Header("Necklace Properties")]
        public ItemMaterialProperties necklaceProperties = new ItemMaterialProperties
        {
            primaryColor = new Color(1f, 0f, 0.5f, 1f), // Pink
            secondaryColor = new Color(1f, 0.5f, 0.8f, 1f), // Light Pink
            emissionIntensity = 1.9f,
            pulseSpeed = 1.4f,
            pulseAmount = 0.7f
        };

        private void Start()
        {
            InitializeMaterials();
        }

        private void InitializeMaterials()
        {
            // Initialize Ring material
            InitializeItemMaterial(ringProperties, "YuGiOh/MillenniumItem/GlowEffect");
            InitializeItemMaterial(ringProperties, "YuGiOh/MillenniumItem/OutlineEffect");

            // Initialize Puzzle material
            InitializeItemMaterial(puzzleProperties, "YuGiOh/MillenniumItem/GlowEffect");
            InitializeItemMaterial(puzzleProperties, "YuGiOh/MillenniumItem/ShieldEffect");

            // Initialize Eye material
            InitializeItemMaterial(eyeProperties, "YuGiOh/MillenniumItem/GlowEffect");
            InitializeItemMaterial(eyeProperties, "YuGiOh/MillenniumItem/FutureVisionEffect");

            // Initialize Rod material
            InitializeItemMaterial(rodProperties, "YuGiOh/MillenniumItem/GlowEffect");
            InitializeItemMaterial(rodProperties, "YuGiOh/MillenniumItem/PowerLevelEffect");

            // Initialize Key material
            InitializeItemMaterial(keyProperties, "YuGiOh/MillenniumItem/GlowEffect");
            InitializeItemMaterial(keyProperties, "YuGiOh/MillenniumItem/ShieldEffect");

            // Initialize Scale material
            InitializeItemMaterial(scaleProperties, "YuGiOh/MillenniumItem/GlowEffect");
            InitializeItemMaterial(scaleProperties, "YuGiOh/MillenniumItem/PowerLevelEffect");

            // Initialize Necklace material
            InitializeItemMaterial(necklaceProperties, "YuGiOh/MillenniumItem/GlowEffect");
            InitializeItemMaterial(necklaceProperties, "YuGiOh/MillenniumItem/MindControlEffect");
        }

        private void InitializeItemMaterial(ItemMaterialProperties properties, string shaderName)
        {
            if (properties.baseMaterial == null)
            {
                properties.baseMaterial = new Material(Shader.Find(shaderName));
            }

            properties.baseMaterial.SetColor("_Color", properties.primaryColor);
            properties.baseMaterial.SetFloat("_EmissionIntensity", properties.emissionIntensity);
            properties.baseMaterial.SetFloat("_PulseSpeed", properties.pulseSpeed);
            properties.baseMaterial.SetFloat("_PulseAmount", properties.pulseAmount);

            // Set additional properties based on shader type
            switch (shaderName)
            {
                case "YuGiOh/MillenniumItem/OutlineEffect":
                    properties.baseMaterial.SetColor("_OutlineColor", properties.secondaryColor);
                    properties.baseMaterial.SetFloat("_OutlineWidth", 0.1f);
                    break;

                case "YuGiOh/MillenniumItem/ShieldEffect":
                    properties.baseMaterial.SetColor("_ShieldColor", properties.secondaryColor);
                    properties.baseMaterial.SetFloat("_ShieldPulse", 0.5f);
                    properties.baseMaterial.SetFloat("_RimPower", 3f);
                    break;

                case "YuGiOh/MillenniumItem/FutureVisionEffect":
                    properties.baseMaterial.SetColor("_VisionColor", properties.secondaryColor);
                    properties.baseMaterial.SetFloat("_VisionAlpha", 0.5f);
                    properties.baseMaterial.SetFloat("_TimeDistortion", 0.5f);
                    properties.baseMaterial.SetFloat("_DistortionSpeed", 2f);
                    break;

                case "YuGiOh/MillenniumItem/PowerLevelEffect":
                    properties.baseMaterial.SetColor("_PowerColor", properties.secondaryColor);
                    properties.baseMaterial.SetFloat("_PowerLevel", 0.7f);
                    break;

                case "YuGiOh/MillenniumItem/MindControlEffect":
                    properties.baseMaterial.SetColor("_ControlColor", properties.secondaryColor);
                    properties.baseMaterial.SetFloat("_ControlPulse", 0.6f);
                    properties.baseMaterial.SetFloat("_NoiseScale", 15f);
                    break;
            }
        }

        public Material GetItemMaterial(MillenniumItemType itemType, string effectType)
        {
            ItemMaterialProperties properties = GetPropertiesForItemType(itemType);
            string shaderName = GetShaderNameForEffect(effectType);
            return properties?.baseMaterial;
        }

        private ItemMaterialProperties GetPropertiesForItemType(MillenniumItemType itemType)
        {
            return itemType switch
            {
                MillenniumItemType.Ring => ringProperties,
                MillenniumItemType.Puzzle => puzzleProperties,
                MillenniumItemType.Eye => eyeProperties,
                MillenniumItemType.Rod => rodProperties,
                MillenniumItemType.Key => keyProperties,
                MillenniumItemType.Scale => scaleProperties,
                MillenniumItemType.Necklace => necklaceProperties,
                _ => null
            };
        }

        private string GetShaderNameForEffect(string effectType)
        {
            return effectType switch
            {
                "Glow" => "YuGiOh/MillenniumItem/GlowEffect",
                "Outline" => "YuGiOh/MillenniumItem/OutlineEffect",
                "Shield" => "YuGiOh/MillenniumItem/ShieldEffect",
                "FutureVision" => "YuGiOh/MillenniumItem/FutureVisionEffect",
                "PowerLevel" => "YuGiOh/MillenniumItem/PowerLevelEffect",
                "MindControl" => "YuGiOh/MillenniumItem/MindControlEffect",
                _ => "YuGiOh/MillenniumItem/GlowEffect"
            };
        }
    }
} 