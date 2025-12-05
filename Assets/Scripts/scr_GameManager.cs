using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scr_GameMaganer : MonoBehaviour
{
   #region 3dPlataformasMenu
   /* private bool juegoPausado, gameOver;
    private string playerName;
    internal int scenaActual;
    
    void Start()
    {
        scenaActual = SceneManager.GetActiveScene().buildIndex;
        juegoPausado = false;
        gameOver = false;
        playerName = "Player";
        Player = GameObject.Find(playerName);

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            juegoPausado = !juegoPausado;

            if(juegoPausado)
            {
                resumeGame();
            }
            else
            {
                pauseGame();
            }

        }

        if (Player.GetComponent<scr_Player>().playerisDead)
        {
            gameOver = true;
            Debug.Log("PlayerisDEAD");
        }
        else
        {
            gameOver = false;
        }

        if (gameOver)
        {
            gameOverFuncion();
        }




/*
        if(scenaActual == 2)
        {
            startGame_002();
        }
        if (scenaActual == 3)
        {
            startGame_003();
        }

    }

    public void startGame()
    {
        SceneManager.LoadScene("level_001");
    }

    public void nextLevel()
{
    int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

    // Verifica que la siguiente escena exista en Build Settings
    if (nextScene < SceneManager.sceneCountInBuildSettings)
    {
        Debug.Log("Cargando siguiente escena: " + nextScene);
        SceneManager.LoadScene(nextScene);
    }
    else
    {
        Debug.Log("No hay más niveles disponibles. Regresando al menú principal.");
        SceneManager.LoadScene("Main_Menu");
    }
}


/*
    public void startGame_002()
    {
        SceneManager.LoadScene("level_002");
    }
    public void startGame_003()
    {
        SceneManager.LoadScene("level_002");
    } */
/*
    public void StartBlueBox()
    {
        SceneManager.LoadScene("Blue_Box");
        
    }

    public void ExitGame()
    {
        Debug.Log("Juego QUIT");
        Application.Quit();
    }

    public void startMainMenu()
    {
        SceneManager.LoadScene("Main_Menu");
        menuGameOver.SetActive(false);
    }



    public void resumeGame()
    {
        menuPausa.SetActive(false);
        Time.timeScale = 1;
        
    }
    public void pauseGame()
    {
        menuPausa.SetActive(true);
        Time.timeScale = 0;
    }

    public void gameOverFuncion()
    {
        menuGameOver.SetActive(true);
    }
*/
#endregion




}//END CLASS
