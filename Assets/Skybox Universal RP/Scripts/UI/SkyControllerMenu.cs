/*
 * ---------------------------------------------------------------------------
 * Description: This script manages the UI for controlling various settings of the 
 *              SkyController component in Unity. It allows users to adjust time, time progression, 
 *              reverse time options, and day length through sliders, input fields, and dropdowns. 
 *              It also handles input validation and updates the SkyController settings based on 
 *              user input, providing a visual representation of the current time.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Skybox URP/UI/Sky Controller Menu")]
public class SkyControllerMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private SkyController skyController; // SkyController component that will be changed.
    [Space(10)]
    [SerializeField] private Slider time; // Slider to control time.
    [SerializeField] private Text hour; // Text that displays the time.
    [SerializeField] private Dropdown timeGoes; // Dropdown to select time direction.
    [Space(5)]
    [SerializeField] private Toggle reverseTime; // Toggle to invert time.
    [Space(5)]
    [SerializeField] private InputField timeSpeed; // Input field for the speed of time.
    [SerializeField] private InputField timeOfDay; // Input field for day length.

    private void OnEnable()
    {
        // Sets the initial values of UI elements based on SkyController settings.
        time.value = skyController.time;
        timeGoes.value = (int)skyController.timeGoes;
        reverseTime.isOn = skyController.reverseTime;
        timeSpeed.text = skyController.TimeSpeed.ToString();
        timeOfDay.text = skyController.TimeOfDay.ToString();

        // Adds listeners for change events on UI elements.
        time.onValueChanged.AddListener(UpdateTime);
        timeGoes.onValueChanged.AddListener(UpdateTimeGoes);
        reverseTime.onValueChanged.AddListener(UpdateReverseTime);
        timeSpeed.onValueChanged.AddListener(UpdateTimeSpeed);
        timeOfDay.onValueChanged.AddListener(UpdateTimeOfDay);

        UpdateTimeGoesInteractable(skyController.timeGoes); // Configures the interaction of UI elements based on SkyController settings.

        // Configures the input and validation types for the time InputFields.
        timeSpeed.contentType = InputField.ContentType.DecimalNumber;
        timeSpeed.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
        timeSpeed.onValidateInput += ValidateNumericInput;
        timeOfDay.contentType = InputField.ContentType.DecimalNumber;
        timeOfDay.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
        timeOfDay.onValidateInput += ValidateNumericInput;

        UpdatePercentage(skyController.time); // Updates the percentage text based on the current time value.
    }

    private void OnDisable()
    {
        // Removes listeners for change events on UI elements.
        time.onValueChanged.RemoveListener(UpdateTime);
        timeGoes.onValueChanged.RemoveListener(UpdateTimeGoes);
        reverseTime.onValueChanged.RemoveListener(UpdateReverseTime);
        timeSpeed.onValueChanged.RemoveListener(UpdateTimeSpeed);
        timeOfDay.onValueChanged.RemoveListener(UpdateTimeOfDay);

        // Removes input validation on time InputFields.
        timeSpeed.onValidateInput -= ValidateNumericInput;
        timeOfDay.onValidateInput -= ValidateNumericInput;
    }

    private void UpdateTime(float value)
    {
        skyController.time = value; // Updates the time value in SkyController.
        UpdatePercentage(value); // Updates the displayed percentage.
    }

    private void UpdatePercentage(float value)
    {
        hour.text = $"H: {Mathf.RoundToInt(value)}"; // Updates the displayed time text.
    }

    private void UpdateTimeGoes(int value)
    {
        skyController.timeGoes = (SkyController.TimeGoes)value; // Updates the time progression mode in SkyController.
        UpdateTimeGoesInteractable((SkyController.TimeGoes)value); // Updates the interaction of UI elements based on progression mode.
    }

    private void UpdateReverseTime(bool value)
    {
        skyController.reverseTime = value; // Updates the reverse weather option in SkyController.
    }

    private void UpdateTimeSpeed(string value)
    {
        if (float.TryParse(value, out float speed))
        {
            skyController.TimeSpeed = speed; // Updates time speed in SkyController.
        }
    }

    private void UpdateTimeOfDay(string value)
    {
        if (float.TryParse(value, out float dayLength))
        {
            skyController.TimeOfDay = dayLength; // Updates the day length in SkyController.
        }
    }

    private void UpdateTimeGoesInteractable(SkyController.TimeGoes timeGoesValue)
    {
        bool enableTimeSpeed = timeGoesValue == SkyController.TimeGoes.TimeBySpeed;
        bool enableTimeOfDay = timeGoesValue == SkyController.TimeGoes.TimeByTime;

        // Configures the interaction of UI elements based on progression mode.
        timeSpeed.interactable = enableTimeSpeed;
        timeOfDay.interactable = enableTimeOfDay;
    }

    private char ValidateNumericInput(string text, int charIndex, char addedChar)
    {
        // Checks whether the added character is a numeric digit or a decimal point.
        if (char.IsDigit(addedChar) || addedChar == '.')
        {
            // Checks if the added character is a decimal point and if a dot already exists in the text.
            if (addedChar == '.' && text.IndexOf('.') != -1)
            {
                return '\0'; // If a dot already exists, returns a null character to prevent its addition.
            }
            else
            {
                return addedChar; // If it is a valid numeric digit or dot, returns the added character.
            }
        }
        else
        {
            return '\0'; // If the character is not a numeric digit or a valid period, it returns a null character to prevent its addition.
        }
    }
}