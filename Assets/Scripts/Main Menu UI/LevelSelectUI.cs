/*****************************************************************************************************************
 - LevelSelectUI.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Parent class that contains all of the common methods that will be shared across all UI Managers.
*****************************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : UIUtilities
{
    #region Constants

    /// <summary>
    /// This is the amount of level select buttons that can appear on one page. The buttons are spaced out and 
    /// and formatted by a Grid Layout Group component.
    /// </summary>
    const int MAX_BUTTON_PER_PAGE = 20;

    /// <summary>
    /// How fast to change a page when you either click on the left or right arrow or when you swipe in the 
    /// repsective direction.
    /// </summary>
    const float PAGE_TRANSITION_SPEED = 3;

    #endregion


    #region Fields

    [SerializeField] RectTransform prefabLevelPage;
    [SerializeField] LevelButton prefabButton;
    [SerializeField] Indicator prefabPageIndicator;

    [SerializeField] Button buttonBack,
                            buttonPreviousPage,
                            buttonNextPage;

    bool touchStarted = false;

    // keep track of the page that the player starts off on, starting page is the page that contains the level
    // that the player is currently on
    int startingPage = 0, 
        currentPage = 0;

    Touch touch;
    Vector2 startPos;

    List<RectTransform> list_pages = new List<RectTransform>();
    List<Indicator> list_indicators = new List<Indicator>();

    #endregion


    #region Built-In Methods

    /// <summary>
    /// Creates the pages that will hold all of the level select buttons and sets up the buttons appropriately.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        SetupPages();

        // makes sure that the left arrow doesn't appear if the player is at the first page, likewise for the 
        // right arrow
        CheckButtons(); 

        System.GC.Collect();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Set touchStarted to false whenever this object is enabled again in order to not have the level select
    /// pages switch the next page whenever the player opens up the level select page.
    /// </summary>
    void OnEnable()
    {
        touchStarted = false;
    }

    #endregion


    #region Update Manager

    /// <summary>
    /// Checks for player swipe so that the page can change if the player has done so.
    /// </summary>
    public override void UpdateMe()
    {
        base.UpdateMe();

        #if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                touchStarted = true;
                startPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0) && touchStarted)
            {
                RegisterSwipe(startPos, Input.mousePosition);
            }
        #else
            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
            
                if (touch.phase == TouchPhase.Began)
                {
                    touchStarted = true;
                    startPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended && touchStarted)
                {
                    RegisterSwipe(startPos, touch.position);
                }
            }
        #endif
    }

    #endregion


    #region Buttons/Swipe

    /// <summary>
    /// Registers a touch as a swipe if the player has moved the finger and the distance from the point where the 
    /// player began touching to the point where the player lifted the finger off of the screen. The distance 
    /// between these two point (only taking to consideration the x-coordinate) should be greater than 25 pixels.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    void RegisterSwipe(Vector2 start, Vector2 end)
    {
        if (end.x - start.x < -25)
            NextPage();
        else if (end.x - start.x > 25)
            PreviousPage();
    }

    /// <summary>
    /// Switches to the next page (if available) and update the indicator to reflect which page the player is 
    /// currently on.
    /// </summary>
    public void NextPage()
    {
        if (currentPage + 1 < list_pages.Count)
        {
            currentPage++;
            StartCoroutine(IEnumTransitionPages(false, list_pages[currentPage - 1], list_pages[currentPage]));

            list_indicators[currentPage - 1].HideDot();
            list_indicators[currentPage].ShowDot();
        }
    }

    /// <summary>
    /// Switches to the previous page (if available) and update the indicator to reflect which page the player is 
    /// currently on.
    /// </summary>
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            StartCoroutine(IEnumTransitionPages(true, list_pages[currentPage + 1], list_pages[currentPage]));

            list_indicators[currentPage + 1].HideDot();
            list_indicators[currentPage].ShowDot();
        }
    }

    /// <summary>
    /// Once the player has pressed the back button, this method will revert the pages back to the original position
    /// in which they spawned in. That means, if the player initially started on page 2 of the level select menu and 
    /// switched to page 4 then returned to the start menu, then once the player presses on the level select menu,
    /// they will begin at page 2 again. The other pages will be moved to their original position. The positions 
    /// are as follows:
    /// 
    /// - The page the player is currently on: x = 0
    /// - Any page before the current page: x = -850
    /// - Any page after the current page: x = 850
    /// </summary>
    public override void Back()
    {
        for (int i = 0; i < list_pages.Count; i++)
        {
            if (i < startingPage)
                list_pages[i].anchoredPosition = new Vector2(-850, list_pages[i].anchoredPosition.y);
            else if (i > startingPage)
                list_pages[i].anchoredPosition = new Vector2(850, list_pages[i].anchoredPosition.y);
            else
                list_pages[i].anchoredPosition = new Vector2(0, list_pages[i].anchoredPosition.y);

            list_pages[i].gameObject.SetActive(i == startingPage);   
        }

        list_indicators[currentPage].UpdateDot(false);
        list_indicators[startingPage].UpdateDot(true);

        // revert currentPage back to startingPage so that the player will start on the starting page when they
        // open up the level select menu again
        currentPage = startingPage;

        CheckButtons();
        base.Back();
    }

    /// <summary>
    /// Checks to make sure that the left arrow doesn't appear if there is no pages before the current page. Likewise,
    /// this method will make sure that the right arrow doesn't appear if there are no pages after the current page.
    /// </summary>
    void CheckButtons()
    {
        buttonNextPage.gameObject.SetActive(currentPage + 1 < list_pages.Count);
        buttonPreviousPage.gameObject.SetActive(currentPage > 0);
    }

    #endregion


    #region Utilties

    /// <summary>
    /// Creates all of the level select buttons and pages that will hold the buttons.
    /// </summary>
    void SetupPages()
    {
        bool setPage = false;
        int level;
        int maxLevel = 1 + ((MainMenuUIManager.Instance.UnlockAllLevels) ? Scaling.max_levels : SaveManager.PlayerSaveData.highest_level);
        int maxPages = Scaling.max_levels / MAX_BUTTON_PER_PAGE;
        Indicator indicator = null;
        RectTransform page = null;
        LevelButton levelButton;
        Transform pageHolder = transform.GetChild(0);
        Transform indicatorHolder = transform.GetChild(1);

        // loop for maxPages amount of times, maxPage is determined as the maximum amount of levels specified within the scaling class divided by
        // the maximum amount of buttons per page (default value is 20).
        for (int i = 0; i < maxPages; i++)
        {
            // create a page
            page = Instantiate(prefabLevelPage, pageHolder);
            page.anchoredPosition = new Vector2((setPage) ? 850 : -850, 50);
            page.gameObject.SetActive(false);
            list_pages.Add(page);

            // create an indicator on the bottom fo the screen
            indicator = Instantiate(prefabPageIndicator, indicatorHolder);
            indicator.TransitionSpeed = PAGE_TRANSITION_SPEED;
            indicator.UpdateDot(false);
            list_indicators.Add(indicator);

            // loop for the maximum amount of buttons per page
            for (int j = 0; j < MAX_BUTTON_PER_PAGE; j++)
            {
                // instaniate the button and update the text of the button to display the level that it will hold
                level = (j + 1) + (i * MAX_BUTTON_PER_PAGE);
                levelButton = Instantiate(prefabButton, list_pages[i]);
                levelButton.SetupButton(level, maxLevel);

                // if this page contains the level that the player has played to, then set the starting page to this page
                if (level == maxLevel)
                {
                    SetPageActive(i, page, indicator);
                    setPage = true;
                }
            }
        }

        // active the last page if the player has completed all of the levels
        if (!setPage)
            SetPageActive(maxPages - 1, page, indicator);
    }

    /// <summary>
    /// Activates the specified page at pageNumber.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="page"></param>
    /// <param name="indicator"></param>
    void SetPageActive(int pageNumber, RectTransform page, Indicator indicator)
    {
        page.anchoredPosition = new Vector2(0, 50);
        page.gameObject.SetActive(true);
        indicator.UpdateDot(true);

        currentPage = pageNumber;
        startingPage = pageNumber;
    }

    /// <summary>
    /// The coroutine responsible for making the pages transition smoothly from out of screen to in view or in view to out of screen.
    /// </summary>
    /// <param name="transitionLeft"></param>
    /// <param name="outgoingMenu"></param>
    /// <param name="incomingMenu"></param>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumTransitionPages(bool transitionLeft, RectTransform outgoingMenu, RectTransform incomingMenu)
    {
        float time = 0;

        // determine the position of the pages based on the direction of the page movement
        Vector2 currentOutgoingPos = outgoingMenu.anchoredPosition,
                newOutgoingPos = new Vector2(outgoingMenu.anchoredPosition.x + ((transitionLeft) ? 850 : -850), outgoingMenu.anchoredPosition.y);
        Vector2 currentIncomingPos = incomingMenu.anchoredPosition,
                newIncomingPos = new Vector2(incomingMenu.anchoredPosition.x + ((transitionLeft) ? 850 : -850), incomingMenu.anchoredPosition.y);

        // deactivate the buttons when transitioning screens so that it doesn't look odd and activate the menu coming into view
        buttonBack.gameObject.SetActive(false);
        buttonNextPage.gameObject.SetActive(false);
        buttonPreviousPage.gameObject.SetActive(false);
        incomingMenu.gameObject.SetActive(true);

        while (time <= 1)
        {
            outgoingMenu.anchoredPosition = Vector2.Lerp(currentOutgoingPos, newOutgoingPos, time);
            incomingMenu.anchoredPosition = Vector2.Lerp(currentIncomingPos, newIncomingPos, time);
            time += Time.unscaledDeltaTime * PAGE_TRANSITION_SPEED;

            yield return null;
        }

        // set the position exactly to calculated position since the while loop above may not have it exactly where 
        // it is supposed to be
        outgoingMenu.anchoredPosition = newOutgoingPos;
        incomingMenu.anchoredPosition = newIncomingPos;

        // deactivate the menu transitioning out of screen and have the buttons appear again
        outgoingMenu.gameObject.SetActive(false);
        buttonBack.gameObject.SetActive(true);
        CheckButtons();
    }

    #endregion


    #region Button Logic

    /// <summary>
    /// Closes the level select menu.
    /// </summary>
    public void CloseLevelSelect()
    {
        AudioManager.Instance.PlayUIClick();
        Back();
    }

    /// <summary>
    /// Switches to the next page when the right arrow button is clicked.
    /// </summary>
    public void ClickRight()
    {
        AudioManager.Instance.PlayUIClick();
        NextPage();
    }

    /// <summary>
    /// Switches to the next page when the left arrow button is clicked.
    /// </summary>
    public void ClickLeft()
    {
        AudioManager.Instance.PlayUIClick();
        PreviousPage();
    }

    #endregion
}
