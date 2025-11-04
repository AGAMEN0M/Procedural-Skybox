/*
 * ---------------------------------------------------------------------------
 * Description: This script manages the UI for controlling various settings of the 
 *              SkyController component in Unity. It allows users to adjust time, time 
 *              progression, reverse time options, and day length through sliders, 
 *              input fields, and dropdowns. It also handles input validation and 
 *              updates the SkyController settings based on user input, providing 
 *              a visual representation of the current time.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.UI;
using UnityEngine;

using static UnityEngine.UI.InputField;
using static SkyController;

/// <summary>
/// Provides UI control for adjusting time and day cycle parameters of the SkyController component.
/// </summary>
[AddComponentMenu("Skybox URP/UI/Sky Controller Menu [Legacy]")]
public class SkyControllerMenu : MonoBehaviour
{
    #region === Fields ===

    [Header("Sky Controller Reference")]
    [SerializeField, Tooltip("Reference to the SkyController component that will be modified.")]
    private SkyController skyController; // Reference to the SkyController component.

    [Header("UI Elements")]
    [SerializeField, Tooltip("Slider for controlling the current time value.")]
    private Slider timeSlider; // Slider for time control.

    [SerializeField, Tooltip("Text element that displays the current time value.")]
    private Text timeLabel; // Text displaying the time.

    [SerializeField, Tooltip("Dropdown to select the time progression mode.")]
    private Dropdown timeProgressionDropdown; // Dropdown for time progression mode.

    [Space(5)]
    [SerializeField, Tooltip("Toggle to enable or disable reverse time progression.")]
    private Toggle reverseTimeToggle; // Toggle for reverse time.

    [Space(5)]
    [SerializeField, Tooltip("Input field to define time speed when using the speed-based progression mode.")]
    private InputField timeSpeedInput; // Input for time speed.

    [SerializeField, Tooltip("Input field to define the length of the day when using time-based progression.")]
    private InputField dayLengthInput; // Input for day length.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the SkyController reference controlled by this menu.
    /// </summary>
    public SkyController SkyControllerReference
    {
        get => skyController;
        set => skyController = value;
    }

    /// <summary>
    /// Gets or sets the time slider used to adjust the current time value.
    /// </summary>
    public Slider TimeSlider
    {
        get => timeSlider;
        set => timeSlider = value;
    }

    /// <summary>
    /// Gets or sets the text element that displays the current time.
    /// </summary>
    public Text TimeLabel
    {
        get => timeLabel;
        set => timeLabel = value;
    }

    /// <summary>
    /// Gets or sets the dropdown for selecting the time progression mode.
    /// </summary>
    public Dropdown TimeProgressionDropdown
    {
        get => timeProgressionDropdown;
        set => timeProgressionDropdown = value;
    }

    /// <summary>
    /// Gets or sets the toggle that enables or disables reverse time progression.
    /// </summary>
    public Toggle ReverseTimeToggle
    {
        get => reverseTimeToggle;
        set => reverseTimeToggle = value;
    }

    /// <summary>
    /// Gets or sets the input field that defines the time speed value.
    /// </summary>
    public InputField TimeSpeedInput
    {
        get => timeSpeedInput;
        set => timeSpeedInput = value;
    }

    /// <summary>
    /// Gets or sets the input field that defines the length of a day.
    /// </summary>
    public InputField DayLengthInput
    {
        get => dayLengthInput;
        set => dayLengthInput = value;
    }

    #endregion

    #region === Unity Lifecycle ===

    private void OnEnable()
    {
        // Initialize UI elements with values from the SkyController.
        timeSlider.value = skyController.CurrentTime;
        timeProgressionDropdown.value = (int)skyController.CurrentTimeMode;
        reverseTimeToggle.isOn = skyController.IsReversedTime;
        timeSpeedInput.text = skyController.TimeMultiplier.ToString();
        dayLengthInput.text = skyController.DayDuration.ToString();

        // Subscribe to UI event listeners.
        timeSlider.onValueChanged.AddListener(OnTimeChanged);
        timeProgressionDropdown.onValueChanged.AddListener(OnTimeProgressionChanged);
        reverseTimeToggle.onValueChanged.AddListener(OnReverseTimeChanged);
        timeSpeedInput.onValueChanged.AddListener(OnTimeSpeedChanged);
        dayLengthInput.onValueChanged.AddListener(OnDayLengthChanged);

        // Set up input field validation for numeric values.
        ConfigureInputField(timeSpeedInput);
        ConfigureInputField(dayLengthInput);

        // Update UI interactability and display.
        UpdateTimeProgressionInteractable(skyController.CurrentTimeMode);
        UpdateDisplayedTime(skyController.CurrentTime);
    }

    private void OnDisable()
    {
        // Unsubscribe from UI event listeners.
        timeSlider.onValueChanged.RemoveListener(OnTimeChanged);
        timeProgressionDropdown.onValueChanged.RemoveListener(OnTimeProgressionChanged);
        reverseTimeToggle.onValueChanged.RemoveListener(OnReverseTimeChanged);
        timeSpeedInput.onValueChanged.RemoveListener(OnTimeSpeedChanged);
        dayLengthInput.onValueChanged.RemoveListener(OnDayLengthChanged);

        // Remove input validation.
        timeSpeedInput.onValidateInput -= ValidateNumericInput;
        dayLengthInput.onValidateInput -= ValidateNumericInput;
    }

    #endregion

    #region === Private Methods ===

    /// <summary>
    /// Configures the provided InputField for numeric input only.
    /// </summary>
    private void ConfigureInputField(InputField field)
    {
        field.contentType = ContentType.DecimalNumber;
        field.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
        field.onValidateInput += ValidateNumericInput;
    }

    /// <summary>
    /// Called when the time slider value changes.
    /// </summary>
    private void OnTimeChanged(float value)
    {
        skyController.CurrentTime = value; // Update the time value in the controller.
        UpdateDisplayedTime(value); // Refresh the displayed time label.
    }

    /// <summary>
    /// Updates the displayed time label based on the current time value.
    /// </summary>
    private void UpdateDisplayedTime(float value)
    {
        timeLabel.text = $"H: {Mathf.RoundToInt(value)}"; // Update the label with the rounded hour value.
    }

    /// <summary>
    /// Called when the time progression mode changes via the dropdown.
    /// </summary>
    private void OnTimeProgressionChanged(int value)
    {
        skyController.CurrentTimeMode = (TimeMode)value; // Update time progression mode.
        UpdateTimeProgressionInteractable((TimeMode)value); // Update UI interactivity.
    }

    /// <summary>
    /// Called when the reverse time toggle changes.
    /// </summary>
    private void OnReverseTimeChanged(bool value)
    {
        skyController.IsReversedTime = value; // Update reverse time setting.
    }

    /// <summary>
    /// Called when the time speed input field changes.
    /// </summary>
    private void OnTimeSpeedChanged(string value)
    {
        if (float.TryParse(value, out float speed))
            skyController.TimeMultiplier = speed; // Update the time speed in SkyController.
    }

    /// <summary>
    /// Called when the day length input field changes.
    /// </summary>
    private void OnDayLengthChanged(string value)
    {
        if (float.TryParse(value, out float dayLength))
            skyController.DayDuration = dayLength; // Update the day length in SkyController.
    }

    /// <summary>
    /// Updates the interactability of UI elements based on the selected time progression mode.
    /// </summary>
    private void UpdateTimeProgressionInteractable(TimeMode mode)
    {
        timeSpeedInput.interactable = mode == TimeMode.BySpeed;
        dayLengthInput.interactable = mode == TimeMode.ByElapsedTime;
    }

    /// <summary>
    /// Validates numeric input for input fields, allowing digits and a single decimal point.
    /// </summary>
    private char ValidateNumericInput(string text, int charIndex, char addedChar)
    {
        if (char.IsDigit(addedChar) || addedChar == '.')
        {
            if (addedChar == '.' && text.Contains("."))
            {
                return '\0'; // Prevent multiple decimal points.
            }
            return addedChar; // Accept valid characters.
        }
        return '\0'; // Reject invalid characters.
    }

    #endregion
}