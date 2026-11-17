namespace RLAudio;

public class KineticBowProjectileAudioEventEmitter : ProjectileEventEmitter
{
	public void SetPitch(float pitch)
	{
		m_lifeTimeEventInstance.setParameterByName("bardShootNote", pitch);
	}
}
