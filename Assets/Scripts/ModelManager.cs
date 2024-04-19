using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SolarModule
{
    public class ModelManager : MonoBehaviour
    {
        public static ModelManager Instance = null;

        public enum Direction
        {
            East = 0, North = 1, West = 2, South = 3
        }

        [HideInInspector]
        public Direction direction;

        public enum Obstacles
        {
            None = 0, Clouds = 1, Tree = 2, Wall = 3
        }

        [HideInInspector]
        public Obstacles obstacles;

        public GameObject m_SolarPanelHolder, m_LowSolarPanel, m_MidSolarPanel, m_HighSolarPanel, m_SolarPanelUI;

        private Transform activeSolarPanel;

        public GameObject m_Clouds, m_Wall, m_Tree, m_Sun;

        [HideInInspector]
        public float currentAngle;

        [HideInInspector]
        public const int ROTATION_ANGLE = 5;

        [HideInInspector]
        public const float CLOUD_100W = 1f;
        [HideInInspector]
        public const float CLOUD_150W = 1.5f;
        [HideInInspector]
        public const float OTHER_100W = 0.4f;
        [HideInInspector]
        public const float OTHER_150W = 0.9f;

        public Material m_PlaneMaterial;

        ARPlaneManager planeManager;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            gameObject.AddComponent<ARAnchor>();
        }

        private void Start()
        {
            ClearSolarPanels();
            ClearObstacles();
            CleanPlanes();
        }

        private void CleanPlanes()
        {
            if (planeManager == null) planeManager = FindObjectOfType<ARPlaneManager>();

            // Set plane transparent
            if (m_PlaneMaterial.HasProperty("_TexTintColor"))
            {
                // Set the color of the property
                m_PlaneMaterial.SetColor("_TexTintColor", new(1, 1, 1, 0));
            }
            else
            {
                Debug.LogWarning("Material does not have property: _TexTintColor");
            }

            planeManager.enabled = false;

            // Get all existing planes
            ARPlane[] allPlanes = FindObjectsOfType<ARPlane>();

            // Destroy all existing planes
            foreach (var plane in allPlanes)
            {
                Destroy(plane.gameObject);
            }
        }

        #region Solar Panels

        public void ClearSolarPanels()
        {
            direction = Direction.East;

            m_SolarPanelHolder.transform.localRotation = Quaternion.identity;

            m_LowSolarPanel.transform.GetChild(0).localRotation = Quaternion.identity;
            m_MidSolarPanel.transform.GetChild(0).localRotation = Quaternion.identity;
            m_HighSolarPanel.transform.GetChild(0).localRotation = Quaternion.identity;

            m_LowSolarPanel.SetActive(false);
            m_MidSolarPanel.SetActive(false);
            m_HighSolarPanel.SetActive(false);

            m_SolarPanelUI.SetActive(false);
        }

        public void SetSolarPower(GameManager.SolarPower power)
        {
            ClearSolarPanels();

            switch (power)
            {
                case GameManager.SolarPower.Low:
                    m_LowSolarPanel.SetActive(true);
                    activeSolarPanel = m_LowSolarPanel.transform;
                    break;
                case GameManager.SolarPower.Mid:
                    m_MidSolarPanel.SetActive(true);
                    activeSolarPanel = m_MidSolarPanel.transform;
                    break;
                case GameManager.SolarPower.High:
                    m_HighSolarPanel.SetActive(true);
                    activeSolarPanel = m_HighSolarPanel.transform;
                    break;
            }
        }

        public void RotateLeft()
        {
            if (!m_SolarPanelUI.activeInHierarchy) return;

            Vector3 currRotation = m_SolarPanelHolder.transform.localRotation.eulerAngles;

            switch (direction)
            {
                case Direction.East:
                    currRotation.y -= 90;
                    direction = Direction.North;
                    break;
                case Direction.North:
                    currRotation.y -= 90;
                    direction = Direction.West;
                    break;
                case Direction.West:
                    currRotation.y -= 90;
                    direction = Direction.South;
                    break;
                case Direction.South:
                    currRotation.y -= 90;
                    direction = Direction.East;
                    break;
            }

            m_SolarPanelHolder.transform.localRotation = Quaternion.Euler(currRotation);
        }

        public void RotateRight()
        {
            if (!m_SolarPanelUI.activeInHierarchy) return;

            Vector3 currRotation = m_SolarPanelHolder.transform.localRotation.eulerAngles;

            switch (direction)
            {
                case Direction.East:
                    currRotation.y += 90;
                    direction = Direction.South;
                    break;
                case Direction.North:
                    currRotation.y += 90;
                    direction = Direction.East;
                    break;
                case Direction.West:
                    currRotation.y += 90;
                    direction = Direction.North;
                    break;
                case Direction.South:
                    currRotation.y += 90;
                    direction = Direction.West;
                    break;
            }

            m_SolarPanelHolder.transform.localRotation = Quaternion.Euler(currRotation);
        }

        public void RotateUp()
        {
            if (!m_SolarPanelUI.activeInHierarchy) return;

            Vector3 currRotation = activeSolarPanel.transform.GetChild(0).localRotation.eulerAngles;
            currRotation.x -= ROTATION_ANGLE;

            if (currRotation.x <= 0) currRotation.x = 0;
            if (currRotation.x >= 60) currRotation.x = 60;

            currentAngle = currRotation.x;

            activeSolarPanel.transform.GetChild(0).localRotation = Quaternion.Euler(currRotation);
        }

        public void RotateDown()
        {
            if (!m_SolarPanelUI.activeInHierarchy) return;

            Vector3 currRotation = activeSolarPanel.transform.GetChild(0).localRotation.eulerAngles;
            currRotation.x += ROTATION_ANGLE;

            if (currRotation.x >= 60) currRotation.x = 60;
            if (currRotation.x <= 0) currRotation.x = 0;

            currentAngle = currRotation.x;

            activeSolarPanel.transform.GetChild(0).localRotation = Quaternion.Euler(currRotation);
        }

        public int GetCurrentRotation()
        {
            int degrees = Mathf.RoundToInt(activeSolarPanel.transform.GetChild(0).eulerAngles.x);

            if (degrees >= 60 && degrees <= 180) degrees = 60;
            else if (degrees <= 0 || degrees > 180) degrees = 0;

            return degrees;
        }

        #endregion

        #region Obstacles

        public void ClearObstacles()
        {
            m_Sun.SetActive(true);

            m_Tree.SetActive(false);
            m_Clouds.SetActive(false);
            m_Wall.SetActive(false);

            obstacles = Obstacles.None;

            PanelManager.Instance.m_CloudPanel.SetActive(false);
        }

        public void SetTree(bool value)
        {
            m_Tree.SetActive(value);
        }

        public void SetClouds(bool value)
        {
            m_Clouds.SetActive(value);
            PanelManager.Instance.m_CloudPanel.SetActive(value);
        }

        public void SetWall(bool value)
        {
            m_Wall.SetActive(value);
        }

        public void SetSun(bool value)
        {
            m_Sun.SetActive(value);
        }

        #endregion

        #region Shades

        public void SetShades(int index, bool value)
        {
            if (index == 0 && value)
            {
                obstacles = Obstacles.None;
                ClearObstacles();
            }
            else if (index == 1)
            {
                obstacles = Obstacles.Clouds;
                SetClouds(value);
            }
            else if (index == 2)
            {
                obstacles = Obstacles.Tree;
                SetTree(value);
            }
            else if (index == 3)
            {
                obstacles = Obstacles.Wall;
                SetWall(value);
            }
        }

        #endregion
    }
}
