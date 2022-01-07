/*****************************************************************************************************************
 - MainMenuUIManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This literally contains only the logic for the back button. Was there a need for this? I mean, this could
     have been simply done by just writing a public method and assigning the logic to the button from the editor.
     But this way, if we ever decide to make a fancy credit scene, then this script can be expanded upon and 
     to implement a fancy credits scene!
*****************************************************************************************************************/

public class CreditsUI : UIUtilities
{
    /// <summary>
    /// Opens the webpage for Kenney.
    /// </summary>
    public void OpenKenneyURL()
    {
        AudioManager.Instance.PlayUIClick();
        UnityEngine.Application.OpenURL(Hyperlinks.KENNEY);
    }

    /// <summary>
    /// Opens the webpage for ZapSplat.
    /// </summary>
    public void OpenZapSplatURL()
    {
        AudioManager.Instance.PlayUIClick();
        UnityEngine.Application.OpenURL(Hyperlinks.ZAPSPLAT);
    }

    /// <summary>
    /// Opens the webpage for Shane Ivers.
    /// </summary>
    public void OpenShaneIversURL()
    {
        AudioManager.Instance.PlayUIClick();
        UnityEngine.Application.OpenURL(Hyperlinks.SHANE_IVERS);
    }

    /// <summary>
    /// Closes the credit menu.
    /// </summary>
    public void CloseCredits()
    {
        AudioManager.Instance.PlayUIClick();
        Back();
    }
}
