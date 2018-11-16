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

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Custom class to represent a resolution changed event.
/// Listeners to this eventg should expect two arguments:
/// 
///     resolution (Vector2): the current resolution of the device, width in the vector2.x and height
///     in the vector2.y. It will be the same value as Screen.width and Screen.height, but this is used
///     just to make the access to the resolution quicker.
///     
///     aspectRatio (float): the aspect ratio (width / height) of the screen, with two decimal precision.
///     For instance, a screen that is 4:3 (standard TV) has an aspect ratio of 1.33, while a screen that
///     is 16:9 (widescreen TV) has an aspect ratio of 1.77.
/// </summary>
public class ResolutionChangedEvent: UnityEvent<Vector2, float> { };

/// <summary>
/// Custom class to represent an orientation changed event.
/// Listeners to this eventg should expect one argument:
/// 
///     orientation (DeviceOrientation): the orientation of the device, that is applicable to the
///     screen rotation.
/// </summary>
public class OrientationChangedEvent: UnityEvent<DeviceOrientation> { };

/// <summary>
/// Implements a watcher for changes in the device (such as resolution and orientation).
/// Usage: simply add this script to any game object and then listen to the change events
/// in your own code.
/// Important: This DOES NOT change the screen orientation to follow the device orientation. It simply
/// notifies the changes in the device orientation and it is up to you to decide what to do in your code.
/// </summary>
public class DeviceWatcher: MonoBehaviour
{
    /// <summary>
    /// Interval in seconds for the checking to be performed.
    /// </summary>
    public float checkInterval = 0.5f;

    /// <summary>
    /// Event used to delivery notifications on resolution changes.
    /// </summary>
    private ResolutionChangedEvent resolutionChanged = new ResolutionChangedEvent();

    /// <summary>
    /// Event used to delivery notifications on orientation changes.
    /// </summary>
    private OrientationChangedEvent orientationChanged = new OrientationChangedEvent();

    /// <summary>
    /// Current resolution of the device.
    /// </summary>
    private Vector2 resolution;

    /// <summary>
    /// Current orientation of the device.
    /// </summary>
    private DeviceOrientation orientation;

    /// <summary>
    /// Reference to the check changes coroutine being executed at the timed interval.
    /// </summary>
    private Coroutine checkCoroutine;
    
    /// <summary>
    /// Adds the given action to listen to resolution changed events.
    /// </summary>
    /// <param name="action">Unity action (expecting a Vector2 and a float arguments) used as
    /// listener.</param>
    public void AddResolutionListener(UnityAction<Vector2, float> action)
    {
        resolutionChanged.AddListener(action);
    }

    /// <summary>
    /// Removes the given action from listening to resolution changed events.
    /// </summary>
    /// <param name="action">Unity action (expecting a Vector2 and a float arguments) used as
    /// listener.</param>
    public void RemoveResolutionListener(UnityAction<Vector2, float> action)
    {
        resolutionChanged.RemoveListener(action);
    }

    /// <summary>
    /// Adds the given action to listen to orientation changed events.
    /// </summary>
    /// <param name="action">Unity action (expecting a DeviceOrientation argument) used as listener.</param>
    public void AddOrientationListener(UnityAction<DeviceOrientation> action)
    {
        orientationChanged.AddListener(action);
    }

    /// <summary>
    /// Removes the given action from listening to orientation changed events.
    /// </summary>
    /// <param name="action">Unity action (expecting a DeviceOrientation argument) used as listener.</param>
    public void RemoveOrientationListener(UnityAction<DeviceOrientation> action)
    {
        orientationChanged.RemoveListener(action);
    }

    /// <summary>
    /// Captures the component enable event to start the checking.
    /// </summary>
    private void OnEnable()
    {
        checkCoroutine = StartCoroutine(CheckForChange());
    }

    /// <summary>
    /// Captures the component disable event to stop the checking.
    /// </summary>
    private void OnDisable()
    {
        StopCoroutine(checkCoroutine);
    }

    /// <summary>
    /// Checking coroutine. It keeps running while the component is enabled,
    /// checking for changes in orientation and resolution at the configured
    /// time interval.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckForChange()
    {
        float aspectRatio;

        resolution = new Vector2(Screen.width, Screen.height);
        aspectRatio = (float)Math.Truncate(((double)Screen.width / (double)Screen.height) * 100) / 100;
        orientation = Input.deviceOrientation;

        // Assume the screen orientation in case the device orientation is not applicable
        switch (orientation)
        {
            case DeviceOrientation.Unknown:
            case DeviceOrientation.FaceUp:
            case DeviceOrientation.FaceDown:
                orientation = (DeviceOrientation) Screen.orientation;
                break;

            default:
                break;
        }

        // Do a first notification upon start
        resolutionChanged.Invoke(resolution, aspectRatio);
        orientationChanged.Invoke(orientation);

        while(true)
        {
            // Check for a Resolution Change
            if(resolution.x != Screen.width || resolution.y != Screen.height)
            {
                resolution = new Vector2(Screen.width, Screen.height);
                aspectRatio = (float)Math.Truncate(((double)Screen.width / (double)Screen.height) * 100) / 100;
                resolutionChanged.Invoke(resolution, aspectRatio);
            }

            // Check for an Orientation Change
            switch(Input.deviceOrientation)
            {
                // Ignore device specific orientations (i.e. not usable for the screen rotation)
                case DeviceOrientation.Unknown:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.FaceDown:
                    break;
                
                default:
                    if(orientation != Input.deviceOrientation)
                    {
                        orientation = Input.deviceOrientation;
                        orientationChanged.Invoke(orientation);
                    }
                    break;
            }

            // Wait for the configured amount of time in seconds
            yield return new WaitForSeconds(checkInterval);
        }
    }
}
