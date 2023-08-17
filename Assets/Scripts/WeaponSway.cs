using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponSway : MonoBehaviour
{
    public float swaySensitivity = 0.02f;
    public float swayClamp = 20f;
    public float swaySmoothness = 20f;
    
    private Vector3 startPosition;
    private Vector3 nextPosition;
    private Vector3 currentVelocity = Vector3.zero;

    public PhotonView photonView;

    void Start()
    {
        startPosition = transform.localPosition;
    }
    
    void Update()
    {
        //para evitar que mi input afecte a los scripts de los otros jugadores o al revés, tenemos que hacer esta comprobación:
        if (PhotonNetwork.InRoom && !photonView.IsMine) { return; }

        float mouseX = Input.GetAxis("Mouse X") * swaySensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * swaySensitivity * Time.deltaTime;

        mouseX = Mathf.Clamp(mouseX, -swayClamp, swayClamp);
        mouseY = Mathf.Clamp(mouseY, -swayClamp, swayClamp);

        nextPosition = new Vector3(mouseX, mouseY, 0);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, nextPosition + startPosition,
            ref currentVelocity, swaySmoothness * Time.deltaTime);
    }
}
