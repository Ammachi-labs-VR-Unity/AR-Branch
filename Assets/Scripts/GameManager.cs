using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SolarModule
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance = null;

        public enum StateRegion
        {
            Red = 0, Orange = 1, Yellow = 2, Green = 3
        }

        [HideInInspector]
        public StateRegion stateRegion;

        public enum SolarPower
        {
            Low = 0, Mid = 1, High = 2
        }

        [HideInInspector]
        public SolarPower solarPower;

        private List<MultimeterData> multimeterData = new();
        private List<StateData> stateData = new();

        [HideInInspector]
        public MultimeterData m_CurrentMultimeter;
        [HideInInspector]
        public StateData m_CurrentState;

        public ToggleGroup m_StateSelection, m_SolarPanelSelection;

        public Image[] m_StateOptions;

        public GameObject m_SolarDescription100W, m_SolarDescription150W, m_SolarDescription330W;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            // State Data

            stateData.Add(new()
            {
                stateRegion = StateRegion.Red,
                optimalAngleMin = 30,
                optimalAngleMax = 35
            });

            stateData.Add(new()
            {
                stateRegion = StateRegion.Orange,
                optimalAngleMin = 25,
                optimalAngleMax = 30
            });

            stateData.Add(new()
            {
                stateRegion = StateRegion.Yellow,
                optimalAngleMin = 20,
                optimalAngleMax = 25
            });

            stateData.Add(new()
            {
                stateRegion = StateRegion.Green,
                optimalAngleMin = 15,
                optimalAngleMax = 15
            });

            // Solar Power

            multimeterData.Add(new()
            {
                solarPower = SolarPower.Low,
                maximumReading = 5.5f,
                readingDecreaseRate = 0.6f
            });

            multimeterData.Add(new()
            {
                solarPower = SolarPower.Mid,
                maximumReading = 8.3f,
                readingDecreaseRate = 0.8f
            });

            multimeterData.Add(new()
            {
                solarPower = SolarPower.High,
                maximumReading = 13.5f,
                readingDecreaseRate = 1.1f
            });
        }

        public void StateUpdated()
        {
            foreach (var item in m_StateOptions)
            {
                item.enabled = false;
            }

            var activeToggle = m_StateSelection.GetFirstActiveToggle().name;
            if (activeToggle.ToLower().StartsWith("red"))
            {
                stateRegion = StateRegion.Red;
                m_StateOptions[0].enabled = true;
                m_StateOptions[1].enabled = true;

                m_CurrentState = stateData[0];
            }
            else if (activeToggle.ToLower().StartsWith("orange"))
            {
                stateRegion = StateRegion.Orange;
                m_StateOptions[2].enabled = true;
                m_StateOptions[3].enabled = true;

                m_CurrentState = stateData[1];
            }
            else if (activeToggle.ToLower().StartsWith("yellow"))
            {
                stateRegion = StateRegion.Yellow;
                m_StateOptions[4].enabled = true;
                m_StateOptions[5].enabled = true;

                m_CurrentState = stateData[2];
            }
            else if (activeToggle.ToLower().StartsWith("green"))
            {
                stateRegion = StateRegion.Green;
                m_StateOptions[6].enabled = true;
                m_StateOptions[7].enabled = true;

                m_CurrentState = stateData[3];
            }

            Debug.Log("State Region - " + stateRegion.ToString());
        }

        public void SolarPowerUpdated()
        {
            try
            {
                var activeToggle = m_SolarPanelSelection.GetFirstActiveToggle().name;

                m_SolarDescription100W.SetActive(false);
                m_SolarDescription150W.SetActive(false);
                m_SolarDescription330W.SetActive(false);

                if (activeToggle.ToLower().Contains("left"))
                {
                    solarPower = SolarPower.Low;
                    m_CurrentMultimeter = multimeterData[0];
                    m_SolarDescription100W.SetActive(true);
                }
                else if (activeToggle.ToLower().Contains("center"))
                {
                    solarPower = SolarPower.Mid;
                    m_CurrentMultimeter = multimeterData[1];
                    m_SolarDescription150W.SetActive(true);
                }
                else if (activeToggle.ToLower().Contains("right"))
                {
                    solarPower = SolarPower.High;
                    m_CurrentMultimeter = multimeterData[2];
                    m_SolarDescription330W.SetActive(true);
                }
                else
                {
                    solarPower = SolarPower.Low;
                    m_CurrentMultimeter = multimeterData[0];
                    m_SolarDescription100W.SetActive(true);
                }

                ModelManager.Instance.SetSolarPower(solarPower);

                Debug.Log("Solar Panel - " + solarPower.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("Null Handling - " + e.ToString());
            }
        }
    }

    [Serializable]
    public class MultimeterData
    {
        public GameManager.SolarPower solarPower;

        public float maximumReading;
        public float readingDecreaseRate;
    }

    [Serializable]
    public class StateData
    {
        public GameManager.StateRegion stateRegion;

        public int optimalAngleMin;
        public int optimalAngleMax;
    }
}