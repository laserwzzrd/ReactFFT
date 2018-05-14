using UnityEngine;
using System.Collections;

public class ReactFFT : MonoBehaviour 
{
	public float bassFFTAmount = 0.0f;
	public string colorName = "_TintColor";

	public enum ReactFFTSpectrum
	{
		BASS,
		MID,
		TREBLE,
		SUM
	};

	public enum ReactFFTType
	{
		NONE,
		COLOR_ADD,
		COLOR_MOD,
		UISPRITE_FILL,
		LIGHT_INTENSITY_ADD,
		SCALE_ADD,
		SCALE_MOD
	};

	public BassManager bassM;
	public ReactFFTSpectrum spectrumType = ReactFFTSpectrum.SUM;
	public ReactFFTType type = ReactFFTType.NONE;
	public Material fftMaterial;
	public Color originalColor = Color.white;
	public float colorMod = 1.0f;
	public bool sharedMaterial = false;
	public int materialIndex = -1;
	public float intensityScale = 1.0f;
	public float originalIntensity = 1.0f;
	public Light fftLight;
	public Vector3 scaleValue = Vector3.one;
	public Vector3 originalScale = Vector3.one;
	public bool lerpFFT = false;
	public float lerpFFTValue = 0.0f;
	public float lerpFFTSpeed = 1.0f;
	public Renderer fftRenderer;
	public UnityFFTSource unityFFTSource;
	public bool usingUnityFFT = false;


	void Start () 
	{
		fftRenderer = GetComponent<Renderer>();
		originalScale = transform.localScale;
		SetMaterial();
		fftLight = GetComponent<Light>();
		if(fftLight != null)
		{
			originalIntensity = fftLight.intensity;
			originalColor = fftLight.color;
		}

		if(unityFFTSource != null)
		{
			usingUnityFFT = true;
		}
	}


	public void SetMaterial()
	{
		if(fftRenderer == null)
		{
			return;
		}

		if(type == ReactFFTType.COLOR_ADD || type == ReactFFTType.COLOR_MOD)
		{
			if(sharedMaterial == false)
			{
				if(materialIndex == -1)
				{
					fftMaterial = fftRenderer.material;
				}
				else
				{
					fftMaterial = fftRenderer.materials[materialIndex];
				}
			}
			else
			{
				if(materialIndex == -1)
				{
					fftMaterial = fftRenderer.sharedMaterial;
				}
				else
				{
					fftMaterial = fftRenderer.sharedMaterials[materialIndex];
				}
			}

			originalColor = fftMaterial.GetColor(colorName);
		}
	}


	void OnDisable()
	{
		if(fftMaterial == null)
		{
			SetMaterial();
		}

		if(sharedMaterial == true)
		{
			fftMaterial.SetColor(colorName, originalColor);
		}
	}


	void Update () 
	{
		if(fftMaterial == null)
		{
			SetMaterial();
		}

		if(usingUnityFFT == false)
		{
			if(spectrumType == ReactFFTSpectrum.SUM)
			{
				bassFFTAmount = bassM.bassDataSum;
			}
			else if(spectrumType == ReactFFTSpectrum.BASS)
			{
				bassFFTAmount = bassM.bassFFTBass;
			}
			else if(spectrumType == ReactFFTSpectrum.MID)
			{
				bassFFTAmount = bassM.bassFFTMid;
			}
			else if(spectrumType == ReactFFTSpectrum.TREBLE)
			{
				bassFFTAmount = bassM.bassFFTTreble;
			}
		}
		else
		{
			if(spectrumType == ReactFFTSpectrum.SUM)
			{
				bassFFTAmount = unityFFTSource.fftSum;
			}
			else if(spectrumType == ReactFFTSpectrum.BASS)
			{
				bassFFTAmount = unityFFTSource.fftBass;
			}
			else if(spectrumType == ReactFFTSpectrum.MID)
			{
				bassFFTAmount = unityFFTSource.fftMid;
			}
			else if(spectrumType == ReactFFTSpectrum.TREBLE)
			{
				bassFFTAmount = unityFFTSource.fftTreble;
			}
		}

		if(lerpFFT == true)
		{
			lerpFFTValue = Mathf.Lerp(lerpFFTValue, bassFFTAmount, Time.deltaTime * lerpFFTSpeed);
			bassFFTAmount = lerpFFTValue;
		}


		if(type == ReactFFTType.COLOR_ADD)
		{
			fftMaterial.SetColor(colorName, originalColor + (originalColor * bassFFTAmount * colorMod));
		}

		if(type == ReactFFTType.COLOR_MOD)
		{
			fftMaterial.SetColor(colorName, (originalColor * bassFFTAmount * colorMod));
		}

		if(type == ReactFFTType.LIGHT_INTENSITY_ADD)
		{
			fftLight.intensity = originalIntensity + (bassFFTAmount * intensityScale);
		}

		if (type == ReactFFTType.SCALE_ADD) 
		{
			transform.localScale = originalScale + (scaleValue * bassFFTAmount * intensityScale);
		}
	}
}
