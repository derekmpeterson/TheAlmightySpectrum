using UnityEngine;
using System.Collections;

public class ScreenShakeController : MonoBehaviour 
{
	Vector3 m_startPos;
	float shakeAmt = 0;

	public void StartShake(float i_magnitude)
	{
		m_startPos = gameObject.transform.position;
		shakeAmt = i_magnitude;
		InvokeRepeating("CameraShake", 0, .01f);
		Invoke("StopShaking", 0.3f);
	}

	void CameraShake()
	{
		if(shakeAmt>0) 
		{
			float quakeAmt = Random.value*shakeAmt*2 - shakeAmt;
			Vector3 pp = gameObject.transform.position;
			pp.y+= quakeAmt; // can also add to x and/or z
			gameObject.transform.position = pp;
		}
	}

	void StopShaking()
	{
		CancelInvoke("CameraShake");
		gameObject.transform.position = m_startPos;
	}
}