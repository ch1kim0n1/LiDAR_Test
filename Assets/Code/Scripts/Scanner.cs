using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Scripts.Player
{
    public class Scanner : MonoBehaviour
    {
        [Header("Scan Parameters")]
        [SerializeField] private Transform _scanPoint;  // The point from where the raycasts will be casted.
        [SerializeField] private int _raycastsPerFixedUpdate = 10;  // The number of raycasts to be performed per FixedUpdate.
        [SerializeField] private LayerMask _scanLayerMask = new LayerMask();  // The layer mask to filter which objects should be scanned.
        [SerializeField] private int _scanDispersion = 150;  // The initial dispersion of the raycasts.
        [SerializeField] private int _maxDispersion = 300;  // The maximum dispersion of the raycasts.
        [SerializeField] private int _minDispersion = 50;  // The minimum dispersion of the raycasts.
        [SerializeField] private int _dispersionChangeStep = 10;  // The step size for changing the dispersion of the raycasts.

        [SerializeField] private float _mins = 0.75f;  // Minimum value.

        private Transform _cameraTransform;  // Reference to the main camera's transform.

        private readonly List<PaintableSurface> _contactedSurfacesPerFrame = new List<PaintableSurface>();  // List to store the surfaces that were contacted by the raycasts in the current frame.

        private void Start()
        {
            _cameraTransform = Camera.main.transform;  // Get the transform of the main camera.
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) == false)  // Check if the left mouse button is not pressed. (DEBUG ONLY, TODO: SWAP TO NEW INPUT SYSTEM)
                return;

            PaintSpray();  // Perform the paint spray operation.
        }

        private void PaintSpray()
        {
            for (var i = 0; i < _raycastsPerFixedUpdate; i++)  // Perform multiple raycasts per FixedUpdate.
                PaintOnePoint();  // Perform a single raycast and paint the surface if hit.

            ApplyChangesOnSurfaces();  // Apply the changes made to the surfaces (e.g., updating textures).
        }

        private void PaintOnePoint()
        {
            if (Physics.Raycast(_cameraTransform.position, GetDispersedVector(), out var hit, maxDistance: Mathf.Infinity, layerMask: _scanLayerMask) == false)
                return;  // If the raycast does not hit anything, return.

            if (!hit.collider.TryGetComponent(out PaintableSurface surface))
                return;  // If the hit object does not have a PaintableSurface component, return.

            surface.DrawPixelOnRaycastHit(hit);  // Paint a pixel on the surface at the hit point.

            if (_contactedSurfacesPerFrame.Contains(surface) == false)
                _contactedSurfacesPerFrame.Add(surface);  // Add the surface to the list of contacted surfaces for this frame.
        }

        private void ApplyChangesOnSurfaces()
        {
            foreach (var surface in _contactedSurfacesPerFrame)
                surface.ApplyTextureChanges();  // Apply the accumulated texture changes to each contacted surface.

            _contactedSurfacesPerFrame.Clear();  // Clear the list of contacted surfaces for the next frame.
        }

        private Vector3 GetDispersedVector()
        {
            var direction = _cameraTransform.forward;  // Get the forward direction of the camera.

            direction += Quaternion.AngleAxis(Random.Range(0, 360), _cameraTransform.forward) * _cameraTransform.up * Random.Range(0, _scanDispersion / 360f);  // Apply random dispersion to the direction.

            return direction;  // Return the dispersed vector.
        }

        private void DecreaseScanRadius()
        {
            ChangeScanRadius(-_dispersionChangeStep);  // Decrease the scan dispersion by the specified step size.
        }

        private void IncreaseScanRadius()
        {
            ChangeScanRadius(_dispersionChangeStep);  // Increase the scan dispersion by the specified step size.
        }

        private void ChangeScanRadius(int amount)
        {
            _scanDispersion += amount;  // Change the scan dispersion by the specified amount.
            _scanDispersion = Mathf.Clamp(_scanDispersion, _minDispersion, _maxDispersion);  // Clamp the scan dispersion between the minimum and maximum values.
        }
    }
}
