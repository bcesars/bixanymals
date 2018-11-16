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

using UnityEngine;

/// <summary>
/// Implements the rendering of the rope through a line renderer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpringJoint2D))]
[RequireComponent(typeof(LineRenderer))]
public class RopeController: MonoBehaviour
{
    /// <summary>
    /// Reference to the target of the line renderer.
    /// </summary>
    private Rigidbody2D target;

    /// <summary>
    /// Public property to access the target attribute.
    /// </summary>
    public Rigidbody2D Target
    {
        get
        {
            return target;
        }

        set
        {
            target = value;
            springJoint.connectedBody = target;
        }
    }

    /// <summary>
    /// Reference to the spring joint used to simulate the rope.
    /// </summary>
    private SpringJoint2D springJoint;

    /// <summary>
    /// Line renderer instance used to draw the rope.
    /// </summary>
    private LineRenderer lineRenderer;

    /// <summary>
    /// Captures the start event to initialize the used references.
    /// </summary>
    void Start()
    {
        springJoint = GetComponent<SpringJoint2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    /// <summary>
    /// Captures the last update event, so the rope can be rendered (i.e. the
    /// line render positions can be updated as the targets positions did)
    /// after all physics calculations have been performed.
    /// </summary>
    void LateUpdate()
    {
        lineRenderer.SetPosition(0, transform.position);
        if(target)
        {
            lineRenderer.SetPosition(1, target.transform.position);
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.SetPosition(1, transform.position);
            lineRenderer.enabled = false;
        }
    }
}