using UnityEngine;
using UnityEngine.UI;

namespace Flyfe.UI
{
    public class MeterScript : MonoBehaviour
    {
        public Slider slider;
        public Gradient gradient;
        public Image fill;

        public void SetMaxTime(float time)
        {
            if (slider == null) return;
            slider.maxValue = time;
            slider.value = time;

            if (fill != null) fill.color = gradient.Evaluate(1f);
        }

        public void SetTime(float time)
        {
            if (slider == null) return;
            slider.value = time;
            if (fill != null) fill.color = gradient.Evaluate(slider.normalizedValue);
        }

        public void SetValue(float normalizedValue)
        {
            if (slider == null) return;
            slider.value = normalizedValue * slider.maxValue;
            if (fill != null) fill.color = gradient.Evaluate(normalizedValue);
        }
    }
}
