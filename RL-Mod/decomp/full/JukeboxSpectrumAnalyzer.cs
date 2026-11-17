using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using UnityEngine;

public class JukeboxSpectrumAnalyzer : MonoBehaviour
{
	[SerializeField]
	private SimpleSpectrum m_spectrumAnalyzer;

	private DSP m_fft;

	private bool m_spectrumInitialized;

	private bool m_isPlaying;

	private void Start()
	{
		InitializeSpectrum();
	}

	private void InitializeSpectrum()
	{
		RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out m_fft);
		m_fft.setParameterInt(1, 3);
		m_fft.setParameterInt(0, 256);
		RuntimeManager.CoreSystem.getMasterChannelGroup(out var channelgroup);
		channelgroup.addDSP(-1, m_fft);
		m_spectrumInitialized = true;
	}

	public void StartSpectrum()
	{
		if (!m_isPlaying)
		{
			m_isPlaying = true;
		}
	}

	public void StopSpectrum()
	{
		if (!m_isPlaying)
		{
			return;
		}
		if ((bool)m_spectrumAnalyzer && m_spectrumInitialized)
		{
			for (int i = 0; i < m_spectrumAnalyzer.spectrumInputData.Length; i++)
			{
				m_spectrumAnalyzer.spectrumInputData[i] = 0f;
			}
		}
		m_isPlaying = false;
	}

	private void Update()
	{
		if ((bool)m_spectrumAnalyzer && m_spectrumInitialized && m_isPlaying)
		{
			m_fft.getParameterData(2, out var data, out var _);
			float[][] spectrum = ((DSP_PARAMETER_FFT)Marshal.PtrToStructure(data, typeof(DSP_PARAMETER_FFT))).spectrum;
			if (spectrum.Length != 0)
			{
				m_spectrumAnalyzer.spectrumInputData = spectrum[0];
			}
		}
	}
}
