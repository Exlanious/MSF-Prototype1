using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIMOdeToggleButtonBehaviour : MonoBehaviour
{
    public LLMProviders provider;
    public Button button;
    public TextMeshProUGUI buttonText;

    void Start()
    {
        // Get the initial provider from LLMCaller and set the button text
        provider = LLMCaller.Instance.getLLMProvider();
        buttonText.text = provider.ToString();
    }

    public void clickButton()
    {
        // Toggle the provider
        provider++;
        if ((int)provider >= System.Enum.GetValues(typeof(LLMProviders)).Length)
        {
            provider = 0;
        }

        // Call method on LLMCaller with the new provider
        LLMCaller.Instance.switchLLMService(provider);
        buttonText.text = provider.ToString();
    }
}