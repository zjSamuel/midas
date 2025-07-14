using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 10f;
    public float shiftMultiplier = 2f;

    [Header("��ת����")]
    public float rotationSpeed = 5f;

    [Header("��������")]
    public float zoomSpeed = 10f;
    public float minZoomDistance = 1f;
    public float maxZoomDistance = 100f;

    [Header("�۽�����")]
    public float focusSpeed = 5f;
    public float focusDistance = 5f;

    private Vector3 lastMousePosition;
    private bool isRotating;
    private bool isPanning;

    void Update()
    {
        HandleKeyboardMovement();
        //HandleMouseRotation();
        HandleMousePan();
        HandleZoom();
        HandleFocus();
    }

    void HandleKeyboardMovement()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f) * Time.deltaTime;

        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.forward * speed);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.back * speed);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * speed);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * speed);
        if (Input.GetKey(KeyCode.Q)) transform.Translate(Vector3.down * speed);
        if (Input.GetKey(KeyCode.E)) transform.Translate(Vector3.up * speed);
    }

    void HandleMouseRotation()
    {
        if (Input.GetMouseButtonDown(1)) // �Ҽ���ʼ��ת
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1)) isRotating = false;

        if (isRotating)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            transform.RotateAround(transform.position, Vector3.up, delta.x * rotationSpeed);
            transform.RotateAround(transform.position, transform.right, -delta.y * rotationSpeed);

            // ����Z����תΪ0
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.z = 0;
            transform.eulerAngles = eulerAngles;
        }
    }

    void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(2)) // �м���ʼƽ��
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2)) isPanning = false;

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // ���ݵ�ǰ�ӽǼ���ƽ�Ʒ���
            transform.Translate(-delta.x * 0.01f, -delta.y * 0.01f, 0, Space.Self);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float distance = Vector3.Distance(transform.position, transform.position + transform.forward);
            float zoomAmount = scroll * zoomSpeed * distance;

            // �������ŷ�Χ
            if (distance - zoomAmount < minZoomDistance) zoomAmount = distance - minZoomDistance;
            if (distance - zoomAmount > maxZoomDistance) zoomAmount = distance - maxZoomDistance;

            transform.Translate(Vector3.forward * zoomAmount, Space.Self);
        }
    }

    void HandleFocus()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                StartCoroutine(FocusOnObject(hit.transform));
            }
        }
    }

    IEnumerator FocusOnObject(Transform target)
    {
        Vector3 targetPosition = target.position;
        Vector3 direction = (transform.position - targetPosition).normalized;
        Vector3 desiredPosition = targetPosition + direction * focusDistance;

        float t = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion desiredRotation = Quaternion.LookRotation(targetPosition - desiredPosition);

        while (t < 1)
        {
            t += Time.deltaTime * focusSpeed;
            transform.position = Vector3.Lerp(startPosition, desiredPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, t);
            yield return null;
        }
    }
}