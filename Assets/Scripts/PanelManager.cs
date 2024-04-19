using System.Collections.Generic;
using System.Linq;
using SolarModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance = null;

    public ARPlaneManager planeManager;

    public GameObject[] m_UIPanels;
    public GameObject m_MultiMeterPanel;
    public GameObject m_RightDegreesPanel;
    public GameObject m_WrongDegreesPanel;
    public GameObject m_HintPanel;
    public GameObject m_NightPanel;
    public GameObject m_DayButton;
    public GameObject m_NightButton;
    public GameObject m_CloudPanel;
    public GameObject m_ShadeInfo;
    public GameObject m_ShadeOnButton;
    public GameObject m_ShadeOffButton;
    public GameObject m_ShadeContinue;

    public TMP_Text m_MultiMeterText;
    public TMP_Text m_DegreeText;

    private int m_CurrentPanelIndex = -1;
    private int m_NumberOfSubmits = 0;

    public Toggle[] m_ShadeToggles;

    public Material m_PlaneMaterial;

    private Dictionary<int, bool> m_ShadeActivated = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ResetData();

        OpenNextPanel();

        // Set plane visible
        if (m_PlaneMaterial.HasProperty("_TexTintColor"))
        {
            // Set the color of the property
            m_PlaneMaterial.SetColor("_TexTintColor", new(1, 1, 1, 1));
        }
        else
        {
            Debug.LogWarning("Material does not have property: _TexTintColor");
        }
    }

    private void ResetData()
    {
        m_CurrentPanelIndex = -1;

        m_DayButton.SetActive(true);
        m_NightButton.SetActive(false);
        m_NightPanel.SetActive(false);
        m_CloudPanel.SetActive(false);
        m_HintPanel.SetActive(false);
        m_HintPanel.transform.GetChild(1).gameObject.SetActive(false);

        m_NumberOfSubmits = 0;

        if (ModelManager.Instance != null)
        {
            ModelManager.Instance.ClearSolarPanels();
            ModelManager.Instance.ClearObstacles();
        }

        m_ShadeActivated.Clear();
        m_ShadeActivated.Add(0, false);
        m_ShadeActivated.Add(1, false);
        m_ShadeActivated.Add(2, false);
        m_ShadeActivated.Add(3, false);

        m_ShadeInfo.SetActive(false);
        m_ShadeOnButton.SetActive(false);
        m_ShadeOffButton.SetActive(true);
        m_ShadeContinue.SetActive(false);
    }

    private void Update()
    {
        UpdateSolarPanelDegrees();
        UpdateMultiMeterReading();
    }

    private void ClosePanels()
    {
        foreach (var panel in m_UIPanels)
        {
            panel.SetActive(false);
        }
    }

    public void OpenNextPanel()
    {
        ClosePanels();

        if ((m_CurrentPanelIndex + 1) < m_UIPanels.Count())
        {
            m_UIPanels[m_CurrentPanelIndex + 1].SetActive(true);
            m_CurrentPanelIndex += 1;
        }
    }

    public void OpenPreviousPanel()
    {
        ClosePanels();

        if ((m_CurrentPanelIndex - 1) > 0)
        {
            m_UIPanels[m_CurrentPanelIndex - 1].SetActive(true);
            m_CurrentPanelIndex -= 1;
        }
    }

    public void OpenSolarPanelSelectorPanel()
    {
        ResetData();

        ClosePanels();

        m_MultiMeterPanel.SetActive(false);

        int solarPanelIndex = 4;

        if (solarPanelIndex < m_UIPanels.Count())
        {
            m_UIPanels[solarPanelIndex].SetActive(true);
            m_CurrentPanelIndex = solarPanelIndex;
        }

        GameManager.Instance.m_SolarPanelSelection.GetFirstActiveToggle().isOn = false;
    }

    public void OpenStateSelectorPanel()
    {
        ResetData();

        ClosePanels();

        m_MultiMeterPanel.SetActive(false);

        int statePanelIndex = 2;

        if (statePanelIndex < m_UIPanels.Count())
        {
            m_UIPanels[statePanelIndex].SetActive(true);
            m_CurrentPanelIndex = statePanelIndex;
        }
    }

    public void StateChosen()
    {
        if (ModelManager.Instance == null)
        {
            OpenNextPanel();
            planeManager.enabled = true;
        }
        else
        {
            OpenNextPanel();
            OpenNextPanel();
            planeManager.enabled = false;
            GameManager.Instance.m_SolarPanelSelection.GetFirstActiveToggle().isOn = false;
        }
    }

    public void OnExitApplication()
    {
        Application.Quit();
    }

    public void SetShade(int value)
    {
        bool isShadeOn = m_ShadeToggles[value].isOn;
        ModelManager.Instance.SetShades(value, isShadeOn);

        if (isShadeOn)
        {
            m_ShadeActivated[value] = true;
        }

        CheckShadesActivation();
    }

    public void CheckShadesActivation()
    {
        if (m_ShadeActivated[1] && m_ShadeActivated[2] && m_ShadeActivated[3])
        {
            if (m_ShadeOnButton.activeInHierarchy)
            {
                m_ShadeInfo.SetActive(true);
            }
            else
            {
                m_ShadeContinue.SetActive(true);
            }
        }
    }

    public void EnableRotation(bool value)
    {
        ModelManager.Instance.m_SolarPanelUI.SetActive(value);
        ModelManager.Instance.m_SolarPanelUI.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void UpdateSolarPanelDegrees()
    {
        if (m_DegreeText.gameObject.activeInHierarchy)
        {
            m_DegreeText.text = ModelManager.Instance.GetCurrentRotation() + " °";
        }
    }

    public void ValidateDegrees()
    {
        m_NumberOfSubmits++;

        bool isCorrect = (ModelManager.Instance.GetCurrentRotation() == GameManager.Instance.m_CurrentState.optimalAngleMin || ModelManager.Instance.GetCurrentRotation() == GameManager.Instance.m_CurrentState.optimalAngleMax) && ModelManager.Instance.direction == ModelManager.Direction.South;

        m_RightDegreesPanel.SetActive(isCorrect);
        m_WrongDegreesPanel.SetActive(!isCorrect);
        ModelManager.Instance.m_SolarPanelUI.SetActive(!isCorrect);
    }

    public void CheckHintPanel()
    {
        if (m_NumberOfSubmits >= 3)
        {
            m_HintPanel.SetActive(true);
            m_NumberOfSubmits = 0;
        }
    }

    public void ResetHintPanel()
    {
        m_NumberOfSubmits = 0;
        m_HintPanel.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void ToggleHintInfo()
    {
        m_HintPanel.transform.GetChild(1).gameObject.SetActive(!m_HintPanel.transform.GetChild(1).gameObject.activeInHierarchy);
    }

    public void UpdateMultiMeterReading()
    {
        if (m_MultiMeterPanel.activeInHierarchy)
        {
            switch (GameManager.Instance.solarPower)
            {
                case GameManager.SolarPower.Low:
                    m_MultiMeterText.text = GetCurrent(GameManager.Instance.m_CurrentState.optimalAngleMin, GameManager.Instance.m_CurrentState.optimalAngleMax, GameManager.Instance.m_CurrentMultimeter.maximumReading, GameManager.Instance.m_CurrentMultimeter.readingDecreaseRate, GetObstacleDecrease(true));
                    break;
                case GameManager.SolarPower.Mid:
                    m_MultiMeterText.text = GetCurrent(GameManager.Instance.m_CurrentState.optimalAngleMin, GameManager.Instance.m_CurrentState.optimalAngleMax, GameManager.Instance.m_CurrentMultimeter.maximumReading, GameManager.Instance.m_CurrentMultimeter.readingDecreaseRate, GetObstacleDecrease(false));
                    break;
                case GameManager.SolarPower.High:
                    m_MultiMeterText.text = GetCurrent(GameManager.Instance.m_CurrentState.optimalAngleMin, GameManager.Instance.m_CurrentState.optimalAngleMax, GameManager.Instance.m_CurrentMultimeter.maximumReading, GameManager.Instance.m_CurrentMultimeter.readingDecreaseRate, GetObstacleDecrease(false));
                    break;
            }
        }
    }

    private float GetObstacleDecrease(bool is100w)
    {
        if (is100w)
        {
            return ModelManager.Instance.obstacles switch
            {
                ModelManager.Obstacles.None => 0f,
                ModelManager.Obstacles.Clouds => ModelManager.CLOUD_100W,
                ModelManager.Obstacles.Tree => ModelManager.OTHER_100W,
                ModelManager.Obstacles.Wall => ModelManager.OTHER_100W,
                _ => 0f,
            };
        }
        else
        {
            return ModelManager.Instance.obstacles switch
            {
                ModelManager.Obstacles.None => 0f,
                ModelManager.Obstacles.Clouds => ModelManager.CLOUD_150W,
                ModelManager.Obstacles.Tree => ModelManager.OTHER_150W,
                ModelManager.Obstacles.Wall => ModelManager.OTHER_150W,
                _ => 0f,
            };
        }
    }

    private int GetDirectionDecrease()
    {
        switch (ModelManager.Instance.direction)
        {
            case ModelManager.Direction.East:
                return 1;
            case ModelManager.Direction.North:
                return 2;
            case ModelManager.Direction.West:
                return 1;
            case ModelManager.Direction.South:
                return 0;
            default:
                return 0;
        }
    }

    private string GetCurrent(float minAngle, float maxAngle, float maxCurrent, float readingDecreaseRate, float obstacleDecrease)
    {
        float reading = maxCurrent;

        if (ModelManager.Instance.currentAngle > maxAngle)
        {
            float difference = (ModelManager.Instance.currentAngle - maxAngle) / ModelManager.ROTATION_ANGLE;
            reading -= readingDecreaseRate * difference;
        }
        else if (ModelManager.Instance.currentAngle < minAngle)
        {
            float difference = (minAngle - ModelManager.Instance.currentAngle) / ModelManager.ROTATION_ANGLE;
            reading -= readingDecreaseRate * difference;
        }

        reading -= readingDecreaseRate * GetDirectionDecrease();
        reading -= obstacleDecrease;

        if (m_NightPanel.transform.parent.gameObject.activeInHierarchy && m_NightButton.activeInHierarchy)
        {
            reading = readingDecreaseRate * 2;
        }

        if (reading <= 0f)
        {
            reading = 0.3f;
        }

        return reading.ToString("F1");
    }

    public void SetSun(bool enable)
    {
        ModelManager.Instance.SetSun(enable);
    }

    public void RotateUp()
    {
        ModelManager.Instance.RotateUp();
    }

    public void RotateDown()
    {
        ModelManager.Instance.RotateDown();
    }

    public void Rotate360()
    {
        ModelManager.Instance.RotateRight();
    }
}
