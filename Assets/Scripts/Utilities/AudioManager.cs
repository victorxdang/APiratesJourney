/*****************************************************************************************************************
 - AudioManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class handles playing background music and sound effect clips. Sound effects are not currently in use,
     but there is method that will play clips, see PlayClipSE() in the Sound Effects region. PlayClipSE() is a
     private method. In order to use it, write public helper methods that will call PlayClipSE(). There will 
     only be one instance of this class.
*****************************************************************************************************************/

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    #region Fields

    [SerializeField] AudioSource sourceSinking;

    [SerializeField] AudioClip seCannonFiring,
                               seCannonballHit,
                               seShipColliding,
                               seUIClick;

    public static AudioManager Instance { get; private set; }

    // cache variables
    AudioClip nextClip;

    #endregion


    #region Unity Built-In Functions

    /// <summary>
    /// Called when the game object is enabled.
    /// </summary>
    void Awake()
    {
        // this class will persist through scene transitions
        if (FindObjectsOfType<AudioManager>().Length == 1)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    #region Sound Effects

    /// <summary>
    /// Play the sound effect for cannons firing.
    /// </summary>
    /// <param name="volume"></param>
    public void PlayCannonFiring()
    {
        PlayClipSE(seCannonFiring, 1);
    }

    /// <summary>
    /// Play the sound effect for ships colliding.
    /// </summary>
    /// <param name="volume"></param>
    public void PlayShipColliding()
    {
        PlayClipSE(seShipColliding, 1);
    }

    /// <summary>
    /// Play the sound effect for cannonballs hitting a ship.
    /// </summary>
    /// <param name="volume"></param>
    public void PlayCannonballHit()
    {
        PlayClipSE(seCannonballHit, 0.5f);
    }

    /// <summary>
    /// Play the sound effect for clicking on buttons on the UI.
    /// </summary>
    /// <param name="volume"></param>
    public void PlayUIClick()
    {
        PlayClipSE(seUIClick, 3);
    }

    /// <summary>
    /// Plays a specified audio clip with the specified volume. Create public helper methods to call this function
    /// when a specific sound effect is needed to be played.
    /// </summary>
    /// 
    /// <param name="seClip"> The clip to be played </param>
    /// <param name="volume"> How loud the clip will be played. </param>
    void PlayClipSE(AudioClip seClip, float volume)
    {
        if (seClip)
            sourceSinking.PlayOneShot(seClip, volume);
    }

    #endregion


    #region Ship Sinking 

    /// <summary>
    /// Plays ship sinking sound effect. Implmented this way so that the sinking sound
    /// effect can be stopped after the player decides to play again.
    /// </summary>
    public void PlayShipSinkingSE()
    {
        sourceSinking.Play();
    }

    /// <summary>
    /// Stops the player ship sinking sound effect only if the sound effect is currently
    /// playing.
    /// </summary>
    public void StopShipSinkingSE()
    {
        if (sourceSinking.isPlaying)
            sourceSinking.Stop();
    }

    #endregion
}
