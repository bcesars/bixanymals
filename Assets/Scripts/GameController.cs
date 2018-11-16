/*
 * Copyright (c) 2018, Luiz Carlos Vieira (http://www.luiz.vieira.nom.br)
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller of the game actions.
/// </summary>
public class GameController : MonoBehaviour
{
    /// <summary>
    /// Collider of the spawning area over the background image, where new animals
    /// are instantiated upon touching.
    /// Configurable in the editor.
    /// </summary>
    public BoxCollider2D spawningArea;

    /// <summary>
    /// Reference to the spring joing used to drag animals
    /// </summary>
    public RopeController rope;

    /// <summary>
    /// List of the animal prefabs for instantiation;
    /// Configurable in the editor.
    /// </summary>
    public List<GameObject> animalPrefabs;

    /// <summary>
    /// Elapsed time since the start of a touch, in seconds.
    /// </summary>
    private float touchingElapsedTime;

    /// <summary>
    /// Position of the touch input.
    /// </summary>
    private Vector2 touchPosition;

    /// <summary>
    /// 
    /// </summary>
    private bool touchingSpawingArea;

    /// <summary>
    /// Reference to the animal being dragged.
    /// </summary>
    private AnimalController animalDragged;

    /// <summary>
    /// Minimum touching time in seconds for a drag of an animal to start.
    /// Configurable in the editor.
    /// </summary>
    public float draggingStartTime = 0.5f;

    public List<Button> buttons;
    public Canvas canvas;
    private RectTransform rect;
    private List<RectTransform> listRects;

    /// <summary>
    /// Captures the game start event to lock the screen orientation in
    /// landscape and enable the gyroscope.
    /// </summary>
    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        if (SystemInfo.supportsGyroscope)
            Input.gyro.enabled = true;

        int i = 3;
        foreach (Button botao in buttons)
        {
            botao.onClick.AddListener(() => ButtonClicked(i));
            i++;
        }
    }

    /// <summary>
    /// Captures the game update events to instantiate animals.
    /// </summary>
    private void Update()
    {
        if (TouchInput.IsTouchDown())
        {
            touchingElapsedTime = 0;
            touchPosition = TouchInput.GetTouchPosition();
            touchingSpawingArea = TouchInput.IsTouchingCollider(spawningArea, LayerMask.GetMask("Background"));
            animalDragged = GetTouchedAnimal();
            if (animalDragged)
            {
                animalDragged.isBeingDragged = true;
                animalDragged.GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
        }
        else if (TouchInput.IsTouching())
        {
            touchingElapsedTime += Time.deltaTime;
            touchPosition = TouchInput.GetTouchPosition();
            touchingSpawingArea = TouchInput.IsTouchingCollider(spawningArea, LayerMask.GetMask("Background"));

            if (!rope.Target && animalDragged && touchingElapsedTime >= draggingStartTime)
                rope.Target = animalDragged.GetComponent<Rigidbody2D>();

            rope.transform.position = Camera.main.ScreenToWorldPoint(touchPosition);


        }
        else if (TouchInput.IsTouchUp())
        {
            if (rope.Target)
                rope.Target = null;
            if (animalDragged)
            {
                animalDragged.isBeingDragged = false;
                animalDragged.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }

            if (touchingSpawingArea && (!animalDragged || touchingElapsedTime < draggingStartTime))
                SpawnAnimal();
        }
    }

    /// <summary>
    /// Gets the animal being touched.
    /// </summary>
    /// <returns>Returns the reference to the AnimalController of the animal
    /// being touched, or null if no animal has been touched.</returns>
    private AnimalController GetTouchedAnimal()
    {
        foreach (Transform child in transform)
        {
            AnimalController animal = child.GetComponent<AnimalController>();
            if (animal && TouchInput.IsTouchingCollider(animal.GetComponent<PolygonCollider2D>(), LayerMask.GetMask("Animals")))
                return animal;
        }
        return null;
    }

    /// <summary>
    /// Spawns a random animal at the touched position inside the spawning area.
    /// </summary>
    private void SpawnAnimal()
    {
        int idx = Random.Range(0, animalPrefabs.Count - 1);
        GameObject animal = Instantiate(animalPrefabs[idx]);
        Vector3 pos = Camera.main.ScreenToWorldPoint(touchPosition);
        pos.z = 0;
        animal.transform.position = pos;
        animal.transform.parent = transform;
    }

    void ButtonClicked(int index)
    {
        GameObject animal = Instantiate(animalPrefabs[index]);
        Vector3 pos = Camera.main.ScreenToWorldPoint(touchPosition);
        pos.z = 0;
        animal.transform.position = pos;
        animal.transform.parent = transform;
    }

    private void OnEnable()
    {

        listRects = new List<RectTransform>();

        for (int i = 0; i < buttons.Count; i++)
        {
            listRects.Add(buttons[i].GetComponent<RectTransform>());
        }

        DeviceWatcher watcher = GetComponent<DeviceWatcher>();
        watcher.AddResolutionListener(OnResolutionChanged);
        watcher.AddOrientationListener(OnOrientationChanged);
    }

    private void OnDisable()
    {
        DeviceWatcher watcher = GetComponent<DeviceWatcher>();
        watcher.RemoveResolutionListener(OnResolutionChanged);
        watcher.RemoveOrientationListener(OnOrientationChanged);
    }

    private void OnResolutionChanged(Vector2 resolution, float aspectRatio)
    {
        Debug.Log("Resolution changed to: " + resolution + " (" + aspectRatio + ")"); //1.77 

    }

    private void OnOrientationChanged(DeviceOrientation orientation)
    {
        Debug.Log("Orientation changed to: " + orientation);
        Screen.orientation = (ScreenOrientation)orientation;


        float PosX = -211.0f;
        float PosY = -35.0f;
        if (orientation == DeviceOrientation.Portrait)
        {
            foreach (var item in listRects)
            {
                item.anchoredPosition = new Vector2(PosX, PosY);
                PosY += -item.rect.height;
            }
        }
        else
        {
            foreach (var item in listRects)
            {
                item.anchoredPosition = new Vector2(PosX, PosY);
                PosY += item.rect.height;
            }

        }
    }

}
