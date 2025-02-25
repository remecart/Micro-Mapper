﻿using UnityEngine;
using System;

[ImageEffectAllowedInSceneView]
public class BloomEffect : MonoBehaviour
{

	const int BoxDownPrefilterPass = 0;
	const int BoxDownPass = 1;
	const int BoxUpPass = 2;
	const int ApplyBloomPass = 3;
	const int DebugBloomPass = 4;

	public Shader bloomShader;
	public Shader AlphaHDRShader;

	[Range(0, 10)]
	public float intensity = 0.15f;

	[Range(1, 16)]
	public int iterations = 16;

	[Range(0, 10)]
	public float threshold = 1;

	[Range(0, 1)]
	public float softThreshold = 0.5f;

	public bool debug;

	RenderTexture[] textures = new RenderTexture[16];

	[NonSerialized]
	Material bloom;
	[NonSerialized]
	Material alphaHDR;

	public static BloomEffect instance;

	void Start()
	{
		instance = this;
		intensity = Settings.instance.config.visuals.cameraSettings.bloom / 10f;
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (bloom == null)
		{
			bloom = new Material(bloomShader);
			bloom.hideFlags = HideFlags.HideAndDontSave;
		}
		if (alphaHDR == null)
		{
			alphaHDR = new Material(AlphaHDRShader);
			alphaHDR.hideFlags = HideFlags.HideAndDontSave;
		}

		//Use AlphaHDR
		RenderTexture preSource =
			RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		Graphics.Blit(source, preSource, alphaHDR);

		float knee = threshold * softThreshold;
		Vector4 filter;
		filter.x = threshold;
		filter.y = filter.x - knee;
		filter.z = 2f * knee;
		filter.w = 0.25f / (knee + 0.00001f);
		bloom.SetVector("_Filter", filter);
		bloom.SetFloat("_Intensity", Mathf.GammaToLinearSpace(intensity));

		int width = source.width / 2;
		int height = source.height / 2;
		RenderTextureFormat format = source.format;

		RenderTexture currentDestination = textures[0] =
			RenderTexture.GetTemporary(width, height, 0, format);
		Graphics.Blit(preSource, currentDestination, bloom, BoxDownPrefilterPass);
		RenderTexture currentSource = currentDestination;

		int i = 1;
		for (; i < iterations; i++)
		{
			width /= 2;
			height /= 2;
			if (height < 2)
			{
				break;
			}
			currentDestination = textures[i] =
				RenderTexture.GetTemporary(width, height, 0, format);
			Graphics.Blit(currentSource, currentDestination, bloom, BoxDownPass);
			currentSource = currentDestination;
		}

		for (i -= 2; i >= 0; i--)
		{
			currentDestination = textures[i];
			textures[i] = null;
			Graphics.Blit(currentSource, currentDestination, bloom, BoxUpPass);
			RenderTexture.ReleaseTemporary(currentSource);
			currentSource = currentDestination;
		}

		if (debug)
		{
			Graphics.Blit(currentSource, destination, bloom, DebugBloomPass);
		}
		else
		{
			bloom.SetTexture("_SourceTex", preSource);
			Graphics.Blit(currentSource, destination, bloom, ApplyBloomPass);
		}
		RenderTexture.ReleaseTemporary(preSource);
		RenderTexture.ReleaseTemporary(currentSource);
	}
}