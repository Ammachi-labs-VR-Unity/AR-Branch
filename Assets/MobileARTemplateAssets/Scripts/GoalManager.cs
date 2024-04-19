using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Onboarding goal to be achieved as part of the <see cref="GoalManager"/>.
/// </summary>
public struct Goal
{
    /// <summary>
    /// Goal state this goal represents.
    /// </summary>
    public GoalManager.OnboardingGoals CurrentGoal;

    /// <summary>
    /// This denotes whether a goal has been completed.
    /// </summary>
    public bool Completed;

    /// <summary>
    /// Creates a new Goal with the specified <see cref="GoalManager.OnboardingGoals"/>.
    /// </summary>
    /// <param name="goal">The <see cref="GoalManager.OnboardingGoals"/> state to assign to this Goal.</param>
    public Goal(GoalManager.OnboardingGoals goal)
    {
        CurrentGoal = goal;
        Completed = false;
    }
}

/// <summary>
/// The GoalManager cycles through a list of Goals, each representing
/// an <see cref="GoalManager.OnboardingGoals"/> state to be completed by the user.
/// </summary>
public class GoalManager : MonoBehaviour
{
    /// <summary>
    /// State representation for the onboarding goals for the GoalManager.
    /// </summary>
    public enum OnboardingGoals
    {
        /// <summary>
        /// Current empty scene
        /// </summary>
        Empty,

        /// <summary>
        /// Find/scan for AR surfaces
        /// </summary>
        FindSurfaces,

        /// <summary>
        /// Tap a surface to spawn an object
        /// </summary>
        TapSurface,
    }

    /// <summary>
    /// Individual step instructions to show as part of a goal.
    /// </summary>
    [Serializable]
    public class Step
    {
        /// <summary>
        /// The GameObject to enable and show the user in order to complete the goal.
        /// </summary>
        [SerializeField]
        public GameObject stepObject;

        /// <summary>
        /// The text to display on the button shown in the step instructions.
        /// </summary>
        [SerializeField]
        public string buttonText;

        /// <summary>
        /// This indicates whether to show an additional button to skip the current goal/step.
        /// </summary>
        [SerializeField]
        public bool includeSkipButton;
    }

    [Tooltip("List of Goals/Steps to complete as part of the user onboarding.")]
    [SerializeField]
    private List<Step> m_StepList = new List<Step>();

    /// <summary>
    /// List of Goals/Steps to complete as part of the user onboarding.
    /// </summary>
    public List<Step> stepList
    {
        get => m_StepList;
        set => m_StepList = value;
    }

    [Tooltip("Object Spawner used to detect whether the spawning goal has been achieved.")]
    [SerializeField]
    private ObjectSpawner m_ObjectSpawner;

    /// <summary>
    /// Object Spawner used to detect whether the spawning goal has been achieved.
    /// </summary>
    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [Tooltip("The AR Template Menu Manager object to enable once the greeting prompt is dismissed.")]
    [SerializeField]
    private ARTemplateMenuManager m_MenuManager;

    /// <summary>
    /// The AR Template Menu Manager object to enable once the greeting prompt is dismissed.
    /// </summary>
    public ARTemplateMenuManager menuManager
    {
        get => m_MenuManager;
        set => m_MenuManager = value;
    }

    private const int k_NumberOfSurfacesTappedToCompleteGoal = 1;
    private Queue<Goal> m_OnboardingGoals;
    private Coroutine m_CurrentCoroutine;
    private Goal m_CurrentGoal;
    private bool m_AllGoalsFinished;
    private int m_SurfacesTapped;
    private int m_CurrentGoalIndex = 0;

    private void Start()
    {
        StartCoaching();
    }

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame && !m_AllGoalsFinished && m_CurrentGoal.CurrentGoal == OnboardingGoals.FindSurfaces)
        {
            if (m_CurrentCoroutine != null)
            {
                StopCoroutine(m_CurrentCoroutine);
            }

            CompleteGoal();
        }
    }

    private void CompleteGoal()
    {
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
            m_ObjectSpawner.objectSpawned -= OnObjectSpawned;

        m_CurrentGoal.Completed = true;
        m_CurrentGoalIndex++;
        if (m_OnboardingGoals.Count > 0)
        {
            m_CurrentGoal = m_OnboardingGoals.Dequeue();
            m_StepList[m_CurrentGoalIndex - 1].stepObject?.SetActive(false);
            m_StepList[m_CurrentGoalIndex].stepObject.SetActive(true);
        }
        else
        {
            m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);
            m_AllGoalsFinished = true;
            return;
        }

        PreprocessGoal();
        StartCoroutine(CheckForSpawn());
    }

    IEnumerator CheckForSpawn()
    {
        if (!m_ObjectSpawner.initialObjectSpawned)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(CheckForSpawn());
        }
        else
        {
            PanelManager.Instance.OpenNextPanel();
        }
    }

    private void PreprocessGoal()
    {
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.FindSurfaces)
        {
            m_CurrentCoroutine = StartCoroutine(WaitUntilNextCard(5f));
        }
        else if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
        {
            m_SurfacesTapped = 0;
            m_ObjectSpawner.objectSpawned += OnObjectSpawned;
        }
    }

    /// <summary>
    /// Tells the Goal Manager to wait for a specific number of seconds before completing
    /// the goal and showing the next card.
    /// </summary>
    /// <param name="seconds">The number of seconds to wait before showing the card.</param>
    /// <returns>Returns an IEnumerator for the current coroutine running.</returns>
    public IEnumerator WaitUntilNextCard(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!Pointer.current.press.wasPressedThisFrame)
        {
            m_CurrentCoroutine = null;
            CompleteGoal();
        }
    }

    /// <summary>
    /// Forces the completion of the current goal and moves to the next.
    /// </summary>
    public void ForceCompleteGoal()
    {
        CompleteGoal();
    }

    private void OnObjectSpawned(GameObject spawnedObject)
    {
        m_SurfacesTapped++;
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface && m_SurfacesTapped >= k_NumberOfSurfacesTappedToCompleteGoal)
        {
            CompleteGoal();
        }
    }

    /// <summary>
    /// Triggers a restart of the onboarding/coaching process.
    /// </summary>
    public void StartCoaching()
    {
        if (m_OnboardingGoals != null)
        {
            m_OnboardingGoals.Clear();
        }

        m_OnboardingGoals = new Queue<Goal>();

        if (!m_AllGoalsFinished)
        {
            var findSurfaceGoal = new Goal(OnboardingGoals.FindSurfaces);
            m_OnboardingGoals.Enqueue(findSurfaceGoal);
        }

        int startingStep = m_AllGoalsFinished ? 1 : 0;

        var tapSurfaceGoal = new Goal(OnboardingGoals.TapSurface);

        m_OnboardingGoals.Enqueue(tapSurfaceGoal);

        m_CurrentGoal = m_OnboardingGoals.Dequeue();
        m_AllGoalsFinished = false;
        m_CurrentGoalIndex = startingStep;

        m_MenuManager.enabled = true;

        for (int i = startingStep; i < m_StepList.Count; i++)
        {
            if (i == startingStep)
            {
                m_StepList[i].stepObject.SetActive(true);
                PreprocessGoal();
            }
            else
            {
                m_StepList[i].stepObject.SetActive(false);
            }
        }
    }
}
